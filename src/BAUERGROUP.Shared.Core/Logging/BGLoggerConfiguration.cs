using BAUERGROUP.Shared.Core.Application;
using BAUERGROUP.Shared.Core.ErrorTracking;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using Sentry.NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace BAUERGROUP.Shared.Core.Logging
{
    /// <summary>
    /// Logging Implementation Configuration Manager
    /// </summary>

    public class BGLoggerConfiguration
    {
        public BGLoggerConfiguration()
        {
            //Basic
            if (String.IsNullOrWhiteSpace(GlobalDiagnosticsContext.Get("ApplicationName")))
            {
                ApplicationName = Assembly.GetEntryAssembly()?.GetName().Name ?? "UnknownApp";
            }

            // Initialize LogDirectory with default value if not already set
            if (String.IsNullOrWhiteSpace(GlobalDiagnosticsContext.Get("LogDirectory")))
                LogDirectory = Path.Combine(DefaultApplicationDataFolder, "Logging");

            //Instances
            InitializeTargets();
            InitializeRules();

            //Set default Ports
            NetworkPort = DefaultNetworkPort;
            NLogViewerPort = DefaultNLogViewerPort;

            //Enable Configuration
            LogManager.Configuration = Targets;

            //Apply default settings
            File = true;

            Mail = false;
            Network = false;
            NLogViewer = false;
            Console = false;
            ConsoleColored = false;
            Memory = false;
#if !NETSTANDARD2_0
            Eventlog = false;
#endif
            Trace = false;
            Debugger = false;
            LogReceiverService = false;
            ErrorTracking = false;

            Debug = false;
            #if DEBUG
            Debug = true;
            #endif

            InitializeCustomTargets();
        }

        public static String ApplicationName
        {
            get
            {
                return GlobalDiagnosticsContext.Get("ApplicationName");
            }

            set
            {
                GlobalDiagnosticsContext.Set("ApplicationName", value);
            }
        }

        /// <summary>
        /// Log directory path. If not set, defaults to %ProgramData%\{ApplicationName}\Logging on Windows and to
        /// {ApplicationFolders.ExecutionAutomaticApplicationDataFolder}/Logging on Linux/macOS.
        /// </summary>
        public static String LogDirectory
        {
            get
            {
                return GlobalDiagnosticsContext.Get("LogDirectory");
            }

            set
            {
                GlobalDiagnosticsContext.Set("LogDirectory", value);
            }
        }

        // Windows keeps its historical location (named after ApplicationName, independent of
        // BAUERGROUP_ROAMINGAPPLICATIONDATA) so installed stations do not move their logs.
        // Elsewhere CommonApplicationData is /usr/share, which normal users cannot write.
        private static String DefaultApplicationDataFolder
        {
            get
            {
                return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), ApplicationName)
                    : ApplicationFolders.ExecutionAutomaticApplicationDataFolder;
            }
        }

        /// <summary>
        /// Enables or disables one target together with its <see cref="LoggingRule"/>. Backs every boolean
        /// target property of this class.
        /// </summary>
        /// <remarks>
        /// Idempotent in both directions and self-healing. The guard compares both halves of the state - the
        /// registration of <paramref name="target"/>, which is exactly what the boolean getters report, and the
        /// presence of <paramref name="rule"/> - so setting a property to the value it already has does nothing
        /// at all and does not reconfigure NLog either.
        /// <para>Never adding blindly is what makes the shared rule objects reusable: <c>Targets.LoggingRules</c>
        /// and <c>rule.Targets</c> are plain lists that happily take duplicates. Adding the same rule twice and
        /// removing it once used to leave a copy behind that <see cref="LoggingConfiguration.RemoveTarget(String)"/>
        /// then emptied for good, and re-enabling the property re-added that rule without any target.</para>
        /// <para>The two halves can also drift apart from outside, because <see cref="Targets"/> is public: an
        /// application calling <c>RemoveTarget</c> itself empties the rule but leaves it in the list. Enabling
        /// then repairs the rule instead of appending it a second time, disabling drops the emptied rule.</para>
        /// <para>The order on disable is load-bearing and mirrors <c>SyncLiveConfiguration</c>: the rule is always
        /// removed BEFORE the target, because <c>RemoveTarget</c> strips the target out of every rule still in the
        /// list, which is what would empty the rule for good.</para>
        /// </remarks>
        private void ApplyTarget(Boolean enable, String name, Target target, LoggingRule rule)
        {
            if (enable == Targets.AllTargets.Contains(target) && enable == Targets.LoggingRules.Contains(rule))
                return;

            if (enable)
            {
                if (!rule.Targets.Contains(target))
                    rule.WriteTo(target);

                Targets.AddTarget(name, target);

                if (!Targets.LoggingRules.Contains(rule))
                    Targets.LoggingRules.Add(rule);
            }
            else
            {
                Targets.LoggingRules.Remove(rule);
                Targets.RemoveTarget(name);
            }

            Reconfigure();
        }

        public Boolean Debug
        {
            set
            {
                ApplyTarget(value, "DEBUG", TargetDebug, LoggingRuleDebug);
            }

            get
            {
                return Targets.AllTargets.Contains(TargetDebug);
            }
        }

        public Boolean Network
        {
            set
            {
                ApplyTarget(value, "NETWORK", TargetNetwork, LoggingRuleNetwork);
            }

            get
            {
                return Targets.AllTargets.Contains(TargetNetwork);
            }
        }

        public Boolean Mail
        {
            set
            {
                ApplyTarget(value, "MAIL", TargetMail, LoggingRuleMail);
            }

            get
            {
                return Targets.AllTargets.Contains(TargetMail);
            }
        }

        public Boolean File
        {
            set
            {
                ApplyTarget(value, "FILE", TargetFile, LoggingRuleFile);
            }

            get
            {
                return Targets.AllTargets.Contains(TargetFile);
            }
        }

        public Boolean NLogViewer
        {
            set
            {
                ApplyTarget(value, "NLOGVIEWER", TargetLogViewer, LoggingRuleLogViewer);
            }

            get
            {
                return Targets.AllTargets.Contains(TargetLogViewer);
            }
        }

        public Boolean Console
        {
            set
            {
                ApplyTarget(value, "CONSOLE", TargetConsole, LoggingRuleConsole);
            }

            get
            {
                return Targets.AllTargets.Contains(TargetConsole);
            }
        }

        public Boolean ConsoleColored
        {
            set
            {
                ApplyTarget(value, "CONSOLECOLORED", TargetConsoleColored, LoggingRuleConsoleColored);
            }

            get
            {
                return Targets.AllTargets.Contains(TargetConsoleColored);
            }
        }

        public Boolean Memory
        {
            set
            {
                ApplyTarget(value, "MEMORY", TargetMemory, LoggingRuleMemory);
            }

            get
            {
                return Targets.AllTargets.Contains(TargetMemory);
            }
        }

