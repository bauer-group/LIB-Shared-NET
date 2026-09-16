# Installation Guide

This documentation describes the installation and usage of BAUERGROUP.Shared NuGet packages.

---

## Available Packages

| Package | Description | Target Frameworks |
|---------|-------------|-------------------|
| `BAUERGROUP.Shared.Core` | Core utilities, logging, extensions | net10.0, net8.0, netstandard2.0 |
| `BAUERGROUP.Shared.API` | REST API client | net10.0, net8.0, netstandard2.0 |
| `BAUERGROUP.Shared.Data` | Data persistence (SQLite, LiteDB) | net10.0, net8.0, netstandard2.0 |
| `BAUERGROUP.Shared.Cloud` | Cloud services (Cloudinary, Remove.bg, Fixer.io) | net10.0, net8.0 |
| `BAUERGROUP.Shared.Avalonia` | Cross-platform Avalonia UI components (Windows, Linux, macOS), incl. live log viewer | net10.0, net8.0 |
| `BAUERGROUP.Shared.Desktop` | WPF/WinForms utilities | net10.0-windows, net8.0-windows |
| `BAUERGROUP.Shared.Desktop.Browser` | Embedded browser (CefSharp, WebView2) | net10.0-windows, net8.0-windows |
| `BAUERGROUP.Shared.Desktop.Reporting` | Reporting (Stimulsoft)* | net10.0-windows, net8.0-windows |

*\*Requires separate Stimulsoft license*

---

## Installation from NuGet.org

The packages are publicly available on NuGet.org - no configuration required.

### .NET CLI

```bash
# Core package (recommended as base)
dotnet add package BAUERGROUP.Shared.Core

# Additional packages as needed
dotnet add package BAUERGROUP.Shared.API
dotnet add package BAUERGROUP.Shared.Data
dotnet add package BAUERGROUP.Shared.Cloud
dotnet add package BAUERGROUP.Shared.Avalonia
dotnet add package BAUERGROUP.Shared.Desktop
dotnet add package BAUERGROUP.Shared.Desktop.Browser
dotnet add package BAUERGROUP.Shared.Desktop.Reporting
```

### Package Manager Console (Visual Studio)

```powershell
Install-Package BAUERGROUP.Shared.Core
Install-Package BAUERGROUP.Shared.API
Install-Package BAUERGROUP.Shared.Data
Install-Package BAUERGROUP.Shared.Cloud
Install-Package BAUERGROUP.Shared.Avalonia
```

### PackageReference (csproj)

```xml
<ItemGroup>
  <PackageReference Include="BAUERGROUP.Shared.Core" Version="2.*" />
  <PackageReference Include="BAUERGROUP.Shared.Data" Version="2.*" />
</ItemGroup>
```

---

## Installation from GitHub Packages

The packages are also available on GitHub Packages.

> **Important:** GitHub Packages requires authentication even for public packages. This is a GitHub platform limitation.

### Step 1: Create a Personal Access Token (PAT)

Create a read-only token for package access:

