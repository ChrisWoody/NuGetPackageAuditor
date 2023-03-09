namespace NuGetPackageAuditor
{
    public enum DeprecatedReason
    {
        /// <summary>
        /// After performing various checks including looking up NuGet.org we're confident the package is not deprecated.
        /// </summary>
        PackageIsNotDeprecated = 0,

        /// <summary>
        /// NuGet.org has reported the package as deprecated. Check '<see cref="PackageDetails.NuGetDeprecationMessage"/>' for more details.
        /// </summary>
        PackageIsMarkedAsDeprecated,
    }
}