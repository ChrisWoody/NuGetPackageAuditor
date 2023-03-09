using System.Net;

namespace NuGetPackageAuditor.Tests
{
    public class PackageAuditorTests
    {
        private readonly string _packageId = Guid.NewGuid().ToString();
        private readonly string _packageVersionRange = "1.0.0";

        private readonly INuGetApiQuerier _nuGetApiQuerier;
        private readonly PackageAuditor _packageAuditor;
        private readonly CatalogRootBuilder _catalogRootBuilder;

        public PackageAuditorTests()
        {
            _nuGetApiQuerier = Substitute.For<INuGetApiQuerier>();
            _packageAuditor = new PackageAuditor(_nuGetApiQuerier);
            _catalogRootBuilder = new CatalogRootBuilder();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetPackageDetailsAsync_ThrowsArgumentIsNullException_WhenPackageIdIsEmpty(string packageId)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _packageAuditor.GetPackageDetailsAsync(packageId, _packageVersionRange));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetPackageDetailsAsync_ThrowsArgumentIsNullException_WhenPackageVersionRangeIsEmpty(string packageVersionRange)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _packageAuditor.GetPackageDetailsAsync(_packageId, packageVersionRange));
        }

        [Fact]
        public async Task GetPackageDetailsAsync_PackageDetailsHasError_WhenApiThrows404()
        {
            _nuGetApiQuerier.When(x => x.GetCatalogRootAsync(_packageId)).Throw(
                new HttpRequestException("Response status code does not indicate success: 404 (Not Found).", null, HttpStatusCode.NotFound));

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, _packageVersionRange);

            Assert.NotNull(result);
            Assert.True(result.HasError);
            Assert.Equal($"Could not find package with id '{_packageId}' on the NuGet Catalog API.", result.Error);
        }

        [Fact]
        public async Task GetPackageDetailsAsync_PackageDetailsHasError_WhenCatalogRootIsMissingCatalogPages()
        {
            var catalogRoot = _catalogRootBuilder.Build();
            _nuGetApiQuerier.GetCatalogRootAsync(_packageId).Returns(catalogRoot);

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, _packageVersionRange);

            Assert.NotNull(result);
            Assert.True(result.HasError);
            Assert.Equal($"Package with id '{_packageId}' found on the NuGet Catalog API but is missing 'CatalogPages'.", result.Error);
        }

        [Fact]
        public async Task GetPackageDetailsAsync_PackageDetailsHasError_WhenCatalogRootIsMissingPackages()
        {
            _catalogRootBuilder.WithCatalogPage(new CatalogPage());
            var catalogRoot = _catalogRootBuilder.Build();
            _nuGetApiQuerier.GetCatalogRootAsync(_packageId).Returns(catalogRoot);

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, _packageVersionRange);

            Assert.NotNull(result);
            Assert.True(result.HasError);
            Assert.Equal($"Package with id '{_packageId}' found on the NuGet Catalog API but is missing 'Packages'.", result.Error);
        }

        [Theory]
        [InlineData("asd123")]
        [InlineData("!@#$")]
        [InlineData("[1.0.0")]
        [InlineData("1.0.0]")]
        [InlineData("(1.0.0")]
        [InlineData("1.0.0)")]
        [InlineData("[1.0.0.0.0.0]")]
        [InlineData("(1.0.0)")]
        [InlineData("(1.0.0,1.0.0.0.0)")]
        [InlineData("(1.0.0,0)")]
        [InlineData("(2.0.0,1.0.0)")]
        public async Task GetPackageDetailsAsync_PackageDetailsHasError_WhenPackageVersionRangeIsInvalid(string packageVersionRange)
        {
            _catalogRootBuilder.WithCatalogPage(new CatalogPage{Packages = new Package[1]});
            var catalogRoot = _catalogRootBuilder.Build();
            _nuGetApiQuerier.GetCatalogRootAsync(_packageId).Returns(catalogRoot);

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, packageVersionRange);

            Assert.NotNull(result);
            Assert.True(result.HasError);
            Assert.Equal($"Package version range of '{packageVersionRange}' is invalid.", result.Error);
        }

        [Fact]
        public async Task GetPackageDetailsAsync_PackageDetailsHasError_WhenBestPackageVersionCouldNotBeFound()
        {
            _catalogRootBuilder.WithPackage(new Package
            {
                CatalogEntry = new CatalogEntry
                {
                    Version = "0.0.9" // older than provided range
                }
            });
            var catalogRoot = _catalogRootBuilder.Build();
            _nuGetApiQuerier.GetCatalogRootAsync(_packageId).Returns(catalogRoot);

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, _packageVersionRange);

            Assert.NotNull(result);
            Assert.True(result.HasError);
            Assert.Equal($"The package version of {_packageVersionRange} could not be found for '{_packageId}'.", result.Error);
        }

        [Fact]
        public async Task GetPackageDetailsAsync_ReturnsPackageDetails_WithNormalInformation()
        {
            _catalogRootBuilder.WithPackage(new Package
            {
                CatalogEntry = new CatalogEntry
                {
                    Version = _packageVersionRange,
                    IsListed = true,
                }
            });
            var catalogRoot = _catalogRootBuilder.Build();
            _nuGetApiQuerier.GetCatalogRootAsync(_packageId).Returns(catalogRoot);

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, _packageVersionRange);

            Assert.NotNull(result);
            Assert.False(result.HasError);
            Assert.Null(result.Error);
            Assert.Equal(_packageId, result.Id);
            Assert.Equal(_packageVersionRange, result.VersionRange);
            Assert.Equal(_packageVersionRange, result.Version);
            Assert.True(result.IsListed);
            Assert.Equal(DeprecatedReason.PackageIsNotDeprecated, result.DeprecatedReason);
            Assert.Null(result.NuGetDeprecationMessage);
            Assert.Null(result.NuGetDeprecationReasons);
        }

        [Fact]
        public async Task GetPackageDetailsAsync_ReturnsPackageDetails_WithDeprecationInformation()
        {
            _catalogRootBuilder.WithPackage(new Package
            {
                CatalogEntry = new CatalogEntry
                {
                    Version = _packageVersionRange,
                    IsListed = true,
                    Deprecation = new Deprecation
                    {
                        Message = "This package is deprecated",
                        Reasons = new[] {"Other"}
                    }
                }
            });
            var catalogRoot = _catalogRootBuilder.Build();
            _nuGetApiQuerier.GetCatalogRootAsync(_packageId).Returns(catalogRoot);

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, _packageVersionRange);

            Assert.NotNull(result);
            Assert.False(result.HasError);
            Assert.Null(result.Error);
            Assert.Equal(_packageId, result.Id);
            Assert.Equal(_packageVersionRange, result.VersionRange);
            Assert.Equal(_packageVersionRange, result.Version);
            Assert.True(result.IsListed);
            Assert.Equal(DeprecatedReason.PackageIsMarkedAsDeprecated, result.DeprecatedReason);
            Assert.Equal("This package is deprecated", result.NuGetDeprecationMessage);
            Assert.Single(result.NuGetDeprecationReasons);
            Assert.Contains("Other", result.NuGetDeprecationReasons);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetPackageDetailsAsync_ReturnsPackageDetails_WithListedInformation(bool isListed)
        {
            _catalogRootBuilder.WithPackage(new Package
            {
                CatalogEntry = new CatalogEntry
                {
                    Version = _packageVersionRange,
                    IsListed = isListed,
                }
            });
            var catalogRoot = _catalogRootBuilder.Build();
            _nuGetApiQuerier.GetCatalogRootAsync(_packageId).Returns(catalogRoot);

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, _packageVersionRange);

            Assert.NotNull(result);
            Assert.False(result.HasError);
            Assert.Null(result.Error);
            Assert.Equal(_packageId, result.Id);
            Assert.Equal(_packageVersionRange, result.VersionRange);
            Assert.Equal(_packageVersionRange, result.Version);
            Assert.Equal(isListed, result.IsListed);
            Assert.Equal(DeprecatedReason.PackageIsNotDeprecated, result.DeprecatedReason);
            Assert.Null(result.NuGetDeprecationMessage);
            Assert.Null(result.NuGetDeprecationReasons);
        }

        // todo: add dependency related tests
    }
}