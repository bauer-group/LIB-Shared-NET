# BAUER GROUP Shared Platform - Dependency License Analysis

**Project License:** MIT
**Analysis Date:** 2026-09-16
**Target Frameworks:** .NET 10.0, .NET 8.0, .NET Standard 2.0

---

## Summary

This document lists the NuGet packages referenced in `Directory.Packages.props` and their licenses. Versions and licenses were read from each package's nuspec on nuget.org (SPDX expression, or the license file embedded in the package where no expression is declared).

### License Compatibility Overview

| License Type | Count | Compatible with MIT? | Notes |
|-------------|-------|---------------------|-------|
| MIT | 23 | ✅ Yes | Permissive, no restrictions |
| BSD 2-Clause/3-Clause | 14 | ✅ Yes | Permissive |
| Apache 2.0 | 4 | ✅ Yes | Permissive, requires attribution |
| MS-PL OR Apache 2.0 | 1 | ✅ Yes | CsvHelper (dual licensed) |
| Proprietary | 2 | ⚠️ Conditional | Stimulsoft - requires own license |

### Conclusion

✅ **No license collisions.**

All dependencies are permissive (MIT, BSD, Apache 2.0) except Stimulsoft, which requires its own commercial license. All dependencies are **dynamically linked**.

**Notes:**

1. **Stimulsoft**: Proprietary license - requires own license key (see below)
2. **Assertions library**: the tests use AwesomeAssertions (Apache 2.0). FluentAssertions 8+ is licensed under the *Xceed Community License Agreement (for Non-Commercial Use)* and must not be reintroduced without a commercial license.

---

## Detailed Package Analysis

### Core / Utilities

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| NLog | 6.1.4 | BSD 3-Clause | ✅ Compatible |
| NLog.Extensions.Logging | 6.1.4 | BSD 2-Clause | ✅ Compatible |
| NLog.WindowsEventLog | 6.1.4 | BSD 3-Clause | ✅ Compatible |
| NLog.Targets.Mail | 6.1.1 | BSD 3-Clause | ✅ Compatible |
| NLog.Targets.Network | 6.0.4 | BSD 3-Clause | ✅ Compatible |
| NLog.Targets.Trace | 6.0.3 | BSD 3-Clause | ✅ Compatible |
| NLog.Targets.WebService | 6.1.1 | BSD 3-Clause | ✅ Compatible |
| Polly | 8.7.0 | BSD 3-Clause | ✅ Compatible |
| Polly.Extensions | 8.7.0 | BSD 3-Clause | ✅ Compatible |
| Sentry | 6.10.0 | MIT | ✅ Compatible |
| Sentry.NLog | 6.10.0 | MIT | ✅ Compatible |

### Data / Persistence

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| LiteDB | 5.0.21 | MIT | ✅ Compatible |
| NMemory | 3.1.6 | MIT (license file) | ✅ Compatible |
| sqlite-net-pcl | 1.11.285 | MIT (license file) | ✅ Compatible |
| System.Runtime.Caching | 10.0.9 | MIT | ✅ Compatible |

**Transitive (via sqlite-net-pcl):** SQLitePCLRaw.core 3.0.3 and SQLitePCLRaw.provider.e_sqlite3 3.0.3 (Apache 2.0), SourceGear.sqlite3 3.53.3 (SQLite, public domain).

### File Processing

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| CsvHelper | 33.1.0 | MS-PL OR Apache 2.0 | ✅ Compatible |
| SharpZipLib | 1.4.2 | MIT | ✅ Compatible |
| HtmlAgilityPack | 1.12.4 | MIT | ✅ Compatible |

### HTTP / API

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| RestSharp | 114.0.0 | Apache 2.0 | ✅ Compatible |

### Cloud Services

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| CloudinaryDotNet | 1.29.2 | MIT | ✅ Compatible |

### Reactive Extensions

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| System.Reactive | 6.1.0 | MIT | ✅ Compatible |

