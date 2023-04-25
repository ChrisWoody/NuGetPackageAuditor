using System.Text.Json.Serialization;

namespace NuGetPackageAuditor.NuGetApi
{
    /// <summary>
    /// Representation of NuGet's 'registration' feed.
    /// </summary>
    public class CatalogRoot
    {
        /// <summary>
        /// The collection of catalog pages for a particular registration.
        /// </summary>
        [JsonPropertyName("items")]
        public CatalogPage[] CatalogPages { get; set; }
    }

    /// <summary>
    /// The specific catalog page for a particular registration.
    /// </summary>
    public class CatalogPage
    {
        /// <summary>
        /// Id of the catalog page.
        /// </summary>
        [JsonPropertyName("@id")]
        public string Id { get; set; }
        /// <summary>
        /// The collection of packages group by the catalog page.
        /// </summary>
        [JsonPropertyName("items")]
        public Package[] Packages { get; set; }
    }

    /// <summary>
    /// The specific package, usually unique by it's version.
    /// </summary>
    public class Package
    {
        /// <summary>
        /// Basically metadata of the package's version.
        /// </summary>
        [JsonPropertyName("catalogEntry")]
        public CatalogEntry CatalogEntry { get; set; }
    }

    /// <summary>
    /// Basically metadata of the package's version.
    /// </summary>
    public class CatalogEntry
    {
        /// <summary>
        /// The 'semver' version of the NuGet package.
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; }
        /// <summary>
        /// Flag indicating if this NuGet package's version is listed on the feed.
        /// </summary>
        [JsonPropertyName("listed")] 
        public bool IsListed { get; set; }
        /// <summary>
        /// Object that contains deprecation information for the specific package's version.
        /// </summary>
        [JsonPropertyName("deprecation")]
        public Deprecation Deprecation { get; set; }
        /// <summary>
        /// If configured the project URL for the specific package's version.
        /// </summary>
        [JsonPropertyName("projectUrl")]
        public string ProjectUrl { get; set; }
    }

    /// <summary>
    /// Object that contains deprecation information for the specific package's version.
    /// </summary>
    public class Deprecation
    {
        /// <summary>
        /// A message to the client about the deprecation, usually suggesting alternative packages.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }
        /// <summary>
        /// A collection of reasons why the package version is deprecated.
        /// </summary>
        [JsonPropertyName("reasons")]
        public string[] Reasons { get; set; }
    }
}