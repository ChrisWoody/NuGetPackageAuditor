using System.Threading.Tasks;

namespace NuGetPackageAuditor
{
    internal class NoOpApiQuerierCache : IApiQuerierCache
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
            return Task.FromResult((byte[])default);
        }
    }
}