using System;
using System.Text.Json.Serialization;

namespace NuGetPackageAuditor
{
    /// <summary>
    /// Representation of GitHub's 'metadata' API response
    /// </summary>
    public class GitHubRepositoryMetadata
    {
        /// <summary>
        /// Identifies if a repository is marked as archived.
        /// </summary>
        [JsonPropertyName("archived")]
        public bool Archived { get; set; }
        /// <summary>
        /// The timestamp for when a repository was created.
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
        /// <summary>
        /// The timestamp for when the repository was last updated.
        /// </summary>
        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
        /// <summary>
        /// The timestamp for when the repository was last pushed to.
        /// </summary>
        [JsonPropertyName("pushed_at")]
        public DateTimeOffset PushedAt { get; set; }
    }
}