#if !NETSTANDARD2_0
        public Boolean Eventlog
        {
            set
            {
                ApplyTarget(value, "EVENTLOG", TargetEventlog, LoggingRuleEventlog);
            }

            get
            {
                return Targets.AllTargets.Contains(TargetEventlog);
            }
        }
#endif

        public Boolean Trace
        {
            set
            {
                ApplyTarget(value, "TRACE", TargetTrace, LoggingRuleTrace);
            }

            get
            {
                return Targets.AllTargets.Contains(TargetTrace);
            }
        }

        public Boolean LogReceiverService
        {
            set
            {
                ApplyTarget(value, "LOGRECEIVERSERVICE", TargetLogReceiverService, LoggingRuleLogReceiverService);
            }

            get
            {
                return Targets.AllTargets.Contains(TargetLogReceiverService);
            }
        }

        public Boolean Debugger
        {
            set
            {
                ApplyTarget(value, "DEBUGGER", TargetDebugger, LoggingRuleDebugger);
            }

            get
            {
                return Targets.AllTargets.Contains(TargetDebugger);
            }
        }

        /// <summary>
        /// Aktiviert/Deaktiviert Sentry Error Tracking (offizielle Sentry.NLog Integration)
        /// Voraussetzung: SentryDsn muss gesetzt sein BEVOR ErrorTracking aktiviert wird
        /// </summary>
        public Boolean ErrorTracking
        {
            set
            {
                // Validation and the Sentry options run on every enable, including a redundant one: the
                // caller may have changed the DSN or a level since, and a missing DSN must keep throwing
                // whether or not the target happens to be registered already.
                if (value == true)
                {
                    if (string.IsNullOrEmpty(SentryDsn))
                        throw new InvalidOperationException("SentryDsn must be set before enabling ErrorTracking. Set BGLogger.Configuration.SentryDsn first.");

                    // DSN und Optionen setzen
                    TargetErrorTracking.Options.Dsn = SentryDsn;
                    TargetErrorTracking.Options.MinimumEventLevel = SentryMinimumEventLevel;
                    TargetErrorTracking.Options.MinimumBreadcrumbLevel = SentryMinimumBreadcrumbLevel;
                    ErrorTrackingCache.Apply(TargetErrorTracking.Options, SentryCacheDirectoryPath, SentryMaxCacheItems, SentryInitCacheFlushTimeout);
                }

                ApplyTarget(value, "ERRORTRACKING", TargetErrorTracking, LoggingRuleErrorTracking);
            }

            get
            {
                return Targets.AllTargets.Contains(TargetErrorTracking);
            }
        }

        private string? _sentryDsn;

        /// <summary>
        /// Sentry DSN für Error Tracking. Muss vor Aktivierung von ErrorTracking gesetzt werden.
        /// Beispiel: BGLogger.Configuration.SentryDsn = "https://...@sentry.io/...";
        /// </summary>
        public string? SentryDsn
        {
            get => _sentryDsn;
            set
            {
                _sentryDsn = value;
                if (TargetErrorTracking != null)
                {
                    TargetErrorTracking.Options.Dsn = value;
                }
            }
        }

        /// <summary>
        /// Minimum Log Level ab dem Events an Sentry gesendet werden (default: Error)
        /// </summary>
        public LogLevel SentryMinimumEventLevel
        {
            get => _sentryMinimumEventLevel;
            set
            {
                _sentryMinimumEventLevel = value;
                if (TargetErrorTracking != null)
                {
                    TargetErrorTracking.Options.MinimumEventLevel = value;
                }
            }
        }
        private LogLevel _sentryMinimumEventLevel = LogLevel.Error;

        /// <summary>
        /// Minimum Log Level für Breadcrumbs (default: Debug)
        /// </summary>
        public LogLevel SentryMinimumBreadcrumbLevel
        {
            get => _sentryMinimumBreadcrumbLevel;
            set
            {
                _sentryMinimumBreadcrumbLevel = value;
                if (TargetErrorTracking != null)
                {
                    TargetErrorTracking.Options.MinimumBreadcrumbLevel = value;
                }
            }
        }
        private LogLevel _sentryMinimumBreadcrumbLevel = LogLevel.Debug;

        /// <summary>
        /// Offline-Cache-Ordner für Sentry: Reports werden vor dem Senden auf Disk geschrieben und bei Netzwerkfehlern
        /// oder Prozessende später gesendet (default: {ApplicationFolders.ExecutionAutomaticApplicationDataFolder}/ErrorReports).
        /// null oder leer deaktiviert den Cache. Muss vor Aktivierung von ErrorTracking gesetzt werden.
        /// </summary>
        public string? SentryCacheDirectoryPath { get; set; } = ErrorTrackingCache.DefaultDirectoryPath;

        /// <summary>
        /// Maximale Anzahl gecachter Reports, bei Erreichen wird der älteste verworfen (default: 50)
        /// </summary>
        public int SentryMaxCacheItems { get; set; } = ErrorTrackingCache.DefaultMaxCacheItems;

        /// <summary>
        /// Wartezeit beim SDK-Start auf das Senden gecachter Reports (default: 0, der Anwendungsstart wartet nie)
        /// </summary>
        public TimeSpan SentryInitCacheFlushTimeout { get; set; } = ErrorTrackingCache.DefaultInitCacheFlushTimeout;

        /// <summary>
        /// Sentry Environment (default: basiert auf DEBUG/RELEASE)
        /// </summary>
        public string SentryEnvironment
        {
            get => _sentryEnvironment;
            set
            {
                _sentryEnvironment = value;
                if (TargetErrorTracking != null)
                {
                    TargetErrorTracking.Options.Environment = value;
                }
            }
        }
        private string _sentryEnvironment =
