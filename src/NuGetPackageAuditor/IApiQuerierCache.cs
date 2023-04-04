using System.Threading.Tasks;

namespace NuGetPackageAuditor
{
    /// <summary>
    /// To avoid the latency and rate limiting of hitting NuGet.org, responses can be cached using either the inbuilt caches or your own implementation.
    /// </summary>
    public interface IApiQuerierCache
    {
        /// <summary>
        /// Save the response from NuGet.org for a given key (NuGet package's id) to the cache.
        /// </summary>
        /// <param name="key">The NuGet package's id</param>
        /// <param name="value">The response from NuGet.org to save</param>
        /// <returns></returns>
        Task SaveAsync(string key, byte[] value);

        /// <summary>
        /// Attempts to delete an item from the cache for a given key (NuGet package's id)
        /// </summary>
        /// <param name="key">The NuGet package's id</param>
        /// <returns></returns>
        Task DeleteAsync(string key);

        /// <summary>
        /// Attempts to retrieve an item from the cache for a given key (NuGet package's id), if it doesn't exist the default of byte[] is returned instead.
        /// </summary>
        /// <param name="key">The NuGet package's id</param>
        /// <returns>The cached byte array</returns>
        Task<byte[]> GetValueOrDefaultAsync(string key);

        /// <summary>
        /// Clears the entire cache
        /// </summary>
        /// <returns></returns>
        Task ClearAsync();
    }
}