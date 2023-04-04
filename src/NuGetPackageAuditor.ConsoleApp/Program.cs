using System.Text.Json;
using NuGetPackageAuditor;

var packageAuditor = new PackageAuditor();

Console.WriteLine("Hi and welcome to the NuGet Package Auditor Console App!");
Console.WriteLine("Here you can interact with the NuGet Package Auditor to find out more about a specific NuGet package.");

while (true)
{
    PrintHelp();
    var input = Console.ReadLine();
    if (input == "0")
        return;

    if (input != "1")
        break;

    string? packageId;
    do
    {
        Console.WriteLine("What is the name of the package you want to inspect?");
        packageId = Console.ReadLine();
    } while (string.IsNullOrWhiteSpace(packageId));

    string? packageVersion;
    do
    {
        Console.WriteLine("What is the version of the package you want to inspect?");
        packageVersion = Console.ReadLine();
    } while (string.IsNullOrWhiteSpace(packageVersion));

    var settings = new GetPackageDetailsSettings {IncludeSourceControlInAuditIfExists = true};
    var result = await packageAuditor.GetPackageDetailsAsync(packageId, packageVersion, settings);

    var serialized = JsonSerializer.Serialize(result, new JsonSerializerOptions
    {
        WriteIndented = true
    });

    Console.WriteLine($"Result for {packageId} and {packageVersion}");
    Console.WriteLine(serialized);
    Console.WriteLine("");
}

void PrintHelp()
{
    Console.WriteLine("");
    Console.WriteLine("Options");
    Console.WriteLine("- 0: Exit");
    Console.WriteLine("- 1: Audit package");
    Console.WriteLine("");
    Console.WriteLine("What option would you like to use?");
}