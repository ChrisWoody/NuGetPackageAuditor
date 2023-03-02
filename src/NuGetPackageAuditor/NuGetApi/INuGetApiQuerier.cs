using System.Threading.Tasks;

namespace NuGetPackageAuditor.NuGetApi
{
    /// <summary>
    /// Implementations of <see cref="INuGetApiQuerier"/> are expected to make requests to a NuGet feed, and (if supported) also only update its cache (if you don't need the package to be audited right now).
    /// </summary>
    public interface INuGetApiQuerier
    {
        /// <summary>
        /// For a given NuGet package's id return its <see cref="CatalogRoot"/>, which can be audited or just queried.
        /// </summary>
        /// <param name="packageId">The NuGet package's id</param>
        /// <returns><see cref="CatalogRoot"/></returns>
        Task<CatalogRoot> GetCatalogRootAsync(string packageId);

        /// <summary>
        /// For a given NuGet package's id, update its value in the cache. This allows you to pre-populate the cache to save making requests later, keeping it up to date at a daily cadence for example.
        /// </summary>
        /// <param name="packageId">The NuGet package's id</param>
        /// <returns></returns>
        Task UpdateCacheAsync(string packageId);
    }
}