using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NuGetPackageAuditor.NuGetApi
{
    internal class NuGetApiQuerier : INuGetApiQuerier
    {
        private const string RegistrationBaseUrl = "https://api.nuget.org/v3/registration5-gz-semver2/";

        private readonly HttpClient _httpClient;
        private readonly INuGetCache _nuGetCache;

        public NuGetApiQuerier(INuGetCache nuGetCache)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(RegistrationBaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _nuGetCache = nuGetCache;
        }

        public async Task<byte[]> GetRawCatalogRootAsync(string packageId)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentNullException(nameof(packageId));

            var cachePayload = await _nuGetCache.GetValueOrDefaultAsync(packageId + ".json");
            if (cachePayload != default)
                return cachePayload;

            var response = await _httpClient.GetAsync($"{packageId.ToLowerInvariant()}/index.json");
            response.EnsureSuccessStatusCode();

            var decompressedBytes = await DecompressContent(response.Content);
            await _nuGetCache.SaveAsync(packageId + ".json", decompressedBytes);
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
            var cachePayload = await _nuGetCache.GetValueOrDefaultAsync(cacheName);
            if (cachePayload != default)
                return cachePayload;

            var response = await _httpClient.GetAsync(cleanCatalogPageId);
            response.EnsureSuccessStatusCode();

            var decompressedBytes = await DecompressContent(response.Content);
            await _nuGetCache.SaveAsync(cacheName, decompressedBytes);
            return decompressedBytes;
        }

        private static async Task<byte[]> DecompressContent(HttpContent content)
        {
            var contentBytes = await content.ReadAsByteArrayAsync();

            content.Headers.TryGetValues("content-encoding", out var contentEncoding);
            if (contentEncoding != null && contentEncoding.Contains("gzip"))
            {
                using (var ms = new MemoryStream(contentBytes))
                using (var gzipStream = new GZipStream(ms, CompressionMode.Decompress))
                using (var outputMs = new MemoryStream())
                {
                    await gzipStream.CopyToAsync(outputMs);
                    return outputMs.ToArray();
                }

            }

            return contentBytes;
        }
    }
}