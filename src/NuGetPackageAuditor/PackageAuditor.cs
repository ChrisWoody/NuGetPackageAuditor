using System;
using System.Net.Http;
using System.Threading.Tasks;
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

        public async Task<ResultWrapper<PackageDeprecationDetails>> GetPackageDeprecationDetailsAsync(string packageId, string packageVersion)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentNullException(nameof(packageId));

            if (string.IsNullOrWhiteSpace(packageVersion))
                throw new ArgumentNullException(nameof(packageVersion));

            var catalogRoot = await GetCatalogRoot(packageId);
            if (catalogRoot == null)
                return ResultWrapper<PackageDeprecationDetails>.Failure($"Could not find package with id '{packageId}' on the NuGet Catalog API.");

            if (TryGetPackageForVersion(catalogRoot, packageVersion, out var package))
            {
                if (package.PackageDetails.Deprecation != null)
                {
                    return ResultWrapper<PackageDeprecationDetails>.Success(new PackageDeprecationDetails
                    {
                        DeprecatedReason = DeprecatedReason.PackageIsMarkedAsDeprecated,
                        NuGetDeprecationMessage = package.PackageDetails.Deprecation.Message,
                    });
                }

                return ResultWrapper<PackageDeprecationDetails>.Success(new PackageDeprecationDetails
                {
                    DeprecatedReason = DeprecatedReason.PackageIsNotDeprecated,
                });
            }

            return ResultWrapper<PackageDeprecationDetails>.Failure($"Could not find package version '{packageVersion}' for '{packageId}'.");
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

                throw;
            }
        }

        private static bool TryGetPackageForVersion(CatalogRoot catalogRoot, string packageVersion, out Package package)
        {
            foreach (var catalogPage in catalogRoot?.CatalogPages ?? Array.Empty<CatalogPage>())
            {
                foreach (var catalogPackage in catalogPage?.Packages ?? Array.Empty<Package>())
                {
                    if (catalogPackage?.PackageDetails?.Version?.Equals(packageVersion, StringComparison.InvariantCultureIgnoreCase) == true)
                    {
                        package = catalogPackage;
                        return true;
                    }
                }
            }

            package = null;
            return false;
        }
    }
}
