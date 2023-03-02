using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace NuGetPackageAuditor.NuGetApi
{
    internal class MemoryNuGetCache : INuGetCache
    {
        private static readonly ConcurrentDictionary<string, byte[]> Cache = new ConcurrentDictionary<string, byte[]>();

        public Task SaveAsync(string key, byte[] value)
        {
            Cache.AddOrUpdate(key, value, (o, n) => n);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string key)
        {
            Cache.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        public Task<byte[]> GetValueOrDefaultAsync(string key)
        {
            return Task.FromResult(Cache.TryGetValue(key, out var value) ? value : default);
        }
    }
}