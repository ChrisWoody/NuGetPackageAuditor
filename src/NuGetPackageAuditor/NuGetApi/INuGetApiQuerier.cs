using System.Threading.Tasks;

namespace NuGetPackageAuditor.NuGetApi
{
    /// <summary>
    /// Implementations of <see cref="INuGetApiQuerier"/> are expected to make requests to a NuGet feed and return the uncompressed response as a byte[].
    /// </summary>
    public interface INuGetApiQuerier
    {
        /// <summary>
        /// For a given NuGet package's id return the uncompressed byte[] content that should represent <see cref="CatalogRoot"/>.
        /// </summary>
        /// <param name="packageId">The NuGet package's id</param>
        /// <returns>byte[]</returns>
        Task<byte[]> GetRawCatalogRootAsync(string packageId);

        /// <summary>
        /// For a given NuGet catalog page's id return the uncompressed byte[] content that should represent <see cref="CatalogPage"/>.
        /// </summary>
        /// <param name="catalogPageId">The NuGet package's id</param>
        /// <returns>byte[]</returns>
        Task<byte[]> GetRawCatalogPageAsync(string catalogPageId);
    }
}