using System;
using System.IO;
using System.Threading.Tasks;

namespace NuGetPackageAuditor
{
    internal class FileApiQuerierCache : IApiQuerierCache
    {
        private readonly string _folderPath;

        public FileApiQuerierCache(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentNullException(nameof(folderPath));

            if (!Directory.Exists(folderPath))
                throw new ArgumentException($"Folder '{folderPath}' does not exist", nameof(folderPath));

            _folderPath = folderPath;
        }

        public Task SaveAsync(string key, byte[] value)
        {
            File.WriteAllBytes($"{_folderPath}/{key}", value);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string key)
        {
            if (File.Exists($"{_folderPath}/{key}"))
                File.Delete($"{_folderPath}/{key}");
            return Task.CompletedTask;
        }

        public Task<byte[]> GetValueOrDefaultAsync(string key)
        {
            return Task.FromResult(File.Exists($"{_folderPath}/{key}")
                ? File.ReadAllBytes($"{_folderPath}/{key}")
                : default);
        }
    }
}