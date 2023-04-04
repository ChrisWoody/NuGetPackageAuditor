namespace NuGetPackageAuditor
{
    /// <summary>
    /// Allows you to configure how the the <see cref="IGitHubApiQuerier"/> is used
    /// </summary>
    public class GitHubApiQuerierSettings
    {
        /// <summary>
        /// GitHub.com has tight rate-limits on unauthenticated requests. Provide a Personal Access Token (PAT) here to get a much larger limit.
        /// </summary>
        public string ApiPersonalAccessToken { get; set; }
    }
}