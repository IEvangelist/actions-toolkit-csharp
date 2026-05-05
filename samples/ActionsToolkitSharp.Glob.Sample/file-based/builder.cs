// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

// builder.cs
//
// Companion to the upstream `@actions/glob` README — illustrates the
// full builder API exposed by ActionsToolkitSharp.Glob via the
// `IGlobPatternResolverBuilder` service registered by AddGitHubActionsGlob().
//
// Use this style when you need to combine include and exclude patterns
// or compose patterns from multiple sources at runtime.
//
// Run with:
//   dotnet run builder.cs

#:package ActionsToolkitSharp.Glob@*
#:package Microsoft.Extensions.DependencyInjection@*

using ActionsToolkitSharp.Glob;
using Microsoft.Extensions.DependencyInjection;

using var provider = new ServiceCollection()
    .AddGitHubActionsGlob()
    .BuildServiceProvider();

var builder = provider.GetRequiredService<IGlobPatternResolverBuilder>();

var resolver = builder
    .WithInclusions("**/*.cs", "**/*.md")
    .WithExclusions("**/bin/**", "**/obj/**")
    .Build();

foreach (var file in resolver.GetGlobFiles())
{
    Console.WriteLine(file);
}
