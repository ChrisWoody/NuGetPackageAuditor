namespace NuGetPackageAuditor
{
    public enum DeprecatedReason
    {
        /// <summary>
        /// After performing various checks including looking up NuGet.org and the package's source control, we're confident the package is not deprecated.
        /// </summary>
        NotDeprecated = 0,

        /// <summary>
        /// NuGet.org has reported the package as deprecated, check '<see cref="PackageDetails.NuGetDeprecationMessage"/>' for more details.
        /// </summary>
        DeprecatedOnNuGet,

        /// <summary>
        /// The source control repository configured for the package is marked as archived, check <see cref="PackageDetails.SourceControlMetadata"/> for more details.
        /// </summary>
        SourceControlIsArchived,

        /// <summary>
        /// The source control repository configured for the package hasn't been pushed to for over 6 months, check <see cref="PackageDetails.SourceControlMetadata"/> for more details.
        /// </summary>
        SourceControlIsStagnant,
    }
}