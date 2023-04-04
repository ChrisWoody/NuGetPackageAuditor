using System;

namespace NuGetPackageAuditor
{
    /// <summary>
    /// Represents a standard version of the metadata from a variety of source control providers.
    /// </summary>
    public class SourceControlMetadata
    {
        /// <summary>
        /// Indicates if the source controlled repository is marked as archived or not
        /// </summary>
        public bool IsArchived { get; set; }
        /// <summary>
        /// Indicates when the source controlled repository was created
        /// </summary>
        public DateTimeOffset CreatedTimestamp { get; set; }
        /// <summary>
        /// Indicates when the source controlled repository was last updated
        /// </summary>
        public DateTimeOffset UpdatedTimestamp { get; set; }
        /// <summary>
        /// Indicates when the source controlled repository was pushed to
        /// </summary>
        public DateTimeOffset PushedTimestamp { get; set; }
    }
}