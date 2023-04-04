using System;
using System.Text.Json.Serialization;

namespace NuGetPackageAuditor
{
    public class GitHubRepositoryMetadata
    {
        [JsonPropertyName("archived")]
        public bool Archived { get; set; }
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
        [JsonPropertyName("pushed_at")]
        public DateTimeOffset PushedAt { get; set; }
    }
}