1. Go to [GitHub Settings → Developer settings → Personal access tokens → Tokens (classic)](https://github.com/settings/tokens)
2. Click **"Generate new token (classic)"**
3. Configure the token:
   - **Note:** `nuget-read-only` (or any descriptive name)
   - **Expiration:** Choose based on your needs (e.g., 90 days, 1 year, or no expiration)
   - **Scopes:** Select only `read:packages`
4. Click **"Generate token"**
5. **Copy the token immediately** - you won't see it again!

### Step 2: Add NuGet Source with Authentication

#### Option A: Using .NET CLI (stores credentials globally)

```bash
dotnet nuget add source "https://nuget.pkg.github.com/bauer-group/index.json" \
  --name "github-bauer-group" \
  --username "YOUR_GITHUB_USERNAME" \
  --password "YOUR_PERSONAL_ACCESS_TOKEN" \
  --store-password-in-clear-text
```

#### Option B: Using nuget.config (per-project)

Add to your `nuget.config` in the solution root:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github-bauer-group" value="https://nuget.pkg.github.com/bauer-group/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github-bauer-group>
      <add key="Username" value="YOUR_GITHUB_USERNAME" />
      <add key="ClearTextPassword" value="YOUR_PERSONAL_ACCESS_TOKEN" />
    </github-bauer-group>
  </packageSourceCredentials>
</configuration>
```

> **Security Note:** Never commit credentials to version control. Use environment variables or user-level nuget.config instead.

#### Option C: Using Environment Variables (CI/CD recommended)

```bash
# Set environment variables
export GITHUB_USERNAME="your-username"
export GITHUB_TOKEN="your-pat-token"

# Add source with environment variable references
dotnet nuget add source "https://nuget.pkg.github.com/bauer-group/index.json" \
  --name "github-bauer-group" \
  --username "$GITHUB_USERNAME" \
  --password "$GITHUB_TOKEN" \
  --store-password-in-clear-text
```

### Step 3: Install Package

```bash
dotnet add package BAUERGROUP.Shared.Core --source "github-bauer-group"
```

### Creating a Shared Read-Only Service Account (Team Use)

For teams, consider creating a dedicated GitHub account for package access:

1. Create a new GitHub account (e.g., `bauer-group-nuget-reader`)
2. **Do not** add this account to your organization (not required for public packages)
3. Generate a PAT with only `read:packages` scope
4. Share these credentials securely with your team (e.g., via password manager or secrets management)

This approach:
- Avoids each developer needing their own PAT
- Provides a single token to rotate if compromised
- Keeps personal accounts separate from CI/CD systems

---

## Package Dependencies

```
BAUERGROUP.Shared.Core (Base)
    │
    ├── BAUERGROUP.Shared.API
    │       │
    │       └── BAUERGROUP.Shared.Cloud
    │
    ├── BAUERGROUP.Shared.Data
    │
    ├── BAUERGROUP.Shared.Avalonia
    │
    └── BAUERGROUP.Shared.Desktop
            │
            ├── BAUERGROUP.Shared.Desktop.Browser
            │
            └── BAUERGROUP.Shared.Desktop.Reporting
```

**Note:** Dependencies are resolved automatically.

**Third-party dependencies of `BAUERGROUP.Shared.Avalonia`:** only `Avalonia` (MIT). It deliberately references no theme package — the viewer picks up the theme of the host application (for example `Avalonia.Themes.Fluent`), which your application references anyway.

---

## Quick Start

### Logging with BGLogger

```csharp
using BAUERGROUP.Shared.Core.Logging;

// No setup needed: the first use applies auto-detected defaults (file logging enabled)
BGLogger.Info("Application started");
BGLogger.Warn("Warning: {0}", warningMessage);
BGLogger.Error(exception, "An error occurred");
```

**Auto-detected defaults:**

- `ApplicationName`: Executable name (without `.exe`), also in single-file applications
- `LogDirectory`: `%ProgramData%\{ApplicationName}\Logging` on Windows; on Linux/macOS `/var/lib/{App}/Logging` if `/var/lib/{App}` exists, otherwise `~/.local/share/{App}/Logging`. See [Data, Log and Error-Report Folders](../README.md#data-log-and-error-report-folders).

**Optional overrides:**

```csharp
// Override ApplicationName and/or LogDirectory before BGLogger is first used
BGLoggerConfiguration.ApplicationName = "CustomAppName";
BGLoggerConfiguration.LogDirectory = @"C:\CustomLogs";

// Enable Sentry error tracking (optional)
BGLogger.Configuration.SentryDsn = "https://xxx@sentry.io/123";
BGLogger.Configuration.SentryEnvironment = "production";
BGLogger.Configuration.ErrorTracking = true;
```

### Live Log Viewer (Avalonia)

`BAUERGROUP.Shared.Avalonia` shows the running process's own log events through an in-process sink — no UDP socket and no log file to tail. The viewer registers the sink only while it is visible.

```csharp
using Avalonia.Controls;
using BAUERGROUP.Shared.Avalonia.Logging;

// Opens a log window owned by `owner`, or closes the one it already owns - ideal for an F12 handler
public static void ToggleLog(Window owner)
{
    LogViewerWindow.Toggle(owner, "Diagnostics");
}
```

Or embed the control in your own layout:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:logging="clr-namespace:BAUERGROUP.Shared.Avalonia.Logging;assembly=BAUERGROUP.Shared.Avalonia">
  <logging:LogViewer MaxLines="10000" ShowToolbar="True" />
</UserControl>
```

To feed a UI of your own, use `BGLogViewBuffer` (in `BAUERGROUP.Shared.Core`, no Avalonia needed) or register a callback with `BGLogger.Configuration.AddLiveSink(...)`. See the [README](../README.md#live-log-viewer-avalonia) for the full example and the rules a sink callback has to follow.

### REST API Client

```csharp
using BAUERGROUP.Shared.API.API;

using var client = new GenericAPIClient("https://api.example.com");

// GET Request
var data = await client.Get<MyResponse>("/endpoint");

// POST Request
var result = await client.Post<MyResponse, MyRequest>("/endpoint", requestData);
```

### Scheduler for Periodic Tasks

```csharp
using BAUERGROUP.Shared.Core.PeriodicExecution;

// Define custom task
public class MyJob : SchedulerObject
{
    public override void Execute()
    {
        // Executed periodically
        Console.WriteLine("Job executed!");
    }
}

// Use scheduler
var scheduler = new Scheduler();
scheduler.RegisterJob(new MyJob { Interval = TimeSpan.FromMinutes(5) });
await scheduler.StartAsync();
```

### Cloudinary Integration

```csharp
using BAUERGROUP.Shared.Cloud.CloudinaryClient;

var config = new CloudinaryImageManagerConfiguration
{
    CloudName = "your-cloud-name",
    ApiKey = "your-api-key",
    ApiSecret = "your-api-secret"
};

var manager = new CloudinaryImageManager(config);
var url = await manager.UploadImageAsync(imageBytes, "folder/image-name");
```

### Remove.bg Integration

```csharp
using BAUERGROUP.Shared.Cloud.RemoveBG;

var config = new RemoveBGConfiguration("your-api-key");
var client = new RemoveBGClient(config);

var options = new RemoveBGClientOptions(
    IsPreview: false,
    Type: RemoveBGForegroundType.Person,
    Format: RemoveBGImageFormat.PNG
);

byte[] resultImage = await client.RemoveBackgroundAsync(inputImageBytes, options);
```

### Fixer.io Currency Rates

```csharp
using BAUERGROUP.Shared.Cloud.FixerIO;

var config = new FixerIOConfiguration { ApiKey = "your-api-key" };
var client = new FixerIOClient(config);

var rates = await client.GetLatestRatesAsync("EUR");
decimal usdRate = rates.Rates["USD"];
```

---

## Target Framework Compatibility

### For .NET Framework Projects

Use `netstandard2.0` compatible packages:

- `BAUERGROUP.Shared.Core`
- `BAUERGROUP.Shared.API`
- `BAUERGROUP.Shared.Data`

### For .NET Core / .NET 5+ Projects

All packages are compatible. Recommended: `net8.0` or `net10.0`.

### For Windows Desktop (WPF/WinForms)

Use the `-windows` variants:

```xml
<TargetFramework>net10.0-windows</TargetFramework>
```

### For Cross-Platform Desktop (Avalonia)

`BAUERGROUP.Shared.Avalonia` targets plain `net10.0` and `net8.0`. It needs **no** `-windows` TFM and runs unchanged on Windows, Linux (X11/Wayland) and macOS:

```xml
<TargetFramework>net8.0</TargetFramework>
```

It is therefore also the only UI package you can reference from a project that has to build on a Linux CI runner.

---

## Versioning

The packages follow [Semantic Versioning](https://semver.org/):

- **Major** (x.0.0): Breaking changes
- **Minor** (0.x.0): New features (backward compatible)
- **Patch** (0.0.x): Bug fixes

### Using Version Ranges

```xml
<!-- Always latest 2.x version -->
<PackageReference Include="BAUERGROUP.Shared.Core" Version="2.*" />

<!-- Exact version -->
<PackageReference Include="BAUERGROUP.Shared.Core" Version="2.0.1" />

<!-- Minimum version -->
<PackageReference Include="BAUERGROUP.Shared.Core" Version="[2.0.0,)" />
```

---

## Troubleshooting

### Package Not Found

```bash
# Clear cache
dotnet nuget locals all --clear

# Specify explicit source
dotnet add package BAUERGROUP.Shared.Core --source "https://api.nuget.org/v3/index.json"
```

### Version Conflict

```bash
# Show all package versions
dotnet list package --include-transitive

# Update package
dotnet add package BAUERGROUP.Shared.Core --version 2.0.1
```

### Windows-specific Packages on Linux

The `Desktop*` packages are Windows-only. For cross-platform development:

```xml
<ItemGroup Condition="'$(TargetFramework)' == 'net10.0-windows'">
  <PackageReference Include="BAUERGROUP.Shared.Desktop" />
</ItemGroup>
```

For UI that has to run on Linux and macOS as well, use `BAUERGROUP.Shared.Avalonia` instead — it carries no Windows-only TFM and needs no condition.

---

## Further Documentation

- [BUILD.md](BUILD.md) - Build instructions
- [DEPENDENCY-LICENSES.md](DEPENDENCY-LICENSES.md) - License analysis
- [CHANGELOG.md](../CHANGELOG.md) - Change log

---

**BAUER GROUP** - *Building Better Software Together*
