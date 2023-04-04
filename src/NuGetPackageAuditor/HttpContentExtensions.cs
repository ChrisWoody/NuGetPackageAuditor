using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NuGetPackageAuditor
{
    internal static class HttpContentExtensions
    {
        internal static async Task<byte[]> DecompressContent(this HttpContent content)
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