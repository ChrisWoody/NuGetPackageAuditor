using System.Threading.Tasks;

namespace NuGetPackageAuditor.NuGetApi
{
    internal class NoOpNuGetCache : INuGetCache
    {
        public Task SaveAsync(string key, byte[] value)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string key)
        {
            return Task.CompletedTask;
        }

        public Task<byte[]> GetValueOrDefaultAsync(string key)
        {
            return Task.FromResult((byte[]) default);
        }
    }
}