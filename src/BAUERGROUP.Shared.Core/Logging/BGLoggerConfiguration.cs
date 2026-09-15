using BAUERGROUP.Shared.Core.Application;
using NLog;
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

        public Boolean Debug
        {
            set
            {
                if (value == true)
                {
                    Targets.AddTarget("DEBUG", TargetDebug);
                    Targets.LoggingRules.Add(LoggingRuleDebug);
                }
                else
                {
                    Targets.LoggingRules.Remove(LoggingRuleDebug);
                    Targets.RemoveTarget("DEBUG");
                }

                Reconfigure();
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
                if (value == true)
                {
                    Targets.AddTarget("NETWORK", TargetNetwork);
                    Targets.LoggingRules.Add(LoggingRuleNetwork);
                }
                else
                {
                    Targets.LoggingRules.Remove(LoggingRuleNetwork);
                    Targets.RemoveTarget("NETWORK");
                }

                Reconfigure();
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
                if (value == true)
                {
                    Targets.AddTarget("MAIL", TargetMail);
                    Targets.LoggingRules.Add(LoggingRuleMail);
                }
                else
                {
                    Targets.LoggingRules.Remove(LoggingRuleMail);
                    Targets.RemoveTarget("MAIL");
                }

                Reconfigure();
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
                if (value == true)
                {
                    Targets.AddTarget("FILE", TargetFile);
                    Targets.LoggingRules.Add(LoggingRuleFile);
                }
                else
                {
                    Targets.LoggingRules.Remove(LoggingRuleFile);
                    Targets.RemoveTarget("FILE");
                }

                Reconfigure();
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
                if (value == true)
                {
                    Targets.AddTarget("NLOGVIEWER", TargetLogViewer);
                    Targets.LoggingRules.Add(LoggingRuleLogViewer);
                }
                else
                {
                    Targets.LoggingRules.Remove(LoggingRuleLogViewer);
                    Targets.RemoveTarget("NLOGVIEWER");
                }

                Reconfigure();
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
                if (value == true)
                {
                    Targets.AddTarget("CONSOLE", TargetConsole);
                    Targets.LoggingRules.Add(LoggingRuleConsole);
                }
                else
                {
                    Targets.LoggingRules.Remove(LoggingRuleConsole);
                    Targets.RemoveTarget("CONSOLE");
                }

                Reconfigure();
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
                if (value == true)
                {
                    Targets.AddTarget("CONSOLECOLORED", TargetConsoleColored);
                    Targets.LoggingRules.Add(LoggingRuleConsoleColored);
                }
                else
                {
                    Targets.LoggingRules.Remove(LoggingRuleConsoleColored);
                    Targets.RemoveTarget("CONSOLECOLORED");
                }

                Reconfigure();
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
                if (value == true)
                {
                    Targets.AddTarget("MEMORY", TargetMemory);
                    Targets.LoggingRules.Add(LoggingRuleMemory);
                }
                else
                {
                    Targets.LoggingRules.Remove(LoggingRuleMemory);
                    Targets.RemoveTarget("MEMORY");
                }

                Reconfigure();
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
                if (value == true)
                {
                    Targets.AddTarget("EVENTLOG", TargetEventlog);
                    Targets.LoggingRules.Add(LoggingRuleEventlog);
                }
                else
                {
                    Targets.LoggingRules.Remove(LoggingRuleEventlog);
                    Targets.RemoveTarget("EVENTLOG");
                }

                Reconfigure();
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
                if (value == true)
                {
                    Targets.AddTarget("TRACE", TargetTrace);
                    Targets.LoggingRules.Add(LoggingRuleTrace);
                }
                else
                {
                    Targets.LoggingRules.Remove(LoggingRuleTrace);
                    Targets.RemoveTarget("TRACE");
                }

                Reconfigure();
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
                if (value == true)
                {
                    Targets.AddTarget("LOGRECEIVERSERVICE", TargetLogReceiverService);
                    Targets.LoggingRules.Add(LoggingRuleLogReceiverService);
                }
                else
                {
                    Targets.LoggingRules.Remove(LoggingRuleLogReceiverService);
                    Targets.RemoveTarget("LOGRECEIVERSERVICE");
                }

                Reconfigure();
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
                if (value == true)
                {
                    Targets.AddTarget("DEBUGGER", TargetDebugger);
                    Targets.LoggingRules.Add(LoggingRuleDebugger);
                }
                else
                {
                    Targets.LoggingRules.Remove(LoggingRuleDebugger);
                    Targets.RemoveTarget("DEBUGGER");
                }

                Reconfigure();
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
                if (value == true)
                {
                    if (string.IsNullOrEmpty(SentryDsn))
                        throw new InvalidOperationException("SentryDsn must be set before enabling ErrorTracking. Set BGLogger.Configuration.SentryDsn first.");

                    // DSN und Optionen setzen
                    TargetErrorTracking.Options.Dsn = SentryDsn;
                    TargetErrorTracking.Options.MinimumEventLevel = SentryMinimumEventLevel;
                    TargetErrorTracking.Options.MinimumBreadcrumbLevel = SentryMinimumBreadcrumbLevel;

                    Targets.AddTarget("ERRORTRACKING", TargetErrorTracking);
                    Targets.LoggingRules.Add(LoggingRuleErrorTracking);
                }
                else
                {
                    Targets.LoggingRules.Remove(LoggingRuleErrorTracking);
                    Targets.RemoveTarget("ERRORTRACKING");
                }

                Reconfigure();
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
            TargetFile.Layout = @"${longdate::universalTime=true} - ${level:uppercase=true}: ${message}${onexception:${newline}EXCEPTION DETAILS\:${newline}${exception:format=ToString}}";
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
