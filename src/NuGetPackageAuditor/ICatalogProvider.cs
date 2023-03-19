using System.Collections.Generic;
using System.Threading.Tasks;
using NuGetPackageAuditor.NuGetApi;

namespace NuGetPackageAuditor
{
    /// <summary>
    /// Implementations of <see cref="ICatalogProvider"/> are expected to know how to retrieve information about a NuGet package.
    /// </summary>
    public interface ICatalogProvider
    {
        /// <summary>
        /// For a given NuGet package's id return all of that package's versions.
        /// </summary>
        /// <param name="packageId">The NuGet package's id</param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetAllVersions(string packageId);

        /// <summary>
        /// For a given NuGet package's id and version return the corresponding <see cref="CatalogEntry"/>.
        /// </summary>
        /// <param name="packageId">The NuGet package's id</param>
        /// <param name="packageVersion">The NuGet package's version</param>
        /// <returns></returns>
        Task<CatalogEntry> GetCatalogEntry(string packageId, string packageVersion);
    }
}