$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = Split-Path $PSScriptRoot -Parent
$stage = Join-Path $root '.showcase'
if (!(Test-Path (Join-Path $stage 'global.json'))) {
    throw 'Run eng/Stage-SdkShowcase.ps1 first.'
}
$sdkVersion = (Get-Content (Join-Path $stage 'global.json') -Raw | ConvertFrom-Json).'msbuild-sdks'.'ActionsToolkit.Sdk'
$nugetConfig = [xml](Get-Content (Join-Path $stage 'NuGet.Config') -Raw)
$packageCache = $nugetConfig.configuration.config.add |
    Where-Object { $_.key -eq 'globalPackagesFolder' } |
    Select-Object -First 1 -ExpandProperty value
if (!$packageCache) { throw 'The staged NuGet.Config must define globalPackagesFolder.' }
$env:NUGET_PACKAGES = [IO.Path]::GetFullPath((Join-Path $stage $packageCache))
$testEnvironment = Join-Path $stage 'test-environment'
New-Item $testEnvironment -ItemType Directory -Force | Out-Null
$env:GITHUB_WORKSPACE = $root
$env:GITHUB_OUTPUT = Join-Path $testEnvironment 'output'
$env:GITHUB_ENV = Join-Path $testEnvironment 'environment'
$env:GITHUB_PATH = Join-Path $testEnvironment 'path'
$env:GITHUB_STATE = Join-Path $testEnvironment 'state'
$env:GITHUB_STEP_SUMMARY = Join-Path $testEnvironment 'summary'
foreach ($path in @($env:GITHUB_OUTPUT, $env:GITHUB_ENV, $env:GITHUB_PATH, $env:GITHUB_STATE, $env:GITHUB_STEP_SUMMARY)) {
    Set-Content $path '' -NoNewline
}

function Read-Outputs {
    $values = @{}
    $lines = @(Get-Content $env:GITHUB_OUTPUT)
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^([^<]+)<<(.+)$') {
            $name = $Matches[1]
            $delimiter = $Matches[2]
            $content = [Collections.Generic.List[string]]::new()
            $i++
            while ($i -lt $lines.Count -and $lines[$i] -ne $delimiter) {
                $content.Add($lines[$i])
                $i++
            }
            if ($i -eq $lines.Count) { throw 'Unterminated output command.' }
            $values[$name] = $content -join "`n"
        }
        elseif ($lines[$i] -match '^([^=]+)=(.*)$') {
            $values[$Matches[1]] = $Matches[2]
        }
        elseif ($lines[$i].Length -ne 0) {
            throw "Malformed output command: $($lines[$i])"
        }
    }
    return $values
}

function Invoke-Sample([string] $File, [string[]] $Arguments = @(), [hashtable] $Inputs = @{}) {
    Get-ChildItem Env:INPUT_* | Remove-Item
    foreach ($entry in $Inputs.GetEnumerator()) {
        Set-Item "Env:INPUT_$($entry.Key.ToUpperInvariant())" ([string] $entry.Value)
    }
    Set-Content $env:GITHUB_OUTPUT '' -NoNewline
    & dotnet run --file $File -- @Arguments | ForEach-Object { Write-Host $_ }
    if ($LASTEXITCODE) { throw "$File failed with exit $LASTEXITCODE" }
    return Read-Outputs
}

