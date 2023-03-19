using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NuGetPackageAuditor.NuGetApi;

namespace NuGetPackageAuditor
{
    internal class CatalogProvider : ICatalogProvider
    {
        private readonly INuGetApiQuerier _nuGetApiQuerier;

        public CatalogProvider(INuGetApiQuerier nuGetApiQuerier)
        {
            _nuGetApiQuerier = nuGetApiQuerier;
        }

        public async Task<IEnumerable<string>> GetAllVersions(string packageId)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentNullException(nameof(packageId));

            packageId = packageId.ToLowerInvariant();

            var catalogEntries = await GetCatalogEntries(packageId);
            return catalogEntries.Select(c => c.Version);
        }

        public async Task<CatalogEntry> GetCatalogEntry(string packageId, string packageVersion)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentNullException(nameof(packageId));

            if (string.IsNullOrWhiteSpace(packageVersion))
                throw new ArgumentNullException(nameof(packageVersion));

            packageId = packageId.ToLowerInvariant();
            packageVersion = packageVersion.ToLowerInvariant();

            return (await GetCatalogEntries(packageId)).FirstOrDefault(c =>
                string.Equals(c.Version, packageVersion, StringComparison.OrdinalIgnoreCase));
        }

        // Get the CatalogRoot, then for each CatalogPage get the CatalogEntries.
        // If the CatalogPage doesn't contain CatalogEntries, we have to go get them separately from the API.
        private async Task<CatalogEntry[]> GetCatalogEntries(string packageId)
        {
            var catalogEntries = new List<CatalogEntry>();
            var catalogRootBytes = await _nuGetApiQuerier.GetRawCatalogRootAsync(packageId);
            var catalogRoot = JsonSerializer.Deserialize<CatalogRoot>(catalogRootBytes);

            foreach (var catalogPage in catalogRoot.CatalogPages ?? Array.Empty<CatalogPage>())
            {
                if ((catalogPage.Packages == null || !catalogPage.Packages.Any()) && catalogPage.Id?.Contains("/page/") == true)
                {
                    var catalogPageBytes = await _nuGetApiQuerier.GetRawCatalogPageAsync(catalogPage.Id);
                    var newCatalogPage = JsonSerializer.Deserialize<CatalogPage>(catalogPageBytes);

                    catalogEntries.AddRange(newCatalogPage.Packages.Select(package => package.CatalogEntry));
                }
                else
                {
                    catalogEntries.AddRange((catalogPage.Packages ?? Array.Empty<Package>()).Select(package => package.CatalogEntry));
                }
            }

            return catalogEntries.ToArray();
        }
    }
}