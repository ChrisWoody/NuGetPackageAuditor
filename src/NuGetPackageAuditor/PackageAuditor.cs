using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NuGet.Versioning;
using NuGetPackageAuditor.NuGetApi;

namespace NuGetPackageAuditor
{
    public class PackageAuditor
    {
        private readonly ICatalogProvider _catalogProvider;
        private readonly ISourceControlProvider _sourceControlProvider;

        public PackageAuditor()
        {
            _catalogProvider = new CatalogProvider(NuGetApiQuerierFactory.Create());
            _sourceControlProvider = new SourceControlProvider(GitHubApiQuerierFactory.Create());
        }

        public PackageAuditor(INuGetApiQuerier nuGetApiQuerier, IGitHubApiQuerier gitHubApiQuerier)
        {
            _catalogProvider = new CatalogProvider(nuGetApiQuerier);
            _sourceControlProvider = new SourceControlProvider(gitHubApiQuerier);
        }

        /// <summary>
        /// Attempts to retrieve details about the provided package id and version range. Check <see cref="PackageDetails.HasError"/> and <see cref="PackageDetails.Error"/> to identify any issues.
        /// </summary>
        /// <param name="packageId">The NuGet package's id</param>
        /// <param name="packageVersionRange">The NuGet package's version or version range</param>
        /// <returns><see cref="PackageDetails"/></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<PackageDetails> GetPackageDetailsAsync(string packageId, string packageVersionRange)
        {
            return await GetPackageDetailsAsync(packageId, packageVersionRange, new GetPackageDetailsSettings
            {
                IncludeSourceControlInAuditIfExists = false,
                IgnoreSourceControlErrors = false
            });
        }

        /// <summary>
        /// Attempts to retrieve details about the provided package id and version range. Check <see cref="PackageDetails.HasError"/> and <see cref="PackageDetails.Error"/> to identify any issues.
        /// </summary>
        /// <param name="packageId">The NuGet package's id</param>
        /// <param name="packageVersionRange">The NuGet package's version or version range</param>
        /// <param name="getPackageDetailsSettings">Customization options for the audit</param>
        /// <returns><see cref="PackageDetails"/></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<PackageDetails> GetPackageDetailsAsync(string packageId, string packageVersionRange, GetPackageDetailsSettings getPackageDetailsSettings)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentNullException(nameof(packageId));

            if (string.IsNullOrWhiteSpace(packageVersionRange))
                throw new ArgumentNullException(nameof(packageVersionRange));

            if (!VersionRange.TryParse(packageVersionRange, out var versionRange))
                return PackageDetails.BuildError($"Package version range of '{packageVersionRange}' is invalid.");

            var packageVersions = await GetPackageVersions(packageId);
            if (packageVersions == null || !packageVersions.Any())
                return PackageDetails.BuildError($"Could not find package with id '{packageId}' on the NuGet Catalog API.");

            var packageVersion = versionRange.FindBestMatch(packageVersions)?.OriginalVersion;
            if (string.IsNullOrWhiteSpace(packageVersion))
                return PackageDetails.BuildError($"The package version of {packageVersionRange} could not be found for '{packageId}'.");

            var catalogEntry = await GetCatalogEntry(packageId, packageVersion);
            var packageDetails = new PackageDetails
            {
                Id = packageId,
                VersionRange = packageVersionRange,
                Version = catalogEntry.Version,
                IsListed = catalogEntry.IsListed,
                ProjectUrl = catalogEntry.ProjectUrl,
                DeprecatedReason = catalogEntry.Deprecation != null
                    ? DeprecatedReason.PackageIsMarkedAsDeprecated
                    : DeprecatedReason.PackageIsNotDeprecated,
                NuGetDeprecationMessage = catalogEntry.Deprecation?.Message,
                NuGetDeprecationReasons = catalogEntry.Deprecation?.Reasons,
            };

            await PopulateWithSourceControlMetadata(packageDetails, getPackageDetailsSettings, catalogEntry.ProjectUrl);

            return packageDetails;
        }

        private async Task<NuGetVersion[]> GetPackageVersions(string packageId)
        {
            try
            {
                return (await _catalogProvider.GetAllVersions(packageId)).Select(NuGetVersion.Parse).ToArray();
            }
            catch (HttpRequestException e)
            {
                if (e.Message == "Response status code does not indicate success: 404 (Not Found).")
                    return null;

                throw;
            }
        }

        private async Task<CatalogEntry> GetCatalogEntry(string packageId, string packageVersion)
        {
            return await _catalogProvider.GetCatalogEntry(packageId, packageVersion);
        }

        private async Task PopulateWithSourceControlMetadata(PackageDetails packageDetails, GetPackageDetailsSettings getPackageDetailsSettings, string projectUrl)
        {
            if (!getPackageDetailsSettings.IncludeSourceControlInAuditIfExists)
                return;

            if (string.IsNullOrWhiteSpace(projectUrl))
                return;

            try
            {
                var sourceControlMetadata = await _sourceControlProvider.GetSourceControlMetadataAsync(projectUrl);
                packageDetails.SourceControlMetadata = sourceControlMetadata;
            }
            catch (Exception ex)
            {
                if (!getPackageDetailsSettings.IgnoreSourceControlErrors)
                {
                    packageDetails.HasError = true;
                    packageDetails.Error = $"Unexpected error occurred retrieving source control metadata for '{projectUrl}': {ex.Message}";
                }
            }
        }
    }
}
