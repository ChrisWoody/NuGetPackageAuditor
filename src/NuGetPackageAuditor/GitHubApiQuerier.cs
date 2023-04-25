using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NuGetPackageAuditor
{
    internal class GitHubApiQuerier : IGitHubApiQuerier
    {
        private const string GitHubApiUrl = "https://api.github.com/repos/";

        private readonly IApiQuerierCache _apiQuerierCache;
        private readonly HttpClient _httpClient;

        public GitHubApiQuerier(IApiQuerierCache apiQuerierCache, GitHubApiQuerierSettings gitHubApiQuerierSettings)
        {
            _apiQuerierCache = apiQuerierCache;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.BaseAddress = new Uri(GitHubApiUrl);
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AppleWebKit", "537.36"));
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Chrome", "5.0"));
            if (!string.IsNullOrWhiteSpace(gitHubApiQuerierSettings?.ApiPersonalAccessToken))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", gitHubApiQuerierSettings.ApiPersonalAccessToken);
        }

        public async Task<byte[]> GetRepositoryMetadataAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            var urlSplit = url.Split(new []{"://github.com/"}, StringSplitOptions.RemoveEmptyEntries);
            if (urlSplit.Length != 2)
                throw new ArgumentException($"Unexpected format for url '{url}', expected '://github.com/' to exist", nameof(url));
            var orgRepoSplit = urlSplit[1].Split('/');
            if (orgRepoSplit.Length < 2)
                throw new ArgumentException($"Unexpected format for url '{url}', expected org and repository name to exist'", nameof(url));

            var cachePayload = await _apiQuerierCache.GetValueOrDefaultAsync(url + ".json");
            if (cachePayload != default)
                return cachePayload;

            var response = await _httpClient.GetAsync($"{orgRepoSplit[0]}/{orgRepoSplit[1]}");
            if (response.StatusCode != HttpStatusCode.NotModified)
                response.EnsureSuccessStatusCode();

            var decompressedBytes = await response.Content.DecompressContent();
            await _apiQuerierCache.SaveAsync(url + ".json", decompressedBytes);
            return decompressedBytes;
        }
    }
}