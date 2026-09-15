using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Sentry;

namespace BAUERGROUP.Shared.Core.ErrorTracking
{
    /// <summary>
    /// Sentry Error Tracking and Performance Monitoring.
    /// Einfache Integration analog zu BGLogger.
    /// Verwendung:
    /// Standalone: BGErrorTracking.Init("DSN"); BGErrorTracking.CaptureException(ex);
    /// Via BGLogger: BGLogger.Configuration.ErrorTracking = true; (nach Init).
    /// </summary>
    public sealed class BGErrorTracking
    {
        #region Instance
        private IDisposable? _sentryInstance;

        private BGErrorTracking() { }

        ~BGErrorTracking()
        {
            _sentryInstance?.Dispose();
        }
        #endregion

        #region CommonStatic
        private static readonly Lazy<BGErrorTracking> InstanceInternal =
            new Lazy<BGErrorTracking>(() => new BGErrorTracking());

        public static BGErrorTracking Instance => InstanceInternal.Value;

        private static readonly Lazy<BGErrorTrackingConfiguration> InstanceConfiguration =
            new Lazy<BGErrorTrackingConfiguration>(() => new BGErrorTrackingConfiguration());

        public static BGErrorTrackingConfiguration Configuration => InstanceConfiguration.Value;

        public static bool IsInitialized { get; private set; }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialisiert Sentry mit DSN - einfachste Integration
        /// </summary>
        /// <param name="dsn">Sentry DSN aus dem Sentry-Projekt</param>
        public static void Init(string dsn)
        {
            Init(dsn, null);
        }

        /// <summary>
        /// Initialisiert Sentry mit erweiterten Optionen
        /// </summary>
        public static void Init(string dsn, Action<SentryOptions>? configureOptions)
        {
            if (IsInitialized) return;
            if (string.IsNullOrWhiteSpace(dsn)) return;

            Configuration.Dsn = dsn;

            Instance._sentryInstance = SentrySdk.Init(options =>
            {
                options.Dsn = dsn;

                // Standard-Konfiguration
                options.Release = Configuration.Release ?? GetDefaultRelease();
                options.Environment = Configuration.Environment ?? GetDefaultEnvironment();
                options.Debug = Configuration.Debug;
                options.TracesSampleRate = Configuration.TracesSampleRate;
                options.AutoSessionTracking = Configuration.AutoSessionTracking;
                options.MaxBreadcrumbs = Configuration.MaxBreadcrumbs;
                ErrorTrackingCache.Apply(options, Configuration.CacheDirectoryPath, Configuration.MaxCacheItems, Configuration.InitCacheFlushTimeout);

                // Global Mode für statische Verwendung
                options.IsGlobalModeEnabled = true;

                // BeforeSend Hook für Standard-Tags
                options.SetBeforeSend((sentryEvent, hint) =>
                {
                    sentryEvent.SetTag("application", Configuration.ApplicationName);
                    sentryEvent.SetTag("machine", Environment.MachineName);
                    sentryEvent.SetTag("user", Environment.UserName);
                    return sentryEvent;
                });

                // Benutzerdefinierte Optionen anwenden
                configureOptions?.Invoke(options);
            });

            IsInitialized = true;
        }

        private static string GetDefaultRelease()
        {
            try
            {
                var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                var name = assembly.GetName().Name;
                var version = assembly.GetName().Version;
                return $"{name}@{version}";
            }
            catch (Exception)
            {
                return "unknown";
            }
        }

        private static string GetDefaultEnvironment()
        {
#if DEBUG
            return "development";
#else
            return "production";
#endif
        }
        #endregion

        #region Exception Capturing
        /// <summary>
        /// Erfasst eine Exception und sendet sie an Sentry
        /// </summary>
        public static SentryId CaptureException(Exception exception)
        {
            if (!IsInitialized || exception == null) return SentryId.Empty;
            return SentrySdk.CaptureException(exception);
        }

        /// <summary>
        /// Erfasst eine Exception mit zusätzlichem Kontext
        /// </summary>
        public static SentryId CaptureException(Exception exception, Action<Scope> configureScope)
        {
            if (!IsInitialized || exception == null) return SentryId.Empty;
            return SentrySdk.CaptureException(exception, configureScope);
        }

