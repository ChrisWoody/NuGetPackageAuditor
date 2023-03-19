<Query Kind="Statements">
  <Reference Relative="..\src\NuGetPackageAuditor\bin\Debug\netstandard2.0\NuGetPackageAuditor.dll">C:\source\NuGetPackageAuditor\src\NuGetPackageAuditor\bin\Debug\netstandard2.0\NuGetPackageAuditor.dll</Reference>
  <Namespace>NuGetPackageAuditor</Namespace>
</Query>

//var packageAuditor = new PackageAuditor();
//var packageAuditor = new PackageAuditor(NuGetApiQuerierFactory.CreateWithMemoryCache());
var packageAuditor = new PackageAuditor(NuGetApiQuerierFactory.CreateWithFileCache(@"PATHTOCACHE"));

var packageId = "WindowsAzure.Storage";
var pacakgeVersionRange = "9.3.3";

var result = await packageAuditor.GetPackageDetailsAsync(packageId, pacakgeVersionRange);
result.Dump();