Push-Location $stage
try {
    $declarations = @('helpers.cs', 'file-stats.helpers.cs')
    foreach ($file in Get-ChildItem -Filter '*.cs' | Where-Object Name -NotIn $declarations) {
        & dotnet build $file.Name --nologo -v quiet
        if ($LASTEXITCODE) { throw "Compilation failed: $($file.Name)" }
    }
    $inputs = Invoke-Sample 'inputs.cs' -Inputs @{ name = 'CSharp'; enabled = 'true'; items = "alpha`nbeta" }
    if ($inputs['name'] -ne 'CSharp' -or $inputs['enabled'] -ne 'true' -or $inputs['items'] -ne '2') {
        throw 'Typed input assertion failed.'
    }
    $outputs = Invoke-Sample 'outputs.cs' -Arguments @('produce')
    Invoke-Sample 'outputs.cs' -Arguments @('consume') -Inputs $outputs | Out-Null
    $environment = Invoke-Sample 'environment.cs' -Arguments @('produce')
    $env:CSHARP_SHOWCASE = 'ready'
    $toolPath = (Get-Content $env:GITHUB_PATH | Select-Object -Last 1)
    $env:PATH = "$toolPath$([IO.Path]::PathSeparator)$env:PATH"
    Invoke-Sample 'environment.cs' -Arguments @('consume') -Inputs $environment | Out-Null
    $diagnostics = Invoke-Sample 'diagnostics.cs'
    if ($diagnostics['notices'] -ne '1' -or $diagnostics['warnings'] -ne '1') { throw 'Missing diagnostics.' }
    $summary = Invoke-Sample 'summary.cs' -Arguments @('measure')
    Invoke-Sample 'summary.cs' -Arguments @('summarize') -Inputs $summary | Out-Null
    $process = Invoke-Sample 'process.cs'
    if ($process['negativeExit'] -eq '0') { throw 'Negative process returned success.' }
    $files = Invoke-Sample 'filesystem.cs' -Arguments @('stage')
    Invoke-Sample 'filesystem.cs' -Arguments @('verify') -Inputs $files | Out-Null
    $stats = Invoke-Sample 'file-stats.cs' -Arguments @('scan')
    Invoke-Sample 'file-stats.cs' -Arguments @('summarize') -Inputs $stats | Out-Null

    $env:INPUT_MESSAGE = 'stdin "quotes" $literal'
    $stdinProgram = @'
#:sdk ActionsToolkit.Sdk
using static ActionsToolkit.Scripting.Core;
if (GetInput("message") != "stdin \"quotes\" $literal" || args is not ["hello world", "$literal"])
    throw new InvalidOperationException("Stdin quoting/arguments changed.");
await SetOutputAsync("stdin", "passed");
'@
    $sdkDirective = "#:sdk ActionsToolkit.Sdk@$sdkVersion"
    $stdinProgram.Replace('#:sdk ActionsToolkit.Sdk', $sdkDirective) | dotnet run - -- 'hello world' '$literal'
    if ($LASTEXITCODE -or (Read-Outputs)['stdin'] -ne 'passed') { throw 'Stdin SDK consumption failed.' }
    $negativeProgram = @'
#:sdk ActionsToolkit.Sdk
Core.SetFailed("Expected negative exit probe.");
'@
    $negativeProgram.Replace('#:sdk ActionsToolkit.Sdk', $sdkDirective) | dotnet run -
    if ($LASTEXITCODE -eq 0) { throw 'SetFailed returned success.' }
    $env:INPUT_MISSING = ''
    $requiredInputProgram = @'
#:sdk ActionsToolkit.Sdk
Core.GetInput("missing", required: true);
'@
    $requiredInputProgram.Replace('#:sdk ActionsToolkit.Sdk', $sdkDirective) | dotnet run -
    if ($LASTEXITCODE -eq 0) { throw 'Missing required input returned success.' }

    $inherited = Join-Path $root '.sdk-consumer-tests'
    New-Item $inherited -ItemType Directory -Force | Out-Null
    Copy-Item NuGet.Config (Join-Path $inherited 'NuGet.Config') -Force
    # This project intentionally inherits the repository's real build and CPM configuration.
    @"
<Project Sdk="ActionsToolkit.Sdk/$sdkVersion">
  <PropertyGroup><TargetFramework>net11.0</TargetFramework><GenerateDocumentationFile>false</GenerateDocumentationFile></PropertyGroup>
</Project>
"@ | Set-Content (Join-Path $inherited 'Consumer.csproj')
    'Core.Info("Inherited repository configuration passed.");' | Set-Content (Join-Path $inherited 'Program.cs')
    & dotnet run --project (Join-Path $inherited 'Consumer.csproj')
    if ($LASTEXITCODE) { throw 'In-repository CPM consumer failed.' }

    Write-Output 'All local packaged SDK consumers passed. Credential-dependent runner operations require their showcase workflows.'
}
finally {
    Pop-Location
}
