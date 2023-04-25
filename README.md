# NuGetPackageAuditor

A tool primarily to check if NuGet package is deprecated and out of support. Other checks may be added going forward.

## How to use

````csharp
// Default implementation, no caching
var packageAuditor = new PackageAuditor();
var result = await packageAuditor.GetPackageDetailsAsync(<NUGET PACKAGE ID>, <NUGET PACKAGE VERSION>);

// Default implementation, no caching, example
var packageAuditor = new PackageAuditor();
var result = await packageAuditor.GetPackageDetailsAsync("Newtonsoft.Json", "13.0.3");

// Default implementation, no caching, example using NuGet's version ranges
var packageAuditor = new PackageAuditor();
var result = await packageAuditor.GetPackageDetailsAsync("Newtonsoft.Json", "(13.0.3,)");

// Default implementation with settings, no caching
var packageAuditor = new PackageAuditor();
var settings = new GetPackageDetailsSettings {IncludeSourceControlInAuditIfExists = true};
var result = await packageAuditor.GetPackageDetailsAsync(<NUGET PACKAGE ID>, <NUGET PACKAGE VERSION>, settings);

// Cached implementation
var packageAuditor = new PackageAuditor(
	NuGetApiQuerierFactory.CreateWithFileCache(<NUGET FOLDER PATH>),
	GitHubApiQuerierFactory.CreateWithFileCache(<GITHUB FOLDER PATH>));
var settings = new GetPackageDetailsSettings {IncludeSourceControlInAuditIfExists = true};
var result = await packageAuditor.GetPackageDetailsAsync(<NUGET PACKAGE ID>, <NUGET PACKAGE VERSION>, settings);

// Cached implementation, using your own GitHub personal access token to remove GitHub's anonymous API rate limit
var gitHubApiQuerierSettings = new GitHubApiQuerierSettings{ApiPersonalAccessToken = "<GITHUB PAT>"};
var packageAuditor = new PackageAuditor(
	NuGetApiQuerierFactory.CreateWithFileCache(<NUGET FOLDER PATH>),
	GitHubApiQuerierFactory.CreateWithFileCache(<GITHUB FOLDER PATH>, gitHubApiQuerierSettings));
var settings = new GetPackageDetailsSettings {IncludeSourceControlInAuditIfExists = true};
var result = await packageAuditor.GetPackageDetailsAsync(<NUGET PACKAGE ID>, <NUGET PACKAGE VERSION>, settings);
````