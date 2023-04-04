namespace NuGetPackageAuditor
{
    /// <summary>
    /// Allows you to get an implementation of an in-built <see cref="IGitHubApiQuerier"/>.
    /// </summary>
    public static class GitHubApiQuerierFactory
    {
        /// <summary>
        /// Creates a new <see cref="IGitHubApiQuerier"/> with your own version of <see cref="IApiQuerierCache"/>.
        /// This implementation makes requests to GitHub.com.
        /// </summary>
        /// <param name="apiQuerierCache"></param>
        /// <param name="settings">Optional settings to configure how <see cref="IGitHubApiQuerier"/> is used</param>
        /// <returns><see cref="IGitHubApiQuerier"/></returns>
        public static IGitHubApiQuerier Create(IApiQuerierCache apiQuerierCache, GitHubApiQuerierSettings settings = null)
        {
            return new GitHubApiQuerier(apiQuerierCache, settings);
        }

        /// <summary>
        /// Creates a new <see cref="IGitHubApiQuerier"/> with the 'no operation' <see cref="IApiQuerierCache"/>.
        /// This is helpful if you want to always pull the latest information from GitHub.com.
        /// </summary>
        /// <returns><see cref="IGitHubApiQuerier"/></returns>
        public static IGitHubApiQuerier Create(GitHubApiQuerierSettings settings = null)
        {
            return new GitHubApiQuerier(ApiQuerierCacheFactory.CreateNoOpCache(), settings);
        }

        /// <summary>
        /// Creates a new <see cref="IGitHubApiQuerier"/> with the 'in memory' <see cref="IApiQuerierCache"/>.
        /// This is helpful if you want to reduce the number of subsequent requests to GitHub.com.
        /// </summary>
        /// <returns><see cref="IGitHubApiQuerier"/></returns>
        public static IGitHubApiQuerier CreateWithMemoryCache(GitHubApiQuerierSettings settings = null)
        {
            return new GitHubApiQuerier(ApiQuerierCacheFactory.CreateMemoryCache(), settings);
        }

        /// <summary>
        /// Creates a new <see cref="IGitHubApiQuerier"/> with the 'file' <see cref="IApiQuerierCache"/>.
        /// This is helpful if you want to reduce the number of subsequent requests to GitHub.com, but you can also access the files yourself after the cache has been populated.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="settings">Optional settings to configure how <see cref="IGitHubApiQuerier"/> is used</param>
        /// <returns><see cref="IGitHubApiQuerier"/></returns>
        public static IGitHubApiQuerier CreateWithFileCache(string folderPath, GitHubApiQuerierSettings settings = null)
        {
            return new GitHubApiQuerier(ApiQuerierCacheFactory.CreateFileCache(folderPath), settings);
        }
    }
}