        /// <summary>
        /// Erfasst eine Exception mit Nachricht
        /// </summary>
        public static SentryId CaptureException(Exception exception, string message)
        {
            if (!IsInitialized || exception == null) return SentryId.Empty;
            return SentrySdk.CaptureException(exception, scope =>
            {
                scope.SetExtra("message", message);
            });
        }

        /// <summary>
        /// Erfasst eine Exception mit Level (wird intern vom NLog Target verwendet)
        /// </summary>
        internal static SentryId CaptureException(Exception? exception, string? message, SentryLevel level)
        {
            if (!IsInitialized) return SentryId.Empty;

            if (exception != null)
            {
                return SentrySdk.CaptureException(exception, scope =>
                {
                    scope.Level = level;
                    if (!string.IsNullOrEmpty(message))
                        scope.SetExtra("message", message);
                });
            }
            else if (!string.IsNullOrEmpty(message))
            {
                return SentrySdk.CaptureMessage(message!, level);
            }

            return SentryId.Empty;
        }
        #endregion

        #region Message Capturing
        /// <summary>
        /// Sendet eine Nachricht an Sentry
        /// </summary>
        public static SentryId CaptureMessage(string message, SentryLevel level = SentryLevel.Info)
        {
            if (!IsInitialized || string.IsNullOrEmpty(message)) return SentryId.Empty;
            return SentrySdk.CaptureMessage(message, level);
        }

        /// <summary>
        /// Error-Level Nachricht
        /// </summary>
        public static SentryId Error(string message) => CaptureMessage(message, SentryLevel.Error);

        /// <summary>
        /// Warning-Level Nachricht
        /// </summary>
        public static SentryId Warning(string message) => CaptureMessage(message, SentryLevel.Warning);

        /// <summary>
        /// Info-Level Nachricht
        /// </summary>
        public static SentryId Info(string message) => CaptureMessage(message, SentryLevel.Info);

        /// <summary>
        /// Fatal-Level Nachricht
        /// </summary>
        public static SentryId Fatal(string message) => CaptureMessage(message, SentryLevel.Fatal);

        /// <summary>
        /// Debug-Level Nachricht
        /// </summary>
        public static SentryId Debug(string message) => CaptureMessage(message, SentryLevel.Debug);
        #endregion

        #region Breadcrumbs
        /// <summary>
        /// Fügt einen Breadcrumb hinzu (Trace-Pfad)
        /// </summary>
        public static void AddBreadcrumb(string message,
            string? category = null,
            string? type = null,
            IDictionary<string, string>? data = null,
            BreadcrumbLevel level = BreadcrumbLevel.Info)
        {
            if (!IsInitialized) return;
            SentrySdk.AddBreadcrumb(message, category, type, data, level);
        }

        /// <summary>
        /// Einfacher Breadcrumb
        /// </summary>
        public static void Breadcrumb(string message) => AddBreadcrumb(message);

        /// <summary>
        /// Navigation Breadcrumb
        /// </summary>
        public static void BreadcrumbNavigation(string from, string to)
        {
            AddBreadcrumb($"{from} -> {to}", "navigation", "navigation");
        }

        /// <summary>
        /// User Action Breadcrumb
        /// </summary>
        public static void BreadcrumbUserAction(string action)
        {
            AddBreadcrumb(action, "user", "user");
        }

        /// <summary>
        /// HTTP Request Breadcrumb
        /// </summary>
        public static void BreadcrumbHttp(string method, string url, int? statusCode = null)
        {
            var data = new Dictionary<string, string>
            {
                { "method", method },
                { "url", url }
            };
            if (statusCode.HasValue)
                data["status_code"] = statusCode.Value.ToString();

            AddBreadcrumb($"{method} {url}", "http", "http", data);
        }
        #endregion

        #region User Context
        /// <summary>
        /// Setzt den aktuellen Benutzer
        /// </summary>
        public static void SetUser(string id, string? email = null,
            string? username = null, string? ipAddress = null)
        {
            if (!IsInitialized) return;
            SentrySdk.ConfigureScope(scope =>
            {
                scope.User = new SentryUser
                {
                    Id = id,
                    Email = email,
                    Username = username,
                    IpAddress = ipAddress
                };
            });
        }

