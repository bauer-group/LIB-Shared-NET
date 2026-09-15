# BAUER GROUP Shared Libraries

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-512BD4)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/badge/NuGet-Ready-004880)](https://www.nuget.org/)

A comprehensive multi-target .NET shared library platform providing essential building blocks for enterprise applications within the BAUER GROUP ecosystem. Supports .NET 10, .NET 8, and .NET Standard 2.0 for maximum compatibility.

---

## Overview

The BAUER GROUP Shared Libraries form a modular, multi-project solution designed to accelerate development across the organization. It provides battle-tested implementations for common enterprise requirements including logging, data persistence, API integrations, cloud services, and desktop UI components.

### Key Features

- **Multi-Target Support**: .NET 10, .NET 8, and .NET Standard 2.0 for broad compatibility
- **Modular Architecture**: Pick only the packages you need
- **Enterprise-Ready Logging**: NLog-based logging with Sentry integration for error tracking
- **Data Layer**: Support for SQLite (persistent key-value storage), LiteDB, and in-memory databases
- **API Integrations**: Generic REST API client with RestSharp
- **Cloud Services**: Cloudinary media management, RemoveBG background removal, Fixer.io currency exchange
- **Desktop Components**: WPF/WinForms utilities with embedded Chromium browser support
- **Reporting**: Stimulsoft Reports integration for professional reporting

---

## Packages

| Package | Target Frameworks | Description |
|---------|-------------------|-------------|
| `BAUERGROUP.Shared.Core` | net10.0, net8.0, netstandard2.0 | Core utilities, extensions, logging (NLog + Sentry), resilience patterns (Polly) |
| `BAUERGROUP.Shared.Data` | net10.0, net8.0, netstandard2.0 | Data persistence: SQLite key-value storage, LiteDB, in-memory database (NMemory) |
| `BAUERGROUP.Shared.API` | net10.0, net8.0, netstandard2.0 | Generic REST API client (RestSharp) with JSON serialization |
| `BAUERGROUP.Shared.Cloud` | net10.0, net8.0 | Cloud services: Cloudinary, RemoveBG, Fixer.io currency exchange |
| `BAUERGROUP.Shared.Desktop` | net10.0-windows, net8.0-windows | WPF/WinForms utilities, behaviors, reactive extensions |
| `BAUERGROUP.Shared.Desktop.Browser` | net10.0-windows, net8.0-windows | Embedded Chromium browser (CefSharp) and WebView2 for WPF |
| `BAUERGROUP.Shared.Desktop.Reporting` | net10.0-windows, net8.0-windows | Stimulsoft Reports integration* |

*\*Requires separate Stimulsoft license*

---

## Installation

### Via NuGet Package Manager

```powershell
# Core package (required)
Install-Package BAUERGROUP.Shared.Core

# Optional packages
Install-Package BAUERGROUP.Shared.Data
Install-Package BAUERGROUP.Shared.API
Install-Package BAUERGROUP.Shared.Cloud
Install-Package BAUERGROUP.Shared.Desktop
Install-Package BAUERGROUP.Shared.Desktop.Browser
Install-Package BAUERGROUP.Shared.Desktop.Reporting
```

### Via .NET CLI

```bash
dotnet add package BAUERGROUP.Shared.Core
```

---

## Quick Start

### Logging with BGLogger

```csharp
using BAUERGROUP.Shared.Core.Logging;

// Optional: override name and log folder before BGLogger is first used
// (defaults: executable name, see "Data, Log and Error-Report Folders")
BGLoggerConfiguration.ApplicationName = "MyApplication";
BGLoggerConfiguration.LogDirectory = @"C:\Logs";

// Enable Sentry error tracking (optional)
BGLogger.Configuration.SentryDsn = "https://your-sentry-dsn@sentry.io/project";
BGLogger.Configuration.SentryEnvironment = "production";
BGLogger.Configuration.ErrorTracking = true;

// Enable additional targets as needed
BGLogger.Configuration.Console = true;
BGLogger.Configuration.File = true; // Enabled by default

// Use the logger
BGLogger.Info("Application started");
BGLogger.Error(exception, "An error occurred");

// Enable automatic unhandled exception reporting
BGLogger.UnhandledExceptionReporting(true);
```

### Data Persistence with SQLite

```csharp
using BAUERGROUP.Shared.Data.EmbeddedDatabase;

// Create a thread-safe, persistent key-value dictionary backed by SQLite
using var storage = new ConcurrentPersistentDictionary<string, MyData>(
    dataStorageDirectory: @"C:\Data",
    databaseName: "MyDatabase",
    tableName: "MyTable"
);

// Store and retrieve data
storage.Create("key1", new MyData { Name = "Example" });
var data = storage.Read("key1");
bool exists = storage.Exists("key1");
storage.Delete("key1");

// Read all entries
var allData = storage.Read();
var allWithKeys = storage.ReadWithKeys();
```

### Embedded Browser (WPF)

```csharp
using BAUERGROUP.Shared.Desktop.Browser;

// Show embedded Chrome browser window (non-blocking)
WPFToolboxBrowser.ChromeEmbeddedWebbrowserWindow(
    title: "Web View",
    url: "https://example.com",
    owner: this,
    wait: false
);

// Show browser and wait for it to close (modal dialog)
WPFToolboxBrowser.ChromeEmbeddedWebbrowserWindow(
    title: "Web View",
    url: "https://example.com",
    owner: this,
    wait: true
);

// Take a screenshot of a website
await WPFToolboxBrowser.MakeWebsiteScreenshot(
    "https://example.com",
    "screenshot.png"
);
```

---

## Requirements

### Runtime Requirements

- **.NET 10.0**, **.NET 8.0**, or **.NET Standard 2.0** compatible runtime
- **Windows** (for Desktop packages: `Desktop`, `Desktop.Browser`, `Desktop.Reporting`)

### Optional Requirements

- **Stimulsoft License** - Required for `BAUERGROUP.Shared.Desktop.Reporting`
- **Sentry Account** - For error tracking integration

---

## Project Structure

```
BAUERGROUP.Shared/
├── src/
│   ├── BAUERGROUP.Shared.Core/                 # Core utilities & logging
│   ├── BAUERGROUP.Shared.Data/                 # Data persistence layer
│   ├── BAUERGROUP.Shared.API/                  # Generic REST API client
│   ├── BAUERGROUP.Shared.Cloud/                # Cloud service integrations
│   ├── BAUERGROUP.Shared.Desktop/              # WPF/WinForms utilities
│   ├── BAUERGROUP.Shared.Desktop.Browser/      # Embedded browser
│   └── BAUERGROUP.Shared.Desktop.Reporting/    # Reporting components
├── tests/
│   └── BAUERGROUP.Shared.Test/                 # Unit tests
├── assets/                                     # Application icons
├── docs/
│   ├── BUILD.md                                # Build documentation
│   ├── DEPENDENCY-LICENSES.md                  # License analysis
│   ├── DOCUMENTATION-PLATFORM-SPEC.md          # Platform specification
│   ├── INSTALLATION.md                         # Installation guide
│   └── VERSIONING.md                           # Version management
├── CHANGELOG.md                                # Version history
├── Directory.Build.props                       # Shared build configuration
├── Directory.Packages.props                    # Central package management
└── BAUERGROUP.Shared.slnx                      # Solution file
```

---

## Building from Source

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (includes support for .NET 8 and earlier)
- Visual Studio 2022 (17.12+) or JetBrains Rider 2024.3+

### Build

```bash
# Clone the repository
git clone https://github.com/bauer-group/LIB-Shared-NET.git
cd LIB-Shared-NET

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test
```

### Creating NuGet Packages

```bash
dotnet pack --configuration Release
```

Packages will be output to `bin/Release/*.nupkg`

---

## Configuration

### NLog Configuration

The library uses NLog for logging. Configuration is done programmatically via `BGLogger.Configuration`:

```csharp
// Available logging targets (can be enabled/disabled at runtime)
BGLogger.Configuration.File = true;           // File logging (default: enabled)
BGLogger.Configuration.Console = true;        // Console output
BGLogger.Configuration.ConsoleColored = true; // Colored console output
BGLogger.Configuration.Network = true;        // UDP network logging
BGLogger.Configuration.NLogViewer = true;     // NLog Viewer (Log4J XML format)
BGLogger.Configuration.Memory = true;         // In-memory log storage
BGLogger.Configuration.Debugger = true;       // VS Debugger output
BGLogger.Configuration.Eventlog = true;       // Windows Event Log (.NET only)
BGLogger.Configuration.ErrorTracking = true;  // Sentry integration
```

### Sentry Integration

Sentry error tracking is integrated via the official `Sentry.NLog` package:

```csharp
// Configure Sentry (must set DSN before enabling ErrorTracking)
BGLogger.Configuration.SentryDsn = "https://...@sentry.io/...";
BGLogger.Configuration.SentryEnvironment = "production";
BGLogger.Configuration.SentryMinimumEventLevel = NLog.LogLevel.Error;
BGLogger.Configuration.SentryMinimumBreadcrumbLevel = NLog.LogLevel.Debug;

// Offline cache (enabled by default): unsent reports survive network failures and process exit
BGLogger.Configuration.SentryCacheDirectoryPath = "/custom/ErrorReports"; // null or "" disables the cache
BGLogger.Configuration.SentryMaxCacheItems = 50;                         // oldest report is dropped beyond this
BGLogger.Configuration.SentryInitCacheFlushTimeout = TimeSpan.Zero;      // start never waits for the cache

BGLogger.Configuration.ErrorTracking = true;
```

Features:

- Automatic integration with NLog pipeline
- Breadcrumbs, context, and event capture
- User tracking with Windows identity
- Tags for ApplicationName, MachineName, ProcessName
- Offline cache: reports are written to disk before sending and sent on the next start if they could not be delivered. A cache folder that cannot be created disables the cache with a logged warning; reporting continues online.

The standalone `BGErrorTracking.Init(dsn)` API uses the same defaults via `BGErrorTracking.Configuration.CacheDirectoryPath`, `MaxCacheItems` and `InitCacheFlushTimeout`.

### Data, Log and Error-Report Folders

Settings stores, the embedded database, the default log directory and the Sentry offline cache all live below one application data folder. `<App>` is the entry assembly name (also correct in single-file applications); `<ApplicationName>` is `BGLoggerConfiguration.ApplicationName`, which defaults to the same value.

| What | Windows | Linux / macOS |
|---|---|---|
| Data folder (`ApplicationFolders.ExecutionAutomaticApplicationDataFolder`) | `%ProgramData%\<App>` | `/var/lib/<App>` if that directory exists, otherwise `~/.local/share/<App>` (per user) |
| Data folder with `BAUERGROUP_ROAMINGAPPLICATIONDATA=TRUE` | `%APPDATA%\<App>` | `~/.config/<App>` |
| Default log directory (`BGLoggerConfiguration.LogDirectory`) | `%ProgramData%\<ApplicationName>\Logging` (always, see note) | `<data folder>/Logging` |
| Log files | `<ApplicationName>.log`; daily archives `<ApplicationName>_yyyy-MM-dd_00.log`, 30 kept | same |
| Sentry offline cache | `<data folder>\ErrorReports` | `<data folder>/ErrorReports` |

**Windows log directory:** it deliberately keeps its historical location so installed stations do not move their logs. It is named after `ApplicationName` (not the executable) and ignores `BAUERGROUP_ROAMINGAPPLICATIONDATA`, so with the variable set, settings live in `%APPDATA%` while logs stay in `%ProgramData%`. Set `BGLoggerConfiguration.LogDirectory` explicitly to place logs elsewhere.

**Linux stations:** `/usr/share` (what .NET reports as `CommonApplicationData`) is never used, because normal users cannot write there. For one shared folder per station, let the installer create `/var/lib/<App>` with the owner the application runs as, e.g. `install -d -o <user> /var/lib/<App>` or systemd `StateDirectory=<App>`. Without it, each OS user gets their own folder. Folders are resolved without being created; stores and the logger create them on first write.

---

## Third-Party Licenses

This project uses various open-source packages. See [DEPENDENCY-LICENSES.md](docs/DEPENDENCY-LICENSES.md) for a complete license analysis.

### Key Dependencies

| Package | License | Notes |
|---------|---------|-------|
| NLog | BSD 3-Clause | Logging framework |
| CefSharp | BSD 3-Clause | Chromium browser |
| Sentry | MIT | Error tracking |
| Stimulsoft | Proprietary | Requires separate license |

---

## Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Code Style

- Follow Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful names for variables, methods, and classes
- Write XML documentation for public APIs
- Include unit tests for new functionality

---

## Support

For questions or issues:

- **Internal**: Contact BAUER GROUP Development Team
- **GitHub Issues**: [Report an issue](https://github.com/bauer-group/LIB-Shared-NET/issues)

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

**Note**: Some dependencies (Stimulsoft) require separate commercial licenses.
See [DEPENDENCY-LICENSES.md](docs/DEPENDENCY-LICENSES.md) for details.

---

**BAUER GROUP** - *Building Better Software Together*
