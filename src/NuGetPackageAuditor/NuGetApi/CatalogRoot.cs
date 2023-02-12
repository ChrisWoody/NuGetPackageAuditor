using System.Text.Json.Serialization;

namespace NuGetPackageAuditor.NuGetApi
{
    public class CatalogRoot
    {
        [JsonPropertyName("items")]
        public CatalogPage[] CatalogPages { get; set; }
    }

    public class CatalogPage
    {
        [JsonPropertyName("items")]
        public Package[] Packages { get; set; }
    }

    public class Package
    {
        [JsonPropertyName("catalogEntry")]
        public PackageDetails PackageDetails { get; set; }
    }

    public class PackageDetails
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("deprecation")]
        public Deprecation Deprecation { get; set; }
    }

    public class Deprecation
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("reasons")]
        public string[] Reasons { get; set; }
    }
}