#if DEBUG
            "development";
#else
            "production";
#endif

        private UInt16 _networkPort = 0;
        public UInt16 NetworkPort
        {
            set
            {
                _networkPort = value;
                TargetNetwork.Address = String.Format("udp://127.0.0.1:{0}", _networkPort);
                Reconfigure();
            }

            get
            {
                return _networkPort;
            }
        }

        private ushort _logViewerPort;
        public ushort NLogViewerPort
        {
            get => _logViewerPort;
            set
            {
                _logViewerPort = value;
                TargetLogViewer.Address = $"udp://127.0.0.1:{_logViewerPort}";
                Reconfigure();
            }
        }

        public static UInt16 DefaultNetworkPort { get { return 9898; } }
        public static UInt16 DefaultNLogViewerPort { get { return 9899; } }

        public void AllTargets(bool enable = true)
        {
            Network = Mail = File = Console = Debug = NLogViewer = enable;
        }

        private void InitializeTargets()
        {
            Targets = new LoggingConfiguration();

            TargetFile = new FileTarget();
            // One colon after the renderer name: NLog reads "${longdate::...}" as an empty default property
            // and rejects the whole layout as soon as an application turns LogManager.ThrowExceptions on.
            TargetFile.Layout = @"${longdate:universalTime=true} - ${level:uppercase=true}: ${message}${onexception:${newline}EXCEPTION DETAILS\:${newline}${exception:format=ToString}}";
            // ${dir-separator} instead of a literal backslash, which is a file name character on Linux/macOS
            TargetFile.FileName = "${gdc:item=LogDirectory}${dir-separator}${gdc:item=ApplicationName}.log";
            TargetFile.KeepFileOpen = true;
            // NLog 6.0: ArchiveSuffixFormat is a string.Format pattern ({0} = sequence number, {1} = archive date),
            // not a date format. Archives are named <App>_yyyy-MM-dd_00.log, the active file stays <App>.log.
            TargetFile.ArchiveFileName = "${gdc:item=LogDirectory}${dir-separator}${gdc:item=ApplicationName}.log";
            TargetFile.ArchiveSuffixFormat = "_{1:yyyy-MM-dd}_{0:00}";
            TargetFile.ArchiveEvery = FileArchivePeriod.Day;
            TargetFile.MaxArchiveFiles = 30;
            TargetFile.Encoding = Encoding.Unicode;

            TargetDebug = new DebugTarget();
            TargetDebug.Layout = TargetFile.Layout;

            TargetNetwork = new NetworkTarget();
            TargetNetwork.Address = String.Format("udp://127.0.0.1:{0}", NetworkPort);
            TargetNetwork.Encoding = Encoding.UTF8;
            TargetNetwork.Layout = TargetFile.Layout;

            TargetMail = new MailTarget();
            TargetMail.Subject = String.Format("{0} - Error Report [{1}]", ApplicationName, Environment.MachineName);
            TargetMail.From = @"no-reply@support.bauer-group.com";
            TargetMail.To = @"support@support.bauer-group.com";
            TargetMail.SmtpServer = @"support.bauer-group.com";
            TargetMail.Layout = TargetFile.Layout;

            TargetLogViewer = new Log4JXmlTarget();
            TargetLogViewer.Address = String.Format("udp://127.0.0.1:{0}", NLogViewerPort);

            TargetConsole = new ConsoleTarget();
            TargetConsole.Layout = TargetFile.Layout;

            TargetConsoleColored = new ColoredConsoleTarget();
            TargetConsoleColored.Layout = TargetFile.Layout;

            TargetMemory = new MemoryTarget();
            TargetMemory.Layout = TargetFile.Layout;

#if !NETSTANDARD2_0
            TargetEventlog = new EventLogTarget();
            TargetEventlog.Layout = TargetFile.Layout;
#endif

            TargetTrace = new TraceTarget();
            TargetTrace.Layout = TargetFile.Layout;

            TargetDebugger = new DebuggerTarget();
            TargetDebugger.Layout = TargetFile.Layout;
            
            TargetLogReceiverService = new WebServiceTarget();
            TargetLogReceiverService.Name = "BGLogger Endpoint";
            TargetLogReceiverService.PreAuthenticate = true;
            TargetLogReceiverService.Headers.Add(new MethodCallParameter("Authorization", Layout.FromString("Bearer some_secure_bearrer_token_here"), typeof(String)));
            TargetLogReceiverService.Encoding = Encoding.UTF8;
            TargetLogReceiverService.Protocol = WebServiceProtocol.JsonPost;
            TargetLogReceiverService.Url = new Uri("https://some.endpoint.com/Logs");
            TargetLogReceiverService.Parameters.Add(new MethodCallParameter("ApplicationName", Layout.FromString(ApplicationName), typeof(String)));
            TargetLogReceiverService.Parameters.Add(new MethodCallParameter("AppDomain", Layout.FromString("${appdomain}"), typeof(String)));
            TargetLogReceiverService.Parameters.Add(new MethodCallParameter("UserName", Layout.FromString("${windows-identity}"), typeof(String)));
            TargetLogReceiverService.Parameters.Add(new MethodCallParameter("MachineName", Layout.FromString("${machinename}"), typeof(String)));
            TargetLogReceiverService.Parameters.Add(new MethodCallParameter("BaseDirectory", Layout.FromString("${basedir}"), typeof(String)));
            TargetLogReceiverService.Parameters.Add(new MethodCallParameter("ProcessName", Layout.FromString("${processname:fullName=true}"), typeof(String)));
            TargetLogReceiverService.Parameters.Add(new MethodCallParameter("ProcessID", Layout.FromString("${processid}"), typeof(Int32)));
            TargetLogReceiverService.Parameters.Add(new MethodCallParameter("ThreadName", Layout.FromString("${threadname}"), typeof(String)));
            TargetLogReceiverService.Parameters.Add(new MethodCallParameter("ThreadID", Layout.FromString("${threadid}"), typeof(Int32)));
            TargetLogReceiverService.Parameters.Add(new MethodCallParameter("Exception", Layout.FromString("${exception:format=Message,Stacktrace:maxInnerExceptionLevel=10:innerFormat=Message,Stacktrace}"), typeof(String)));

            // ErrorTracking Target (offizielle Sentry.NLog Integration)
            TargetErrorTracking = new SentryTarget
            {
                Name = "BGErrorTracking",
                Layout = "${message}",
                BreadcrumbLayout = "${logger}: ${message}",
                Options =
                {
                    InitializeSdk = true,
                    MinimumBreadcrumbLevel = _sentryMinimumBreadcrumbLevel,
                    MinimumEventLevel = _sentryMinimumEventLevel,
                    IncludeEventDataOnBreadcrumbs = true,
                    Environment = _sentryEnvironment,
                    AttachStacktrace = true,
                    SendDefaultPii = true
                },
                // User-Context für User-Tracking
                User = new SentryNLogUser
                {
                    Username = "${windows-identity}",
                    IpAddress = "{{auto}}"
                }
            };

            // Tags (indiziert, durchsuchbar in Sentry)
            TargetErrorTracking.Tags.Add(new TargetPropertyWithContext("ApplicationName", ApplicationName));
            TargetErrorTracking.Tags.Add(new TargetPropertyWithContext("MachineName", "${machinename}"));
            TargetErrorTracking.Tags.Add(new TargetPropertyWithContext("ProcessName", "${processname:fullName=true}"));

            // Context Properties (zusätzliche Daten, sichtbar im Event)
            TargetErrorTracking.ContextProperties.Add(new TargetPropertyWithContext("AppDomain", "${appdomain}"));
            TargetErrorTracking.ContextProperties.Add(new TargetPropertyWithContext("BaseDirectory", "${basedir}"));
            TargetErrorTracking.ContextProperties.Add(new TargetPropertyWithContext("ProcessID", "${processid}"));
            TargetErrorTracking.ContextProperties.Add(new TargetPropertyWithContext("ThreadName", "${threadname}"));
            TargetErrorTracking.ContextProperties.Add(new TargetPropertyWithContext("ThreadID", "${threadid}"));
            TargetErrorTracking.ContextProperties.Add(new TargetPropertyWithContext("Logger", "${logger}"));
            TargetErrorTracking.ContextProperties.Add(new TargetPropertyWithContext("CallSite", "${callsite:includeNamespace=true}"));
        }

        private void InitializeRules()
        {
            LoggingRuleFile = new LoggingRule("*", LogLevel.Debug, TargetFile);
            LoggingRuleDebug = new LoggingRule("*", LogLevel.Debug, TargetDebug);
            LoggingRuleNetwork = new LoggingRule("*", LogLevel.Trace, TargetNetwork);
            LoggingRuleMail = new LoggingRule("*", LogLevel.Error, TargetMail);
            LoggingRuleLogViewer = new LoggingRule("*", LogLevel.Trace, TargetLogViewer);
            LoggingRuleConsole = new LoggingRule("*", LogLevel.Trace, TargetConsole);
            LoggingRuleConsoleColored = new LoggingRule("*", LogLevel.Trace, TargetConsoleColored);            
            LoggingRuleMemory = new LoggingRule("*", LogLevel.Trace, TargetMemory);
#if !NETSTANDARD2_0
            LoggingRuleEventlog = new LoggingRule("*", LogLevel.Info, TargetEventlog);
#endif
            LoggingRuleTrace = new LoggingRule("*", LogLevel.Trace, TargetTrace);
            LoggingRuleDebugger = new LoggingRule("*", LogLevel.Debug, TargetDebugger);
            LoggingRuleLogReceiverService = new LoggingRule("*", LogLevel.Error, TargetLogReceiverService);
            LoggingRuleErrorTracking = new LoggingRule("*", LogLevel.Debug, TargetErrorTracking);
        }

        public LoggingConfiguration Targets { get; private set; } = null!;

        // The LIVE target and its rule are private fields, not protected properties: BGLiveTarget is
        // internal, and a protected member of an internal type is CS0053.
        private readonly object _liveSync = new object();
        private readonly BGLiveTarget _targetLive = new BGLiveTarget();
        private LoggingRule? _loggingRuleLive;
        private LogLevel? _liveRuleLevel;

        protected FileTarget TargetFile { get; private set; } = null!;
        protected DebugTarget TargetDebug { get; private set; } = null!;
        protected NetworkTarget TargetNetwork { get; private set; } = null!;
        protected MailTarget TargetMail { get; private set; } = null!;
        protected Log4JXmlTarget TargetLogViewer { get; private set; } = null!;
        protected ConsoleTarget TargetConsole { get; private set; } = null!;
        protected ColoredConsoleTarget TargetConsoleColored { get; private set; } = null!;
        protected MemoryTarget TargetMemory { get; private set; } = null!;
