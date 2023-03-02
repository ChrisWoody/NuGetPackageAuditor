using NuGetPackageAuditor.NuGetApi;

namespace NuGetPackageAuditor
{
    /// <summary>
    /// Allows you to get an implementation of an in-built <see cref="INuGetCache"/>.
    /// </summary>
    public static class NuGetApiCacheFactory
    {
        /// <summary>
        /// Creates a 'no operation' <see cref="INuGetCache"/> when you don't want or need a cache, which means all requests will go to NuGet.org.
        /// </summary>
        /// <returns><see cref="INuGetCache"/></returns>
        public static INuGetCache CreateNoOpCache()
        {
            return new NoOpNuGetCache();
        }

        /// <summary>
        /// Creates an 'in memory' <see cref="INuGetCache"/> which will store responses from NuGet.org in memory.
        /// Helpful for subsequent requests for the same package.
        /// </summary>
        /// <returns><see cref="INuGetCache"/></returns>
        public static INuGetCache CreateMemoryCache()
        {
            return new MemoryNuGetCache();
        }

        /// <summary>
        /// Creates a 'file' <see cref="INuGetCache"/> which will store the responses from NuGet.org to a file on disk to the specified folder path.
        /// A file is created for each package.
        /// </summary>
        /// <param name="folderPath">The folder path that you want to cache to read/write to.</param>
        /// <returns><see cref="INuGetCache"/></returns>
        public static INuGetCache CreateFileCache(string folderPath)
        {
            return new FileNuGetCache(folderPath);
        }
    }
}