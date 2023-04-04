using System.Threading.Tasks;

namespace NuGetPackageAuditor
{
    /// <summary>
    /// Implementations of <see cref="IGitHubApiQuerier"/> are expected to make requests to the GitHub API and return the uncompressed response as a byte[].
    /// </summary>
    public interface IGitHubApiQuerier
    {
        /// <summary>
        /// For a given GitHub repositories URL return the uncompressed byte[] content that should represent <see cref="GitHubRepositoryMetadata"/>.
        /// </summary>
        /// <param name="url">The GitHub repositories full web-URL</param>
        /// <returns>byte[]</returns>
        Task<byte[]> GetRepositoryMetadataAsync(string url);
    }
}