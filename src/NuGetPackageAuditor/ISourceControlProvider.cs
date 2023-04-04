using System.Threading.Tasks;

namespace NuGetPackageAuditor
{
    /// <summary>
    /// Implementations of <see cref="ISourceControlProvider"/> are expected to know how to retrieve metadata from the provided source control url.
    /// </summary>
    public interface ISourceControlProvider
    {
        /// <summary>
        /// For a given source control URL retrieve its metadata to help determine if a NuGet package is deprecated.
        /// </summary>
        /// <param name="sourceControlUrl">The full URL of the source control project</param>
        /// <returns></returns>
        Task<SourceControlMetadata> GetSourceControlMetadataAsync(string sourceControlUrl);
    }
}