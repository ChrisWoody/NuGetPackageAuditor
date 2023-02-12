namespace NuGetPackageAuditor.Tests
{
    public class PackageAuditorTests
    {
        private readonly string _packageId = Guid.NewGuid().ToString();
        private readonly string _packageVersion = Guid.NewGuid().ToString();

        private readonly INuGetApiQuerier _nuGetApiQuerier;
        private readonly PackageAuditor _packageAuditor;
        private readonly CatalogRootBuilder _catalogRootBuilder;

        public PackageAuditorTests()
        {
            _nuGetApiQuerier = Substitute.For<INuGetApiQuerier>();
            _packageAuditor = new PackageAuditor(_nuGetApiQuerier);
            _catalogRootBuilder = new CatalogRootBuilder();
        }

        [Fact]
        public async Task IsPackageDeprecatedAsync_ReturnsTrue_WhenPackageIdAndVersionIsDeprecated()
        {
            _catalogRootBuilder.WithPackage(new Package
            {
                PackageDetails = new PackageDetails
                {
                    Version = _packageVersion,
                    Deprecation = new Deprecation
                    {
                        Message = "This package is deprecated",
                        Reasons = new[] {"Other"}
                    }
                }
            });
            var catalogRoot = _catalogRootBuilder.Build();
            _nuGetApiQuerier.GetCatalogRootAsync(_packageId).Returns(catalogRoot);

            var result = await _packageAuditor.IsPackageDeprecatedAsync(_packageId, _packageVersion);

            Assert.True(result);
        }

        [Fact]
        public async Task IsPackageDeprecatedAsync_ReturnsFalse_WhenPackageIdAndVersionIsNotDeprecated()
        {
            _catalogRootBuilder.WithPackage(new Package
            {
                PackageDetails = new PackageDetails
                {
                    Version = _packageVersion,
                    Deprecation = null
                }
            });
            var catalogRoot = _catalogRootBuilder.Build();
            _nuGetApiQuerier.GetCatalogRootAsync(_packageId).Returns(catalogRoot);

            var result = await _packageAuditor.IsPackageDeprecatedAsync(_packageId, _packageVersion);

            Assert.False(result);
        }

        [Fact]
        public async Task IsPackageDeprecatedAsync_ReturnsFalse_WhenPackageIdAndVersionIsNotDeprecatedButOtherVersionIsDeprecated()
        {
            _catalogRootBuilder.WithPackage(new Package
            {
                PackageDetails = new PackageDetails
                {
                    Version = _packageVersion,
                    Deprecation = null
                }
            });
            _catalogRootBuilder.WithPackage(new Package
            {
                PackageDetails = new PackageDetails
                {
                    Version = "anotherversion",
                    Deprecation = new Deprecation
                    {
                        Message = "This package is deprecated",
                        Reasons = new[] { "Other" }
                    }
                }
            });
            var catalogRoot = _catalogRootBuilder.Build();
            _nuGetApiQuerier.GetCatalogRootAsync(_packageId).Returns(catalogRoot);

            var result = await _packageAuditor.IsPackageDeprecatedAsync(_packageId, _packageVersion);

            Assert.False(result);
        }

        [Fact]
        public async Task IsPackageDeprecatedAsync_ReturnsTrue_WhenPackageIdAndVersionIsDeprecatedAndOtherVersionIsDeprecated()
        {
            _catalogRootBuilder.WithPackage(new Package
            {
                PackageDetails = new PackageDetails
                {
                    Version = _packageVersion,
                    Deprecation = new Deprecation
                    {
                        Message = "This package is deprecated",
                        Reasons = new[] { "Other" }
                    }
                }
            });
            _catalogRootBuilder.WithPackage(new Package
            {
                PackageDetails = new PackageDetails
                {
                    Version = "anotherversion",
                    Deprecation = new Deprecation
                    {
                        Message = "This package is also deprecated",
                        Reasons = new[] { "Other" }
                    }
                }
            });
            var catalogRoot = _catalogRootBuilder.Build();
            _nuGetApiQuerier.GetCatalogRootAsync(_packageId).Returns(catalogRoot);

            var result = await _packageAuditor.IsPackageDeprecatedAsync(_packageId, _packageVersion);

            Assert.True(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task IsPackageDeprecatedAsync_ThrowsArgumentIsNullException_WhenPackageIdIsEmpty(string packageId)
        {
            var nuGetApiQuerier = Substitute.For<INuGetApiQuerier>();
            var packageAuditor = new PackageAuditor(nuGetApiQuerier);

            await Assert.ThrowsAsync<ArgumentNullException>(() => packageAuditor.IsPackageDeprecatedAsync(packageId, _packageVersion));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task IsPackageDeprecatedAsync_ThrowsArgumentIsNullException_WhenPackageVersionIsEmpty(string packageVersion)
        {
            var nuGetApiQuerier = Substitute.For<INuGetApiQuerier>();
            var packageAuditor = new PackageAuditor(nuGetApiQuerier);

            await Assert.ThrowsAsync<ArgumentNullException>(() => packageAuditor.IsPackageDeprecatedAsync(_packageId, packageVersion));
        }
    }
}