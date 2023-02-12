using System;
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

        public async Task<bool> IsPackageDeprecatedAsync(string packageId, string packageVersion)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentNullException(nameof(packageId));

            if (string.IsNullOrWhiteSpace(packageVersion))
                throw new ArgumentNullException(nameof(packageVersion));

            var catalogRoot = await _nuGetApiQuerier.GetCatalogRootAsync(packageId);

            foreach (var catalogPage in catalogRoot?.CatalogPages ?? Array.Empty<CatalogPage>())
            {
                foreach (var package in catalogPage?.Packages ?? Array.Empty<Package>())
                {
                    if (package?.PackageDetails?.Version?.Equals(packageVersion, StringComparison.InvariantCultureIgnoreCase) == true)
                    {
                        if (package.PackageDetails.Deprecation != null)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
