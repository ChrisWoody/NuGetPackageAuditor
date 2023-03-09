using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NuGet.Versioning;
using NuGetPackageAuditor.NuGetApi;

namespace NuGetPackageAuditor
{
    public class PackageAuditor
    {
        private readonly INuGetApiQuerier _nuGetApiQuerier;

        public PackageAuditor()
        {
            _nuGetApiQuerier = NuGetApiQuerierFactory.Create();
        }

        public PackageAuditor(INuGetApiQuerier nuGetApiQuerier)
        {
            _nuGetApiQuerier = nuGetApiQuerier;
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
            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentNullException(nameof(packageId));

            if (string.IsNullOrWhiteSpace(packageVersionRange))
                throw new ArgumentNullException(nameof(packageVersionRange));

            var catalogRoot = await GetCatalogRoot(packageId);
            if (catalogRoot == null)
                return PackageDetails.BuildError($"Could not find package with id '{packageId}' on the NuGet Catalog API.");

            if (catalogRoot.CatalogPages == null || !catalogRoot.CatalogPages.Any())
                return PackageDetails.BuildError($"Package with id '{packageId}' found on the NuGet Catalog API but is missing 'CatalogPages'.");

            if (catalogRoot.CatalogPages.Any(c => c.Packages == null || !c.Packages.Any()))
                return PackageDetails.BuildError($"Package with id '{packageId}' found on the NuGet Catalog API but is missing 'Packages'.");

            if (!VersionRange.TryParse(packageVersionRange, out var versionRange))
                return PackageDetails.BuildError($"Package version range of '{packageVersionRange}' is invalid.");

            var packageVersions = GetPackageVersions(catalogRoot);
            var packageVersion = versionRange.FindBestMatch(packageVersions)?.OriginalVersion;

            if (string.IsNullOrWhiteSpace(packageVersion))
                return PackageDetails.BuildError($"The package version of {packageVersionRange} could not be found for '{packageId}'.");

            var catalogEntry = GetCatalogEntry(catalogRoot, packageVersion);
            return new PackageDetails
            {
                Id = packageId,
                VersionRange = packageVersionRange,
                Version = catalogEntry.Version,
                IsListed = catalogEntry.IsListed,
                DeprecatedReason = catalogEntry.Deprecation != null
                    ? DeprecatedReason.PackageIsMarkedAsDeprecated
                    : DeprecatedReason.PackageIsNotDeprecated,
                NuGetDeprecationMessage = catalogEntry.Deprecation?.Message,
                NuGetDeprecationReasons = catalogEntry.Deprecation?.Reasons,
            };
        }
        
        private static IEnumerable<NuGetVersion> GetPackageVersions(CatalogRoot catalogRoot)
        {
            foreach (var catalogPage in catalogRoot.CatalogPages)
            foreach (var catalogPackage in catalogPage.Packages)
                if (NuGetVersion.TryParse(catalogPackage?.CatalogEntry?.Version, out var version))
                    yield return version;
        }

        private static CatalogEntry GetCatalogEntry(CatalogRoot catalogRoot, string packageVersion)
        {
            foreach (var catalogPage in catalogRoot.CatalogPages)
            foreach (var catalogPackage in catalogPage.Packages)
                if (catalogPackage.CatalogEntry?.Version == packageVersion)
                    return catalogPackage.CatalogEntry;

            throw new InvalidDataException($"Could not find CatalogEntry for package version '{packageVersion}', even though we resolved this version from the provided range");
        }

        private async Task<CatalogRoot> GetCatalogRoot(string packageId)
        {
            try
            {
                return await _nuGetApiQuerier.GetCatalogRootAsync(packageId);
            }
            catch (HttpRequestException e)
            {
                if (e.Message == "Response status code does not indicate success: 404 (Not Found).")
                    return null;

                // todo: handle api request limit exceeded and retry

                throw;
            }
        }
    }
}
