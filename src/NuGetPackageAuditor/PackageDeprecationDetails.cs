namespace NuGetPackageAuditor
{
    public class PackageDeprecationDetails
    {
        public bool IsDeprecatedOnNuget { get; set; }
        public string DeprecationMessage { get; set; }
    }
}