using System.Threading.Tasks;

namespace NuGetPackageAuditor.NuGetApi
{
    public interface INuGetApiQuerier
    {
        Task<CatalogRoot> GetCatalogRootAsync(string packageId);
    }
}