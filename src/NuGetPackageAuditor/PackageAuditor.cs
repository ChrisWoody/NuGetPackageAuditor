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
            _nuGetApiQuerier = new NuGetApiQuerier();
        }

        public PackageAuditor(INuGetApiQuerier nuGetApiQuerier)
        {
            _nuGetApiQuerier = nuGetApiQuerier;
        }

        public async Task<ResultWrapper<bool>> IsPackageDeprecatedAsync(string packageId, string packageVersion)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentNullException(nameof(packageId));

            if (string.IsNullOrWhiteSpace(packageVersion))
                throw new ArgumentNullException(nameof(packageVersion));

            CatalogRoot catalogRoot;
            try
            {
                catalogRoot = await _nuGetApiQuerier.GetCatalogRootAsync(packageId);
            }
            catch (HttpRequestException e)
            {
                if (e.Message == "Response status code does not indicate success: 404 (Not Found).")
                    return ResultWrapper<bool>.Failure($"Could not find package with id '{packageId}' on the NuGet Catalog API.");

                throw;
            }

            Package packageWithVersionFound = null;

            foreach (var catalogPage in catalogRoot?.CatalogPages ?? Array.Empty<CatalogPage>())
            {
                foreach (var package in catalogPage?.Packages ?? Array.Empty<Package>())
                {
                    if (package?.PackageDetails?.Version?.Equals(packageVersion, StringComparison.InvariantCultureIgnoreCase) == true)
                    {
                        packageWithVersionFound = package;
                    }
                }
            }

            return packageWithVersionFound == null
                ? ResultWrapper<bool>.Failure($"Could not find package version '{packageVersion}' for '{packageId}'.")
                : ResultWrapper<bool>.Success(packageWithVersionFound.PackageDetails.Deprecation != null);
        }
    }
}
