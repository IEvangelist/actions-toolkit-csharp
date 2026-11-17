$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = Split-Path $PSScriptRoot -Parent
Push-Location $root
try {
    dotnet build src/ActionsToolkit.Scripting/ActionsToolkit.Scripting.csproj -c Release --nologo -m:1
    if ($LASTEXITCODE) { throw 'Scripting package build failed.' }
    dotnet build src/ActionsToolkit.Sdk/ActionsToolkit.Sdk.csproj -c Release --nologo -m:1
    if ($LASTEXITCODE) { throw 'SDK package build failed.' }
    foreach ($project in Get-ChildItem src -Filter '*.csproj' -Recurse) {
        dotnet pack $project.FullName -c Release --no-build --nologo -v quiet
        if ($LASTEXITCODE) { throw "Packing failed: $($project.Name)" }
    }

    $feed = Join-Path $root 'artifacts/package/release'
    $packages = @(Get-ChildItem $feed -Filter 'ActionsToolkit.Sdk.*.nupkg')
    if ($packages.Count -ne 1) {
        throw "Expected one SDK package in $feed; found $($packages.Count). Clean stale package files first."
    }
    $version = $packages[0].Name -replace '^ActionsToolkit\.Sdk\.', '' -replace '\.nupkg$', ''
    $stage = Join-Path $root '.showcase'
    New-Item $stage -ItemType Directory -Force | Out-Null
    Copy-Item 'samples/ActionsToolkit.Sdk.Sample/*' $stage -Recurse -Force

    $globalJson = Get-Content 'global.json' -Raw | ConvertFrom-Json -AsHashtable
    $globalJson['msbuild-sdks'] = @{ 'ActionsToolkit.Sdk' = $version }
    $globalJson | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $stage 'global.json')
    $packageCache = ".packages/$([Guid]::NewGuid().ToString('N'))"
    @'
<configuration>
  <config>
    <add key="globalPackagesFolder" value="__PACKAGE_CACHE__" />
  </config>
  <packageSources>
    <clear />
    <add key="local" value="../artifacts/package/release" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
'@.Replace('__PACKAGE_CACHE__', $packageCache) | Set-Content (Join-Path $stage 'NuGet.Config')
    '<Project><PropertyGroup><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings></PropertyGroup></Project>' |
        Set-Content (Join-Path $stage 'Directory.Build.props')
    '<Project />' | Set-Content (Join-Path $stage 'Directory.Build.targets')
    '<Project><PropertyGroup><ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally></PropertyGroup></Project>' |
        Set-Content (Join-Path $stage 'Directory.Packages.props')
    Write-Output "Staged ActionsToolkit.Sdk $version in $stage"
}
finally {
    Pop-Location
}
