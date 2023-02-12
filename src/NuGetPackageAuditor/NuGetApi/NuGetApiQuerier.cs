using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace NuGetPackageAuditor.NuGetApi
{
    public class NuGetApiQuerier : INuGetApiQuerier
    {
        private const string RegistrationBaseUrl = "https://api.nuget.org/v3/registration5-gz-semver2";

        private readonly HttpClient _httpClient;

        public NuGetApiQuerier()
        {
            _httpClient = new HttpClient();
        }

        public async Task<CatalogRoot> GetCatalogRootAsync(string packageId)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentNullException(nameof(packageId));

            var response = await _httpClient.GetAsync($"{RegistrationBaseUrl}/{packageId.ToLowerInvariant()}/index.json");
            response.EnsureSuccessStatusCode();
            var contentStream = await response.Content.ReadAsStreamAsync();

            return await JsonSerializer.DeserializeAsync<CatalogRoot>(contentStream);
        }
    }
}