### Windows Desktop (WPF/WinForms)

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| Microsoft.Xaml.Behaviors.Wpf | 1.1.142 | MIT | ✅ Compatible |
| Microsoft.Web.WebView2 | 1.0.4022.49 | BSD 3-Clause (license file) | ✅ Compatible |
| Microsoft.Win32.SystemEvents | 10.0.9 | MIT | ✅ Compatible |
| System.Configuration.ConfigurationManager | 10.0.9 | MIT | ✅ Compatible |
| System.Data.Odbc | 10.0.9 | MIT | ✅ Compatible |
| System.ServiceProcess.ServiceController | 10.0.9 | MIT | ✅ Compatible |
| System.Text.Encoding.CodePages | 10.0.9 | MIT | ✅ Compatible |
| System.Collections.Immutable | 10.0.9 | MIT | ✅ Compatible |

### CefSharp (Chromium Embedded Framework)

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| CefSharp.Common.NETCore | 152.0.60 | BSD 3-Clause (license file) | ✅ Compatible |
| CefSharp.OffScreen.NETCore | 152.0.60 | BSD 3-Clause (license file) | ✅ Compatible |
| CefSharp.Wpf.NETCore | 152.0.60 | BSD 3-Clause (license file) | ✅ Compatible |

**Note on CefSharp:**

- CefSharp and the underlying Chromium Embedded Framework (CEF) are BSD 3-Clause licensed
- The bundled Chromium binaries contain third-party components under their own licenses (listed in Chromium's `about:credits`); they are used as dynamically loaded libraries

### Reporting (Stimulsoft)

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| Stimulsoft.Dashboards.Win | 2022.1.2 | **Proprietary** | ⚠️ Requires License |
| Stimulsoft.Reports.Wpf | 2022.1.2 | **Proprietary** | ⚠️ Requires License |

**Important:**

Stimulsoft is a commercial product. To use the reporting features:

1. Acquire your own Stimulsoft license: [stimulsoft.com](https://www.stimulsoft.com/)
2. Set license key as environment variable:
   - See: `.env.example` → `STIMULSOFT_LICENSE_KEY`
3. Stimulsoft packages are pinned to version 2022.1.2

Stimulsoft 2022.1.2 brings in System.Data.SqlClient 4.7.0 (MIT) transitively, which has known security advisories (NU1902/NU1903).

### Polyfills (netstandard2.0 / net8.0)

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| PolySharp | 1.16.0 | MIT | ✅ Compatible (build-time only) |
| System.Text.Json | 10.0.2 | MIT | ✅ Compatible |
| Microsoft.Bcl.AsyncInterfaces | 10.0.2 | MIT | ✅ Compatible |

### Testing (not shipped)

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| Microsoft.NET.Test.Sdk | 18.6.0 | MIT | ✅ Compatible |
| xunit | 2.9.3 | Apache 2.0 | ✅ Compatible |
| xunit.runner.visualstudio | 3.1.5 | Apache 2.0 | ✅ Compatible |
| coverlet.collector | 10.0.1 | MIT | ✅ Compatible |
| AwesomeAssertions | 9.6.0 | Apache 2.0 | ✅ Compatible |
| Moq | 4.20.72 | BSD 3-Clause | ✅ Compatible |

### Build / Source Link

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| Microsoft.SourceLink.GitHub | 10.0.300 | MIT | ✅ Compatible (build-time only) |

---

## License Categories Explained

### MIT License

The MIT License is one of the most permissive licenses. It allows:

- Commercial use
- Modification
- Distribution
- Private use
- Sublicensing

**Requirements:** Include copyright notice and license text

### BSD Licenses (2-Clause, 3-Clause)

BSD licenses are permissive and similar to MIT:

- 2-Clause (Simplified): Very similar to MIT
- 3-Clause (New BSD): Adds non-endorsement clause

**Requirements:** Include copyright notice and license text

### Apache 2.0 License

Apache 2.0 is permissive with additional patent grants:

- Provides explicit patent grant
- Requires attribution
- Changes must be documented

**Requirements:** Include NOTICE file if present, state changes

### Proprietary (Stimulsoft)

Requires:

- Valid commercial license
- Compliance with Stimulsoft EULA
- Not redistributable without proper licensing

---

## Generated Information

Versions and licenses were verified against nuget.org package metadata on 2026-09-16.

For questions regarding licensing, contact the BAUER GROUP Development Team.
