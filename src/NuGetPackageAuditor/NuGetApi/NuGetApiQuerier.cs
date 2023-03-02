using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace NuGetPackageAuditor.NuGetApi
{
    internal class NuGetApiQuerier : INuGetApiQuerier
    {
        private const string RegistrationBaseUrl = "https://api.nuget.org/v3/registration5-gz-semver2";

        private readonly HttpClient _httpClient;
        private readonly INuGetCache _nuGetCache;

        public NuGetApiQuerier(INuGetCache nuGetCache)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(RegistrationBaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _nuGetCache = nuGetCache;
        }

        public async Task<CatalogRoot> GetCatalogRootAsync(string packageId)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentNullException(nameof(packageId));

            var cachePayload = await _nuGetCache.GetValueOrDefaultAsync(packageId);
            if (cachePayload != default)
                return JsonSerializer.Deserialize<CatalogRoot>(cachePayload);

            var response = await _httpClient.GetAsync($"{packageId.ToLowerInvariant()}/index.json");
            response.EnsureSuccessStatusCode();

            var contentBytes = await response.Content.ReadAsByteArrayAsync();
            await _nuGetCache.SaveAsync(packageId, contentBytes);

            using (var ms = new MemoryStream(contentBytes))
            using (var gzipStream = new GZipStream(ms, CompressionMode.Decompress))
            {
                return await JsonSerializer.DeserializeAsync<CatalogRoot>(gzipStream);
            }
        }

        public async Task UpdateCacheAsync(string packageId)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentNullException(nameof(packageId));

            var response = await _httpClient.GetAsync($"{packageId.ToLowerInvariant()}/index.json");
            response.EnsureSuccessStatusCode();

            var contentBytes = await response.Content.ReadAsByteArrayAsync();
            await _nuGetCache.SaveAsync(packageId, contentBytes);
        }
    }
}