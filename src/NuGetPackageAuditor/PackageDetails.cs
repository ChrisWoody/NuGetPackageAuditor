﻿namespace NuGetPackageAuditor
{
    public class PackageDetails
    {
        /// <summary>
        /// Indicates if there was an issue trying to populate this <see cref="PackageDetails"/>
        /// </summary>
        public bool HasError { get; internal set; }
        /// <summary>
        /// Information about why there was an issue trying to populate this <see cref="PackageDetails"/>
        /// </summary>
        public string Error { get; internal set; }

        /// <summary>
        /// The NuGet package's Id
        /// </summary>
        public string Id { get; internal set; }
        /// <summary>
        /// The NuGet package's version, resolved from <see cref="PackageDetails.VersionRange"/>
        /// </summary>
        public string Version { get; internal set; }
        /// <summary>
        /// The range of versions defined for clients of this package
        /// </summary>
        public string VersionRange { get; internal set; }

        /// <summary>
        /// Indicates if the package is currently listed on NuGet.org
        /// </summary>
        public bool IsListed { get; internal set; }

        /// <summary>
        /// Indicates if the package is considered deprecated and why
        /// </summary>
        public DeprecatedReason DeprecatedReason { get; internal set; }
        /// <summary>
        /// The deprecation message from NuGet.org if the package is deprecated there
        /// </summary>
        public string NuGetDeprecationMessage { get; internal set; }
        /// <summary>
        /// The list deprecation reasons from NuGet.org if the package is deprecated there
        /// </summary>
        public string[] NuGetDeprecationReasons { get; internal set; }

        internal static PackageDetails BuildError(string errorMessage)
        {
            return new PackageDetails
            {
                HasError = true,
                Error = errorMessage
            };
        }
    }
}