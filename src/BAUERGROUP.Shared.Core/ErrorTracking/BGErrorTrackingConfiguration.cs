using System;
using System.Reflection;

namespace BAUERGROUP.Shared.Core.ErrorTracking
{
    /// <summary>
    /// Konfiguration für BGErrorTracking (Sentry)
    /// </summary>
    public class BGErrorTrackingConfiguration
    {
        public BGErrorTrackingConfiguration()
        {
            // Application Name ermitteln
            try
            {
                var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                ApplicationName = assembly.GetName().Name ?? "Unknown";
            }
            catch (Exception)
            {
                ApplicationName = "Unknown";
            }

            // Environment basierend auf Build-Konfiguration
#if DEBUG
            Debug = true;
            Environment = "development";
#else
            Debug = false;
            Environment = "production";
#endif

            // Defaults für Performance Monitoring
            TracesSampleRate = 0.1;      // 10% der Transactions
            AutoSessionTracking = true;
            MaxBreadcrumbs = 100;

            // Minimum Log Level für NLog Target
            MinimumLogLevel = ErrorTrackingLogLevel.Error;
            CaptureExceptionsOnly = false;

            // Offline Cache
            CacheDirectoryPath = ErrorTrackingCache.DefaultDirectoryPath;
            MaxCacheItems = ErrorTrackingCache.DefaultMaxCacheItems;
            InitCacheFlushTimeout = ErrorTrackingCache.DefaultInitCacheFlushTimeout;
        }

        /// <summary>
        /// Offline-Cache-Ordner: Reports werden vor dem Senden auf Disk geschrieben und bei Netzwerkfehlern
        /// oder Prozessende später gesendet (default: {ApplicationFolders.ExecutionAutomaticApplicationDataFolder}/ErrorReports).
        /// null oder leer deaktiviert den Cache. Muss vor <see cref="BGErrorTracking.Init(string)"/> gesetzt werden.
        /// </summary>
        public string? CacheDirectoryPath { get; set; }

        /// <summary>
        /// Maximale Anzahl gecachter Reports, bei Erreichen wird der älteste verworfen (default: 50)
        /// </summary>
        public int MaxCacheItems { get; set; }

        /// <summary>
        /// Wartezeit in Init auf das Senden gecachter Reports (default: 0, der Anwendungsstart wartet nie)
        /// </summary>
        public TimeSpan InitCacheFlushTimeout { get; set; }

        /// <summary>
        /// Sentry DSN (Data Source Name)
        /// </summary>
        public string? Dsn { get; set; }

        /// <summary>
        /// Anwendungsname (automatisch ermittelt)
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Release Version (z.B. "MyApp@1.0.0")
        /// </summary>
        public string? Release { get; set; }

        /// <summary>
        /// Umgebung: production, development, staging, etc.
        /// </summary>
        public string? Environment { get; set; }

        /// <summary>
        /// Debug-Modus für Sentry SDK
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// Sample Rate für Performance Transactions (0.0 - 1.0)
        /// 0.0 = keine, 1.0 = alle
        /// </summary>
        public double TracesSampleRate { get; set; }

        /// <summary>
        /// Automatisches Session Tracking aktivieren
        /// </summary>
        public bool AutoSessionTracking { get; set; }

        /// <summary>
        /// Maximale Anzahl Breadcrumbs
        /// </summary>
        public int MaxBreadcrumbs { get; set; }

        /// <summary>
        /// Minimum Log Level für NLog Target Integration
        /// Nur Logs ab diesem Level werden an Sentry gesendet
        /// </summary>
        public ErrorTrackingLogLevel MinimumLogLevel { get; set; }

        /// <summary>
        /// Wenn true, werden nur Log-Einträge mit Exceptions an Sentry gesendet
        /// Wenn false, werden auch reine Nachrichten gesendet
        /// </summary>
        public bool CaptureExceptionsOnly { get; set; }

        /// <summary>
        /// Zusätzliche Tags die bei jedem Event mitgesendet werden
        /// </summary>
        public Action<System.Collections.Generic.IDictionary<string, string>>? ConfigureTags { get; set; }
    }

    /// <summary>
    /// Log Level für Error Tracking (gemappt auf NLog und Sentry)
    /// </summary>
    public enum ErrorTrackingLogLevel
    {
        /// <summary>
        /// Alle Log-Einträge (Trace und höher)
        /// </summary>
        Trace = 0,

        /// <summary>
        /// Debug und höher
        /// </summary>
        Debug = 1,

        /// <summary>
        /// Info und höher
        /// </summary>
        Info = 2,

        /// <summary>
        /// Warn und höher
        /// </summary>
        Warn = 3,

        /// <summary>
        /// Error und höher (empfohlen)
        /// </summary>
        Error = 4,

        /// <summary>
        /// Nur Fatal
        /// </summary>
        Fatal = 5
    }
}
