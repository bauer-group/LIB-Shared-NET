using System;
using System.IO;
using BAUERGROUP.Shared.Core.Application;
using BAUERGROUP.Shared.Core.Logging;
using Sentry;

namespace BAUERGROUP.Shared.Core.ErrorTracking
{
    /// <summary>
    /// Offline cache settings shared by <see cref="BGErrorTracking"/> and the BGLogger Sentry target.
    /// With a cache directory the Sentry SDK writes every report to disk before sending it, so reports
    /// survive network failures and process exit and are sent on the next start.
    /// </summary>
    internal static class ErrorTrackingCache
    {
        /// <summary>
        /// Default number of cached reports. The SDK default is 30 and drops the oldest report regardless of its
        /// level, so an early crash on a station that is offline for a while would be lost to later warnings.
        /// </summary>
        internal const int DefaultMaxCacheItems = 50;

        internal static TimeSpan DefaultInitCacheFlushTimeout => TimeSpan.Zero;

        internal static string DefaultDirectoryPath =>
            Path.Combine(ApplicationFolders.ExecutionAutomaticApplicationDataFolder, "ErrorReports");

        /// <summary>
        /// Applies the cache settings. A null or empty directory disables the cache. A directory that cannot be
        /// created also disables it, because the SDK would otherwise fail inside its transport initialization
        /// and leave error reporting off without any notice.
        /// </summary>
        internal static void Apply(SentryOptions options, string? directoryPath, int maxCacheItems, TimeSpan initCacheFlushTimeout)
        {
            options.MaxCacheItems = maxCacheItems;
            options.InitCacheFlushTimeout = initCacheFlushTimeout;
            options.CacheDirectoryPath = null;

            if (string.IsNullOrWhiteSpace(directoryPath))
                return;

            try
            {
                Directory.CreateDirectory(directoryPath);
                options.CacheDirectoryPath = directoryPath;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                BGLogger.Warn(ex, "Error tracking offline cache disabled: cache directory could not be created. Reports are sent without local caching.");
            }
        }
    }
}
