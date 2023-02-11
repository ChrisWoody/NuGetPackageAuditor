using System;
using System.Threading.Tasks;

namespace NuGetPackageAuditor
{
    public class PackageAuditor
    {
        public async Task<bool> IsPackageDeprecatedAsync(string packageId)
        {
            return true;
        }
    }
}
