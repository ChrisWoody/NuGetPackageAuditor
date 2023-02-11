namespace NuGetPackageAuditor.Tests
{
    public class PackageAuditorTests
    {
        [Fact]
        public async Task PackageIsDeprecated()
        {
            var packageAuditor = new PackageAuditor();

            var result = await packageAuditor.IsPackageDeprecatedAsync("RandomPackageId");

            Assert.True(result);
        }
    }
}