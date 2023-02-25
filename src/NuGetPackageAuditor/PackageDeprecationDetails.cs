namespace NuGetPackageAuditor
{
    public class PackageDeprecationDetails
    {
        public DeprecatedReason DeprecatedReason { get; internal set; }
        public string NuGetDeprecationMessage { get; internal set; }
    }
}