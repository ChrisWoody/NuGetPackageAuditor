<Query Kind="Statements">
  <Reference Relative="..\src\NuGetPackageAuditor\bin\Debug\netstandard2.0\NuGetPackageAuditor.dll">C:\source\NuGetPackageAuditor\src\NuGetPackageAuditor\bin\Debug\netstandard2.0\NuGetPackageAuditor.dll</Reference>
  <Namespace>NuGetPackageAuditor</Namespace>
</Query>

//var packageAuditor = new PackageAuditor();
//var packageAuditor = new PackageAuditor(NuGetApiQuerierFactory.CreateWithMemoryCache());
var gitHubApiQuerierSettings = new GitHubApiQuerierSettings { ApiPersonalAccessToken = "APIPAT" };
var getPackageDetailsSettings = new GetPackageDetailsSettings { IncludeSourceControlInAuditIfExists = true };
var packageAuditor = new PackageAuditor(NuGetApiQuerierFactory.CreateWithFileCache(@"PATHTOCACHE"), GitHubApiQuerierFactory.CreateWithFileCache(@"PATHTOCACHE", gitHubApiQuerierSettings));
var packageId = "newtonsoft.json";
var pacakgeVersionRange = "12.0.1";
var result = await packageAuditor.GetPackageDetailsAsync(packageId, pacakgeVersionRange, getPackageDetailsSettings);
result.Dump();