        /// <summary>
        /// Entfernt den Benutzerkontext
        /// </summary>
        public static void ClearUser()
        {
            if (!IsInitialized) return;
            SentrySdk.ConfigureScope(scope => scope.User = null!);
        }
        #endregion

        #region Tags & Context
        /// <summary>
        /// Setzt einen globalen Tag
        /// </summary>
        public static void SetTag(string key, string value)
        {
            if (!IsInitialized) return;
            SentrySdk.ConfigureScope(scope => scope.SetTag(key, value));
        }

        /// <summary>
        /// Setzt mehrere Tags
        /// </summary>
        public static void SetTags(IDictionary<string, string> tags)
        {
            if (!IsInitialized || tags == null) return;
            SentrySdk.ConfigureScope(scope =>
            {
                foreach (var tag in tags)
                    scope.SetTag(tag.Key, tag.Value);
            });
        }

        /// <summary>
        /// Fügt Extra-Daten hinzu
        /// </summary>
        public static void SetExtra(string key, object value)
        {
            if (!IsInitialized) return;
            SentrySdk.ConfigureScope(scope => scope.SetExtra(key, value));
        }

        /// <summary>
        /// Setzt strukturierten Kontext
        /// </summary>
        public static void SetContext(string key, object value)
        {
            if (!IsInitialized) return;
            SentrySdk.ConfigureScope(scope => scope.Contexts[key] = value);
        }
        #endregion

        #region Performance Monitoring
        /// <summary>
        /// Startet eine Transaction für Performance Monitoring
        /// </summary>
        public static ITransactionTracer? StartTransaction(string name, string operation)
        {
            if (!IsInitialized) return null;
            return SentrySdk.StartTransaction(name, operation);
        }

        /// <summary>
        /// Startet eine Transaction und gibt ein IDisposable zurück
        /// </summary>
        public static IDisposable Transaction(string name, string operation)
        {
            var transaction = StartTransaction(name, operation);
            return new TransactionScope(transaction);
        }

        private class TransactionScope : IDisposable
        {
            private readonly ITransactionTracer? _transaction;

            public TransactionScope(ITransactionTracer? transaction)
            {
                _transaction = transaction;
            }

            public void Dispose()
            {
                _transaction?.Finish();
            }
        }
        #endregion

        #region Unhandled Exception Reporting
        /// <summary>
        /// Aktiviert automatisches Exception Reporting (analog zu BGLogger)
        /// </summary>
        public static void UnhandledExceptionReporting(bool enabled = true)
        {
            if (enabled)
            {
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
                TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            }
            else
            {
                AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
                TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
            }
        }

        private static void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception != null && IsInitialized)
            {
                SentrySdk.CaptureException(exception, scope =>
                {
                    scope.SetTag("unhandled", "true");
                    scope.SetTag("terminating", e.IsTerminating.ToString());
                    scope.Level = SentryLevel.Fatal;
                });

                // Sicherstellen dass Events gesendet werden vor App-Ende
                SentrySdk.FlushAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
            }
        }

        private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            if (!IsInitialized) return;

            SentrySdk.CaptureException(e.Exception, scope =>
            {
                scope.SetTag("unhandled", "true");
                scope.SetTag("task_observed", e.Observed.ToString());
                scope.Level = SentryLevel.Error;
            });
        }
        #endregion

        #region Flush & Close
        /// <summary>
        /// Sendet alle gepufferten Events sofort
        /// </summary>
        public static void Flush(TimeSpan? timeout = null)
        {
            if (!IsInitialized) return;
            SentrySdk.FlushAsync(timeout ?? TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Async Flush
        /// </summary>
        public static Task FlushAsync(TimeSpan? timeout = null)
        {
            if (!IsInitialized) return Task.CompletedTask;
            return SentrySdk.FlushAsync(timeout ?? TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Beendet Sentry ordnungsgemäß
        /// </summary>
        public static void Close()
        {
            if (!IsInitialized) return;
            Flush();
            Instance._sentryInstance?.Dispose();
            Instance._sentryInstance = null;
            IsInitialized = false;
        }
        #endregion
    }
}
