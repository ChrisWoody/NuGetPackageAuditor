using System.Net;
using System.Text.Json;
using NSubstitute.ExceptionExtensions;

namespace NuGetPackageAuditor.Tests
{
    public class PackageAuditorTests
    {
        private readonly string _packageId = Guid.NewGuid().ToString();
        private readonly string _packageVersionRange = "1.0.0";
        private readonly string _projectUrl = "https://github.com/orgname/reponame";

        private readonly INuGetApiQuerier _nuGetApiQuerier;
        private readonly IGitHubApiQuerier _gitHubApiQuerier;
        private readonly PackageAuditor _packageAuditor;
        private readonly CatalogRootBuilder _catalogRootBuilder;
        private readonly GitHubRepositoryMetadataBuilder _gitHubRepositoryMetadataBuilder;

        public PackageAuditorTests()
        {
            _nuGetApiQuerier = Substitute.For<INuGetApiQuerier>();
            _gitHubApiQuerier = Substitute.For<IGitHubApiQuerier>();
            _packageAuditor = new PackageAuditor(_nuGetApiQuerier, _gitHubApiQuerier);
            _catalogRootBuilder = new CatalogRootBuilder();
            _gitHubRepositoryMetadataBuilder = new GitHubRepositoryMetadataBuilder();
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
            _nuGetApiQuerier.When(x => x.GetRawCatalogRootAsync(_packageId)).Throw(
                new HttpRequestException("Response status code does not indicate success: 404 (Not Found).", null, HttpStatusCode.NotFound));

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, _packageVersionRange);

            Assert.NotNull(result);
            Assert.True(result.HasError);
            Assert.Equal($"Could not find package with id '{_packageId}' on the NuGet Catalog API.", result.Error);
        }

        [Fact]
        public async Task GetPackageDetailsAsync_PackageDetailsHasError_WhenCatalogRootIsMissingCatalogPages()
        {
            var catalogRoot = _catalogRootBuilder.BuildAsApiBytes();
            _nuGetApiQuerier.GetRawCatalogRootAsync(_packageId).Returns(catalogRoot);

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, _packageVersionRange);

