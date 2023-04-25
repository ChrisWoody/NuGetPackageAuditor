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

            // If running in the browser (i.e. Blazor), appears the content is decompressed before it gets to this point
            // even though the 'content-encoding' header is still set. Detect if the length of the content is different
            // to what was specified in the header, if it is assume that it is already decompressed.
            content.Headers.TryGetValues("content-length", out var contentLength);
            int.TryParse(contentLength?.FirstOrDefault(), out var length);
            if (length != contentBytes.Length)
                return contentBytes;

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