#if !NETSTANDARD2_0
        protected EventLogTarget TargetEventlog { get; private set; } = null!;
#endif
        protected TraceTarget TargetTrace { get; private set; } = null!;
        protected DebuggerTarget TargetDebugger { get; private set; } = null!;
        protected WebServiceTarget TargetLogReceiverService { get; private set; } = null!;
        protected SentryTarget TargetErrorTracking { get; private set; } = null!;

        protected LoggingRule LoggingRuleFile { get; private set; } = null!;
        protected LoggingRule LoggingRuleDebug { get; private set; } = null!;
        protected LoggingRule LoggingRuleNetwork { get; private set; } = null!;
        protected LoggingRule LoggingRuleMail { get; private set; } = null!;
        protected LoggingRule LoggingRuleLogViewer { get; private set; } = null!;
        protected LoggingRule LoggingRuleConsole { get; private set; } = null!;
        protected LoggingRule LoggingRuleConsoleColored { get; private set; } = null!;
        protected LoggingRule? LoggingRuleDebugString { get; private set; }
        protected LoggingRule LoggingRuleMemory { get; private set; } = null!;
#if !NETSTANDARD2_0
        protected LoggingRule LoggingRuleEventlog { get; private set; } = null!;
#endif
        protected LoggingRule LoggingRuleTrace { get; private set; } = null!;
        protected LoggingRule LoggingRuleDebugger { get; private set; } = null!;
        protected LoggingRule LoggingRuleLogReceiverService { get; private set; } = null!;
        protected LoggingRule LoggingRuleErrorTracking { get; private set; } = null!;

        public void Reconfigure()
        {
            LogManager.ReconfigExistingLoggers();
        }

        /// <summary>
        /// Registers an in-process callback that receives every log event at <paramref name="minimumLevel"/> or above.
        /// No UDP, no <c>System.Diagnostics.Trace</c>, no other configuration change.
        /// </summary>
        /// <remarks>
        /// The NLog target "LIVE" and its rule are part of <see cref="Targets"/> only while at least one sink is
        /// registered (reference counted), so there is no cost while no viewer is open. The rule level is the least
        /// restrictive level over all registered sinks and is recomputed on every registration change.
        /// <para>The callback runs synchronously on the logging thread inside NLog's per-target lock. It MUST return
        /// immediately, MUST NOT block, MUST NOT marshal synchronously to a UI thread (disposing the last registration
        /// waits for an in-flight call — a sink that waits on the UI thread while the UI thread disposes deadlocks),
        /// and MUST NOT log (events logged from a sink are dropped by a re-entrancy guard). Exceptions thrown by a sink
        /// are caught per sink and reported to NLog's <c>InternalLogger</c>; other sinks and the caller are unaffected.
        /// Pass <c>BGLogViewBuffer.Post</c> — it satisfies all of this.</para>
        /// <para>A callback also MUST NOT register a live sink and MUST NOT dispose a live-sink registration — not even
        /// its own, so no one-shot sink that unsubscribes itself. Both take the live lock while the callback already
        /// holds NLog's per-target lock and would deadlock against any other thread doing the reverse; both therefore
        /// throw <see cref="InvalidOperationException"/> when called from a callback. Record what the callback saw and
        /// dispose the registration from the thread that owns it.</para>
        /// <para>Only effective while BGLogger owns <c>LogManager.Configuration</c>. An application that assigns
        /// <c>LogManager.Configuration</c> itself closes every BGLogger target, including this one. The target name
        /// "LIVE" is reserved: NLog replaces a same-named target silently, so an application must not add its own
        /// target called "LIVE" to <see cref="Targets"/>.</para>
        /// <para>While registered, every log call at or above the effective level builds and formats a LogEventInfo.
        /// Raise <paramref name="minimumLevel"/> in chatty processes.</para>
        /// <para>Registrations are serialised against each other, but <see cref="BGLoggerConfiguration"/> as a whole is
        /// not thread-safe: the target toggles (<see cref="Memory"/>, <see cref="Network"/>, …) mutate
        /// <c>Targets.LoggingRules</c> without any lock. Do not register or remove a sink while another thread flips a
        /// target on or off.</para>
        /// </remarks>
        /// <param name="sink">Callback invoked for every captured log event.</param>
        /// <param name="minimumLevel">Lowest level this sink receives; null means <c>LogLevel.Trace</c>.</param>
        /// <returns>Disposing removes the sink. Dispose is idempotent and thread-safe.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sink"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Called from inside a live-sink callback.</exception>
        public IDisposable AddLiveSink(Action<BGLogRecord> sink, LogLevel? minimumLevel = null)
        {
            if (sink == null)
                throw new ArgumentNullException(nameof(sink));

            if (BGLiveTarget.IsInWrite)
                throw new InvalidOperationException(
                    "A live sink must not be registered from inside a live-sink callback. " +
                    "Register it from the thread that owns the registration.");

            lock (_liveSync)
            {
                var registration = new BGLiveSink(this, sink, minimumLevel ?? LogLevel.Trace);
                _targetLive.AddSink(registration);

                try
                {
                    SyncLiveConfiguration();
                }
                catch
                {
                    // Roll the NLog configuration back as well: SyncLiveConfiguration may already have added
                    // the target and the rule before failing, and leaving those behind would keep every log
                    // call formatting a LogEventInfo for a target with no sinks.
                    _targetLive.RemoveSink(registration);
                    SyncLiveConfigurationSafely();
                    throw;
                }

                return registration;
            }
        }

        /// <summary>
        /// Number of registered live sinks. 0 means the "LIVE" target is not in the configuration.
        /// Deliberately not derived from <c>Targets.AllTargets</c>, which goes stale when an application
        /// replaces <c>LogManager.Configuration</c>.
        /// </summary>
        public Int32 LiveSinkCount
        {
            get { return _targetLive.SinkCount; }
        }

        internal void RemoveLiveSink(BGLiveSink registration)
        {
            lock (_liveSync)
            {
                _targetLive.RemoveSink(registration);

                // Removal happens through IDisposable.Dispose, which viewers call from window-closing and
                // shutdown handlers: a failing NLog reconfiguration must not escape from there.
                SyncLiveConfigurationSafely();
            }
        }

        // Called only under _liveSync. Reports instead of throwing, so the caller's state stays consistent.
        private void SyncLiveConfigurationSafely()
        {
            try
            {
                SyncLiveConfiguration();
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "BGLoggerConfiguration: the live-sink configuration could not be synchronised.");
            }
        }

        // Called only under _liveSync; the only place that touches NLog for the LIVE target.
        // Ordering is load-bearing: the rule is always removed BEFORE the target, otherwise
        // LoggingConfiguration.RemoveTarget permanently empties rule.Targets. The statement-scoped
        // lock on LoggingRules is only a lock-ordering marker - it is released before RemoveTarget takes
        // the target's own SyncRoot, because nesting the two would invert the lock order. It grants no
        // mutual exclusion: the target toggles of this class mutate the same list without any lock.
        private void SyncLiveConfiguration()
        {
            var level = _targetLive.EffectiveMinimumLevel;
            var current = _loggingRuleLive;

            if (level == null)
            {
                if (current != null)
                {
                    lock (Targets.LoggingRules) { Targets.LoggingRules.Remove(current); }
                    _loggingRuleLive = null;
                    _liveRuleLevel = null;
                }

                Targets.RemoveTarget("LIVE");
                Reconfigure();
                return;
            }

            if (current == null)
            {
                Targets.AddTarget("LIVE", _targetLive);
                var rule = new LoggingRule("*", level, _targetLive);
                lock (Targets.LoggingRules) { Targets.LoggingRules.Add(rule); }
                _loggingRuleLive = rule;
                _liveRuleLevel = level;
                Reconfigure();
                return;
            }

            if (_liveRuleLevel == null || _liveRuleLevel.Ordinal != level.Ordinal)
            {
                var replacement = new LoggingRule("*", level, _targetLive);
                lock (Targets.LoggingRules)
                {
                    Targets.LoggingRules.Remove(current);
                    Targets.LoggingRules.Add(replacement);
                }

                _loggingRuleLive = replacement;
                _liveRuleLevel = level;
                Reconfigure();
            }
        }

        protected virtual void InitializeCustomTargets()
        {
            /*
            var x = new WebServiceTarget();
            x.Encoding = Encoding.UTF8;
            x.Url = new Uri("");
            x.Protocol = WebServiceProtocol.JsonPost;
            */

            /*
            var x = new MethodCallTarget();
            x.ClassName = "";
            x.MethodName = "";
            */

            /*
            var x = new DatabaseTarget();
            */            
        }

        public IList<String> MemoryLogs
        {
            get
            {
                return TargetMemory.Logs;
            }            
        }

        public void MemoryLogsClear()
        {            
            TargetMemory.Logs.Clear();
        }

        public String LogReceiverServiceEndpointAddress
        {
            get { return TargetLogReceiverService.Url?.FixedValue?.AbsoluteUri ?? string.Empty; }
            set { TargetLogReceiverService.Url = new Uri(value); Reconfigure(); }
        }

        public String LogReceiverServiceEndpointConfigurationName
        {
            get { return TargetLogReceiverService.Name; }
            set { TargetLogReceiverService.Name = value; Reconfigure(); }
        }
    }
}