            Assert.NotNull(result);
            Assert.True(result.HasError);
            Assert.Equal($"Could not find package with id '{_packageId}' on the NuGet Catalog API.", result.Error);
        }

        [Fact]
        public async Task GetPackageDetailsAsync_PackageDetailsHasError_WhenCatalogRootIsMissingPackages()
        {
            _catalogRootBuilder.WithCatalogPage(new CatalogPage());
            var catalogRoot = _catalogRootBuilder.BuildAsApiBytes();
            _nuGetApiQuerier.GetRawCatalogRootAsync(_packageId).Returns(catalogRoot);

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, _packageVersionRange);

            Assert.NotNull(result);
            Assert.True(result.HasError);
            Assert.Equal($"Could not find package with id '{_packageId}' on the NuGet Catalog API.", result.Error);
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
            var catalogRoot = _catalogRootBuilder.BuildAsApiBytes();
            _nuGetApiQuerier.GetRawCatalogRootAsync(_packageId).Returns(catalogRoot);

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
            var catalogRoot = _catalogRootBuilder.BuildAsApiBytes();
            _nuGetApiQuerier.GetRawCatalogRootAsync(_packageId).Returns(catalogRoot);

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
            var catalogRoot = _catalogRootBuilder.BuildAsApiBytes();
            _nuGetApiQuerier.GetRawCatalogRootAsync(_packageId).Returns(catalogRoot);

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, _packageVersionRange);

            Assert.NotNull(result);
            Assert.False(result.HasError);
            Assert.Null(result.Error);
            Assert.Equal(_packageId, result.Id);
            Assert.Equal(_packageVersionRange, result.VersionRange);
            Assert.Equal(_packageVersionRange, result.Version);
            Assert.True(result.IsListed);
            Assert.Equal(DeprecatedReason.NotDeprecated, result.DeprecatedReason);
            Assert.False(result.NuGetDeprecationExists);
            Assert.Null(result.NuGetDeprecationMessage);
            Assert.Null(result.NuGetDeprecationReasons);
            Assert.Null(result.SourceControlMetadata);
        }

        [Fact]
        public async Task GetPackageDetailsAsync_ReturnsPackageDetails_WithNormalInformation_WhenCatalogRootIsSplit()
        {
            var catalogPageId = _packageId + "/page/version";
            _catalogRootBuilder.WithCatalogPage(new CatalogPage
            {
                Id = catalogPageId,
                Packages = null
            });
            var catalogRoot = _catalogRootBuilder.BuildAsApiBytes();
            _nuGetApiQuerier.GetRawCatalogRootAsync(_packageId).Returns(catalogRoot);

            var catalogPage = new CatalogPage
            {
                Id = catalogPageId,
                Packages = new []
                {
                    new Package
                    {
                        CatalogEntry = new CatalogEntry
                        {
                            Version = _packageVersionRange,
                            IsListed = true,
                        }
                    }
                }
            };
            var catalogPageBytes = JsonSerializer.SerializeToUtf8Bytes(catalogPage);
            _nuGetApiQuerier.GetRawCatalogPageAsync(catalogPageId).Returns(catalogPageBytes);

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, _packageVersionRange);

            Assert.NotNull(result);
            Assert.False(result.HasError);
            Assert.Null(result.Error);
            Assert.Equal(_packageId, result.Id);
            Assert.Equal(_packageVersionRange, result.VersionRange);
            Assert.Equal(_packageVersionRange, result.Version);
            Assert.True(result.IsListed);
            Assert.Equal(DeprecatedReason.NotDeprecated, result.DeprecatedReason);
            Assert.False(result.NuGetDeprecationExists);
            Assert.Null(result.NuGetDeprecationMessage);
            Assert.Null(result.NuGetDeprecationReasons);
            Assert.Null(result.SourceControlMetadata);
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
            var catalogRoot = _catalogRootBuilder.BuildAsApiBytes();
            _nuGetApiQuerier.GetRawCatalogRootAsync(_packageId).Returns(catalogRoot);

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, _packageVersionRange);

            Assert.NotNull(result);
            Assert.False(result.HasError);
            Assert.Null(result.Error);
            Assert.Equal(_packageId, result.Id);
            Assert.Equal(_packageVersionRange, result.VersionRange);
            Assert.Equal(_packageVersionRange, result.Version);
            Assert.True(result.IsListed);
            Assert.Equal(DeprecatedReason.DeprecatedOnNuGet, result.DeprecatedReason);
            Assert.True(result.NuGetDeprecationExists);
            Assert.Equal("This package is deprecated", result.NuGetDeprecationMessage);
            Assert.Single(result.NuGetDeprecationReasons);
            Assert.Contains("Other", result.NuGetDeprecationReasons);
            Assert.Null(result.SourceControlMetadata);
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
            var catalogRoot = _catalogRootBuilder.BuildAsApiBytes();
            _nuGetApiQuerier.GetRawCatalogRootAsync(_packageId).Returns(catalogRoot);

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, _packageVersionRange);

            Assert.NotNull(result);
            Assert.False(result.HasError);
            Assert.Null(result.Error);
            Assert.Equal(_packageId, result.Id);
            Assert.Equal(_packageVersionRange, result.VersionRange);
            Assert.Equal(_packageVersionRange, result.Version);
            Assert.Equal(isListed, result.IsListed);
            Assert.Equal(DeprecatedReason.NotDeprecated, result.DeprecatedReason);
            Assert.False(result.NuGetDeprecationExists);
            Assert.Null(result.NuGetDeprecationMessage);
            Assert.Null(result.NuGetDeprecationReasons);
            Assert.Null(result.SourceControlMetadata);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetPackageDetailsAsync_ReturnsPackageDetails_WithSourceControlInformation(bool isArchived)
        {
            _catalogRootBuilder.WithPackage(new Package
            {
                CatalogEntry = new CatalogEntry
                {
                    Version = _packageVersionRange,
                    IsListed = true,
                    ProjectUrl = _projectUrl
                }
            });
            var catalogRoot = _catalogRootBuilder.BuildAsApiBytes();
            _nuGetApiQuerier.GetRawCatalogRootAsync(_packageId).Returns(catalogRoot);

            var gitHubMetadata = new GitHubRepositoryMetadata
            {
                Archived = isArchived,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                PushedAt = DateTimeOffset.UtcNow,
            };
            var gitHubRepositoryMetadata = _gitHubRepositoryMetadataBuilder.WithMetadata(gitHubMetadata).BuildAsApiBytes();
            _gitHubApiQuerier.GetRepositoryMetadataAsync(_projectUrl).Returns(gitHubRepositoryMetadata);
            
            var settings = new GetPackageDetailsSettings
            {
                IncludeSourceControlInAuditIfExists = true
            };

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, _packageVersionRange, settings);

            Assert.Equal(_projectUrl, result.ProjectUrl);
            Assert.NotNull(result.SourceControlMetadata);
            Assert.Equal(gitHubMetadata.Archived, result.SourceControlMetadata.IsArchived);
            Assert.Equal(gitHubMetadata.CreatedAt, result.SourceControlMetadata.CreatedTimestamp);
            Assert.Equal(gitHubMetadata.UpdatedAt, result.SourceControlMetadata.UpdatedTimestamp);
            Assert.Equal(gitHubMetadata.PushedAt, result.SourceControlMetadata.PushedTimestamp);
        }

        [Fact]
        public async Task GetPackageDetailsAsync_ReturnsPackageDetails_WithoutSourceControlInformation_IfOptedOut()
        {
            _catalogRootBuilder.WithPackage(new Package
            {
                CatalogEntry = new CatalogEntry
                {
                    Version = _packageVersionRange,
                    IsListed = true,
                    ProjectUrl = _projectUrl
                }
            });
            var catalogRoot = _catalogRootBuilder.BuildAsApiBytes();
            _nuGetApiQuerier.GetRawCatalogRootAsync(_packageId).Returns(catalogRoot);

            var gitHubMetadata = new GitHubRepositoryMetadata
            {
                Archived = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                PushedAt = DateTimeOffset.UtcNow,
            };
            var gitHubRepositoryMetadata = _gitHubRepositoryMetadataBuilder.WithMetadata(gitHubMetadata).BuildAsApiBytes();
            _gitHubApiQuerier.GetRepositoryMetadataAsync(_projectUrl).Returns(gitHubRepositoryMetadata);
            
            var settings = new GetPackageDetailsSettings
            {
                IncludeSourceControlInAuditIfExists = false
            };

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, _packageVersionRange, settings);

            Assert.Equal(_projectUrl, result.ProjectUrl);
            Assert.Null(result.SourceControlMetadata);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("notasuportedgitrepo")]
        public async Task GetPackageDetailsAsync_ReturnsPackageDetails_WithoutSourceControlInformation_IfProjectUrlIsMissingOrInvalid(string projectUrl)
        {
            _catalogRootBuilder.WithPackage(new Package
            {
                CatalogEntry = new CatalogEntry
                {
                    Version = _packageVersionRange,
                    IsListed = true,
                    ProjectUrl = projectUrl
                }
            });
            var catalogRoot = _catalogRootBuilder.BuildAsApiBytes();
            _nuGetApiQuerier.GetRawCatalogRootAsync(_packageId).Returns(catalogRoot);

            var gitHubMetadata = new GitHubRepositoryMetadata
            {
                Archived = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                PushedAt = DateTimeOffset.UtcNow,
            };
            var gitHubRepositoryMetadata = _gitHubRepositoryMetadataBuilder.WithMetadata(gitHubMetadata).BuildAsApiBytes();
            _gitHubApiQuerier.GetRepositoryMetadataAsync(_projectUrl).Returns(gitHubRepositoryMetadata);
            
            var settings = new GetPackageDetailsSettings
            {
                IncludeSourceControlInAuditIfExists = true
            };

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, _packageVersionRange, settings);

            Assert.Equal(projectUrl, result.ProjectUrl);
            Assert.Null(result.SourceControlMetadata);
        }

        [Fact]
        public async Task GetPackageDetailsAsync_ReturnsPackageDetails_WithoutSourceControlInformation_WithErrorInformation_NotOptedOut()
        {
            _catalogRootBuilder.WithPackage(new Package
            {
                CatalogEntry = new CatalogEntry
                {
                    Version = _packageVersionRange,
                    IsListed = true,
                    ProjectUrl = _projectUrl
                }
            });
            var catalogRoot = _catalogRootBuilder.BuildAsApiBytes();
            _nuGetApiQuerier.GetRawCatalogRootAsync(_packageId).Returns(catalogRoot);

            var exception = new Exception("some error");
            _gitHubApiQuerier.GetRepositoryMetadataAsync(_projectUrl).ThrowsForAnyArgs(exception);

            var settings = new GetPackageDetailsSettings
            {
                IncludeSourceControlInAuditIfExists = true,
                IgnoreSourceControlErrors = false
            };

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, _packageVersionRange, settings);

            Assert.Equal(_projectUrl, result.ProjectUrl);
            Assert.True(result.HasError);
            Assert.Equal($"Unexpected error occurred retrieving source control metadata for '{_projectUrl}': some error", result.Error);
            Assert.Null(result.SourceControlMetadata);
        }

        [Fact]
        public async Task GetPackageDetailsAsync_ReturnsPackageDetails_WithoutSourceControlInformation_WithoutErrorInformation_OptedOut()
        {
            _catalogRootBuilder.WithPackage(new Package
            {
                CatalogEntry = new CatalogEntry
                {
                    Version = _packageVersionRange,
                    IsListed = true,
                    ProjectUrl = _projectUrl
                }
            });
            var catalogRoot = _catalogRootBuilder.BuildAsApiBytes();
            _nuGetApiQuerier.GetRawCatalogRootAsync(_packageId).Returns(catalogRoot);

            var exception = new Exception("some error");
            _gitHubApiQuerier.GetRepositoryMetadataAsync(_projectUrl).ThrowsForAnyArgs(exception);

            var settings = new GetPackageDetailsSettings
            {
                IncludeSourceControlInAuditIfExists = true,
                IgnoreSourceControlErrors = true
            };

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, _packageVersionRange, settings);

            Assert.Equal(_projectUrl, result.ProjectUrl);
            Assert.False(result.HasError);
            Assert.Null(result.Error);
            Assert.Null(result.SourceControlMetadata);
        }

        [Fact]
        public async Task GetPackageDetailsAsync_ReturnsPackageDetails_WithSourceControlArchived()
        {
            _catalogRootBuilder.WithPackage(new Package
            {
                CatalogEntry = new CatalogEntry
                {
                    Version = _packageVersionRange,
                    IsListed = true,
                    ProjectUrl = _projectUrl
                }
            });
            var catalogRoot = _catalogRootBuilder.BuildAsApiBytes();
            _nuGetApiQuerier.GetRawCatalogRootAsync(_packageId).Returns(catalogRoot);

            var gitHubMetadata = new GitHubRepositoryMetadata
            {
                Archived = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                PushedAt = DateTimeOffset.UtcNow,
            };
            var gitHubRepositoryMetadata = _gitHubRepositoryMetadataBuilder.WithMetadata(gitHubMetadata).BuildAsApiBytes();
            _gitHubApiQuerier.GetRepositoryMetadataAsync(_projectUrl).Returns(gitHubRepositoryMetadata);

            var settings = new GetPackageDetailsSettings
            {
                IncludeSourceControlInAuditIfExists = true
            };

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, _packageVersionRange, settings);

            Assert.Equal(DeprecatedReason.SourceControlIsArchived, result.DeprecatedReason);
        }

        [Fact]
        public async Task GetPackageDetailsAsync_ReturnsPackageDetails_WithSourceControlBeingStagnant()
        {
            _catalogRootBuilder.WithPackage(new Package
            {
                CatalogEntry = new CatalogEntry
                {
                    Version = _packageVersionRange,
                    IsListed = true,
                    ProjectUrl = _projectUrl
                }
            });
            var catalogRoot = _catalogRootBuilder.BuildAsApiBytes();
            _nuGetApiQuerier.GetRawCatalogRootAsync(_packageId).Returns(catalogRoot);

            var gitHubMetadata = new GitHubRepositoryMetadata
            {
                Archived = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                PushedAt = DateTimeOffset.UtcNow.AddMonths(-7),
            };
            var gitHubRepositoryMetadata = _gitHubRepositoryMetadataBuilder.WithMetadata(gitHubMetadata).BuildAsApiBytes();
            _gitHubApiQuerier.GetRepositoryMetadataAsync(_projectUrl).Returns(gitHubRepositoryMetadata);

            var settings = new GetPackageDetailsSettings
            {
                IncludeSourceControlInAuditIfExists = true
            };

            var result = await _packageAuditor.GetPackageDetailsAsync(_packageId, _packageVersionRange, settings);

            Assert.Equal(DeprecatedReason.SourceControlIsStagnant, result.DeprecatedReason);
        }
    }
}