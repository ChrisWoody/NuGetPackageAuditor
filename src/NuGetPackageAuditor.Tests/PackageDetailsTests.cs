namespace NuGetPackageAuditor.Tests;

public class PackageDetailsTests
{
    [Fact]
    public void ValidateAllPropertiesWithSettersAreInternal()
    {
        foreach (var property in typeof(PackageDetails).GetProperties().Where(x => x.SetMethod != null))
        {
            Assert.True(property.SetMethod.IsAssembly, $"'{property.Name}' setter should be marked as 'internal'.");
        }
    }
}