using System;
using System.IO;
using System.Runtime.InteropServices;

namespace BAUERGROUP.Shared.Core.Application
{
    /// <summary>
    /// Provides access to common application folder paths.
    /// </summary>
    public static class ApplicationFolders
    {
        /// <summary>
        /// Gets the roaming application data folder path for the current user.
        /// </summary>
        public static String ApplicationData
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }
        }

        /// <summary>
        /// Gets the directory containing the application binary.
        /// </summary>
        /// <remarks>
        /// Based on <see cref="AppContext.BaseDirectory"/>, because <c>Assembly.Location</c> is empty in single-file applications.
        /// </remarks>
        public static String? ApplicationBinary
        {
            get
            {
                return BaseDirectory;
            }
        }

        /// <summary>
        /// Gets the application name, which is the file name of the application without the extension.
        /// </summary>
        /// <remarks>
        /// Based on the assembly name, because <c>Assembly.Location</c> is empty in single-file applications.
        /// </remarks>
        public static String ApplicationFileNameWithoutExtension
        {
            get
            {
                return ApplicationProperties.AutomaticAssembly.GetName().Name ?? String.Empty;
            }
        }

        private static String BaseDirectory
        {
            get
            {
                return AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
        }

        /// <summary>
        /// Gets the application-specific folder within the roaming application data directory.
        /// </summary>
        public static String ExecutionApplicationDataFolder
        {
            get
            {
                return Path.Combine(ApplicationData, ApplicationFileNameWithoutExtension);
            }
        }

        /// <summary>
        /// Gets the common application data folder path shared by all users.
        /// </summary>
        public static String CommonApplicationData
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            }
        }

        /// <summary>
        /// Gets the application-specific folder within the common application data directory.
        /// </summary>
        public static String ExecutionCommonApplicationDataFolder
        {
            get
            {
                return Path.Combine(CommonApplicationData, ApplicationFileNameWithoutExtension);
            }
        }

        /// <summary>
        /// Gets the directory containing the currently executing assembly.
        /// </summary>
        /// <remarks>
        /// Based on <see cref="AppContext.BaseDirectory"/>, because <c>Assembly.Location</c> is empty in single-file applications.
        /// </remarks>
        public static String? ApplicationExecuting
        {
            get
            {
                return BaseDirectory;
            }
        }

        /// <summary>
        /// Gets the local (non-roaming) application data folder path for the current user.
        /// </summary>
        public static String LocalApplicationData
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
        }

        /// <summary>
        /// Gets the current working directory of the application.
        /// </summary>
        public static String CurrentDirectory
        {
            get
            {
                return Environment.CurrentDirectory;
            }
        }

        /// <summary>
        /// Gets the name of the environment variable that forces data storage in the user's roaming profile.
        /// </summary>
        /// <remarks>
        /// When this environment variable is set to "TRUE", application data is stored in the roaming profile.
        /// </remarks>
        public static String ForceApplicationDataFolderInUserProfileEnvironmentVariable
        {
            get
            {
                return @"BAUERGROUP_ROAMINGAPPLICATIONDATA"; //Environment Variable must be Present and be TRUE
            }
        }

        /// <summary>
        /// Gets the appropriate application data folder based on the operating system and the BAUERGROUP_ROAMINGAPPLICATIONDATA environment variable.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>BAUERGROUP_ROAMINGAPPLICATIONDATA=TRUE (any OS): <see cref="ExecutionApplicationDataFolder"/>.</item>
        /// <item>Windows: <see cref="ExecutionCommonApplicationDataFolder"/> (%ProgramData%\{App}).</item>
        /// <item>Linux/macOS: /var/lib/{App} if that directory exists (created by the installer with the required owner),
        /// otherwise the per-user folder under <see cref="Environment.SpecialFolder.LocalApplicationData"/>
        /// (~/.local/share/{App} on Linux). <see cref="Environment.SpecialFolder.CommonApplicationData"/> maps to
        /// /usr/share there, which normal users cannot write.</item>
        /// </list>
        /// The folder is not created; stores create it when saving.
        /// </remarks>
        public static String ExecutionAutomaticApplicationDataFolder
        {
            get
            {
                return ResolveAutomaticApplicationDataFolder(
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
                    EnvironmentProperties.GetEnvironmentVariable(ForceApplicationDataFolderInUserProfileEnvironmentVariable),
                    ApplicationFileNameWithoutExtension,
                    Directory.Exists);
            }
        }

        private const String UnixSystemApplicationDataRoot = "/var/lib";

        internal static String ResolveAutomaticApplicationDataFolder(Boolean isWindows, String? roamingApplicationDataVariable, String applicationName, Func<String, Boolean> directoryExists)
        {
            if (String.Equals(roamingApplicationDataVariable, "TRUE", StringComparison.OrdinalIgnoreCase))
                return Path.Combine(ApplicationData, applicationName);

            if (isWindows)
                return Path.Combine(CommonApplicationData, applicationName);

            var systemFolder = Path.Combine(UnixSystemApplicationDataRoot, applicationName);
            if (directoryExists(systemFolder))
                return systemFolder;

            // DoNotVerify: without it the runtime returns "" while ~/.local/share does not exist yet
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify), applicationName);
        }

        /// <summary>
        /// Gets the user's personal documents directory (My Documents).
        /// </summary>
        public static String UserPersonalDocumentsDirectory
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
        }
    }
}
