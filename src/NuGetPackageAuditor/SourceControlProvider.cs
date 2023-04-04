using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace NuGetPackageAuditor
{
    internal class SourceControlProvider : ISourceControlProvider
    {
        private readonly IGitHubApiQuerier _gitHubApiQuerier;

        public SourceControlProvider(IGitHubApiQuerier gitHubApiQuerier)
        {
            _gitHubApiQuerier = gitHubApiQuerier;
        }

        public async Task<SourceControlMetadata> GetSourceControlMetadataAsync(string sourceControlUrl)
        {
            if (string.IsNullOrWhiteSpace(sourceControlUrl))
                throw new ArgumentNullException(nameof(sourceControlUrl));

            if (sourceControlUrl.StartsWith("http://github.com", StringComparison.InvariantCultureIgnoreCase) ||
                sourceControlUrl.StartsWith("https://github.com", StringComparison.InvariantCultureIgnoreCase))
            {
                var gitHubResponseBytes = await _gitHubApiQuerier.GetRepositoryMetadataAsync(sourceControlUrl);
                var gitHubResponse = JsonSerializer.Deserialize<GitHubRepositoryMetadata>(gitHubResponseBytes);
                return new SourceControlMetadata
                {
                    IsArchived = gitHubResponse.Archived,
                    CreatedTimestamp = gitHubResponse.CreatedAt,
                    UpdatedTimestamp = gitHubResponse.UpdatedAt,
                    PushedTimestamp = gitHubResponse.PushedAt
                };
            }

            return null;
        }
    }
}