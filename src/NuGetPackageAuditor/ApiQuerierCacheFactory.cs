using NuGetPackageAuditor.NuGetApi;

namespace NuGetPackageAuditor
{
    /// <summary>
    /// Allows you to get an implementation of an in-built <see cref="IApiQuerierCache"/>.
    /// </summary>
    public static class ApiQuerierCacheFactory
    {
        /// <summary>
        /// Creates a 'no operation' <see cref="IApiQuerierCache"/> when you don't want or need a cache.
        /// </summary>
        /// <returns><see cref="IApiQuerierCache"/></returns>
        public static IApiQuerierCache CreateNoOpCache()
        {
            return new NoOpApiQuerierCache();
        }

        /// <summary>
        /// Creates an 'in memory' <see cref="IApiQuerierCache"/> which will store responses from the API in memory. Helpful for subsequent requests.
        /// </summary>
        /// <returns><see cref="IApiQuerierCache"/></returns>
        public static IApiQuerierCache CreateMemoryCache()
        {
            return new MemoryApiQuerierCache();
        }

        /// <summary>
        /// Creates a 'file' <see cref="IApiQuerierCache"/> which will store the responses from the API to a file on disk to the specified folder path.
        /// </summary>
        /// <param name="folderPath">The folder path that you want to cache to read/write to.</param>
        /// <returns><see cref="IApiQuerierCache"/></returns>
        public static IApiQuerierCache CreateFileCache(string folderPath)
        {
            return new FileApiQuerierCache(folderPath);
        }
    }
}