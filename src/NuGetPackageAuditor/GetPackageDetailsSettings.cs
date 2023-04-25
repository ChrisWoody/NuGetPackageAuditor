namespace NuGetPackageAuditor
{
    /// <summary>
    /// Allows you to configure how <see cref="PackageAuditor"/> audits packages.
    /// </summary>
    public class GetPackageDetailsSettings
    {
        /// <summary>
        /// If <code>true</code> the <see cref="PackageAuditor"/> will also check the state of the NuGet package's configured source control based on its URL. For example if its GitHub repository is Archived the package could be considered as 'deprecated'.
        /// </summary>
        public bool IncludeSourceControlInAuditIfExists { get; set; }

        /// <summary>
        /// If <code>true</code> the <see cref="PackageAuditor"/> will ignore any errors that occur when trying to retrieve source control metadata. Otherwise if <code>false</code> the <see cref="PackageDetails"/> will be updated with the error details. Auditing of the package will continue regardless.
        /// </summary>
        public bool IgnoreSourceControlErrors { get; set; }
    }
}