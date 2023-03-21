using NuGetPackageAuditor.NuGetApi;

namespace NuGetPackageAuditor
{
    /// <summary>
    /// Allows you to get an implementation of an in-built <see cref="INuGetApiQuerier"/>.
    /// </summary>
    public static class NuGetApiQuerierFactory
    {
        /// <summary>
        /// Creates a new <see cref="INuGetApiQuerier"/> with your own version of <see cref="IApiQuerierCache"/>.
        /// This implementation makes requests to NuGet.org.
        /// </summary>
        /// <param name="apiQuerierCache"></param>
        /// <returns><see cref="INuGetApiQuerier"/></returns>
        public static INuGetApiQuerier Create(IApiQuerierCache apiQuerierCache)
        {
            return new NuGetApiQuerier(apiQuerierCache);
        }

        /// <summary>
        /// Creates a new <see cref="INuGetApiQuerier"/> with the 'no operation' <see cref="IApiQuerierCache"/>.
        /// This is helpful if you want to always pull the latest information from NuGet.org.
        /// </summary>
        /// <returns><see cref="INuGetApiQuerier"/></returns>
        public static INuGetApiQuerier Create()
        {
            return new NuGetApiQuerier(ApiQuerierCacheFactory.CreateNoOpCache());
        }

        /// <summary>
        /// Creates a new <see cref="INuGetApiQuerier"/> with the 'in memory' <see cref="IApiQuerierCache"/>.
        /// This is helpful if you want to reduce the number of subsequent requests to NuGet.org for the same package.
        /// </summary>
        /// <returns><see cref="INuGetApiQuerier"/></returns>
        public static INuGetApiQuerier CreateWithMemoryCache()
        {
            return new NuGetApiQuerier(ApiQuerierCacheFactory.CreateMemoryCache());
        }

        /// <summary>
        /// Creates a new <see cref="INuGetApiQuerier"/> with the 'file' <see cref="IApiQuerierCache"/>.
        /// This is helpful if you want to reduce the number of subsequent requests to NuGet.org for the same package, but you can also access the files yourself after the cache has been populated.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns><see cref="INuGetApiQuerier"/></returns>
        public static INuGetApiQuerier CreateWithFileCache(string folderPath)
        {
            return new NuGetApiQuerier(ApiQuerierCacheFactory.CreateFileCache(folderPath));
        }
    }
}