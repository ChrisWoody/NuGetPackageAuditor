using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NuGetPackageAuditor.NuGetApi
{
    internal class NuGetApiQuerier : INuGetApiQuerier
    {
        private const string RegistrationBaseUrl = "https://api.nuget.org/v3/registration5-gz-semver2/";

        private readonly IApiQuerierCache _apiQuerierCache;
        private readonly HttpClient _httpClient;

        public NuGetApiQuerier(IApiQuerierCache apiQuerierCache)
        {
            _apiQuerierCache = apiQuerierCache;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(RegistrationBaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<byte[]> GetRawCatalogRootAsync(string packageId)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentNullException(nameof(packageId));

            var cachePayload = await _apiQuerierCache.GetValueOrDefaultAsync(packageId + ".json");
            if (cachePayload != default)
                return cachePayload;

            var response = await _httpClient.GetAsync($"{packageId.ToLowerInvariant()}/index.json");
            response.EnsureSuccessStatusCode();

            var decompressedBytes = await response.Content.DecompressContent();
            await _apiQuerierCache.SaveAsync(packageId + ".json", decompressedBytes);
            return decompressedBytes;
        }

        public async Task<byte[]> GetRawCatalogPageAsync(string catalogPageId)
        {
            if (string.IsNullOrWhiteSpace(catalogPageId))
                throw new ArgumentNullException(nameof(catalogPageId));

            var cleanCatalogPageId = catalogPageId.ToLowerInvariant().Replace(RegistrationBaseUrl, "");

            var split = cleanCatalogPageId.Split(new[] { "/page/" }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 2)
                throw new ArgumentException("Parameter not in the expected format. Requires 'page' in path.", nameof(catalogPageId));

            var cacheName = $"{split[0]}_{split[1].Replace('/', '_')}.json";
            var cachePayload = await _apiQuerierCache.GetValueOrDefaultAsync(cacheName);
            if (cachePayload != default)
                return cachePayload;

            var response = await _httpClient.GetAsync(cleanCatalogPageId);
            response.EnsureSuccessStatusCode();

            var decompressedBytes = await response.Content.DecompressContent();
            await _apiQuerierCache.SaveAsync(cacheName, decompressedBytes);
            return decompressedBytes;
        }
    }
}