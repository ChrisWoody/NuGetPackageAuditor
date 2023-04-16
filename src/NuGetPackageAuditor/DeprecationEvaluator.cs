using System;

namespace NuGetPackageAuditor
{
    internal static class DeprecationEvaluator
    {
        public static void EvaluateAndSetDeprecation(PackageDetails packageDetails)
        {
            if (packageDetails == null)
                throw new ArgumentNullException(nameof(packageDetails));

            var reason = DeprecatedReason.NotDeprecated;

            if (packageDetails.NuGetDeprecationExists)
                reason = DeprecatedReason.DeprecatedOnNuGet;
            else if (packageDetails.SourceControlMetadata?.IsArchived == true)
                reason = DeprecatedReason.SourceControlIsArchived;
            else if (packageDetails.SourceControlMetadata?.PushedTimestamp < DateTimeOffset.UtcNow.AddMonths(-6))
                reason = DeprecatedReason.SourceControlIsStagnant;

            packageDetails.DeprecatedReason = reason;
        }
    }
}