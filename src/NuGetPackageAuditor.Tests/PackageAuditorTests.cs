using System.Net;

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

            Assert.True(result.Result);
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

            Assert.False(result.Result);
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

            Assert.False(result.Result);
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

            Assert.True(result.Result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task IsPackageDeprecatedAsync_ThrowsArgumentIsNullException_WhenPackageIdIsEmpty(string packageId)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _packageAuditor.IsPackageDeprecatedAsync(packageId, _packageVersion));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task IsPackageDeprecatedAsync_ThrowsArgumentIsNullException_WhenPackageVersionIsEmpty(string packageVersion)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _packageAuditor.IsPackageDeprecatedAsync(_packageId, packageVersion));
        }

        [Fact]
        public async Task IsPackageDeprecatedAsync_ReturnFailureResult_WhenApiThrows404()
        {
            _nuGetApiQuerier.When(x => x.GetCatalogRootAsync(_packageId)).Throw(new HttpRequestException("Response status code does not indicate success: 404 (Not Found).", null, HttpStatusCode.NotFound));

            var result = await _packageAuditor.IsPackageDeprecatedAsync(_packageId, _packageVersion);

            Assert.Equal(default, result.Result);
            Assert.False(result.IsSuccess);
            Assert.Equal($"Could not find package with id '{_packageId}' on the NuGet Catalog API.", result.Error);
        }

        [Fact]
        public async Task IsPackageDeprecatedAsync_ReturnsFailureResult_WhenVersionDoesntExist()
        {
            _catalogRootBuilder.WithPackage(new Package
            {
                PackageDetails = new PackageDetails
                {
                    Version = "AnotherPackageVersion",
                    Deprecation = null
                }
            });
            var catalogRoot = _catalogRootBuilder.Build();
            _nuGetApiQuerier.GetCatalogRootAsync(_packageId).Returns(catalogRoot);

            var result = await _packageAuditor.IsPackageDeprecatedAsync(_packageId, _packageVersion);

            Assert.Equal(default, result.Result);
            Assert.False(result.IsSuccess);
            Assert.Equal($"Could not find package version '{_packageVersion}' for '{_packageId}'.", result.Error);
        }
    }
}