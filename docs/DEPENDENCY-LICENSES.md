# BAUER GROUP Shared Platform - Dependency License Analysis

**Project License:** MIT
**Analysis Date:** 2026-01-16
**Target Framework:** .NET 10.0

---

## Summary

This document provides a comprehensive analysis of all NuGet package dependencies used in the BAUER GROUP Shared Platform and their respective licenses.

### License Compatibility Overview

| License Type | Count | Compatible with MIT? | Notes |
|-------------|-------|---------------------|-------|
| MIT | 15+ | ✅ Yes | Permissive, no restrictions |
| Apache 2.0 | 10+ | ✅ Yes | Permissive, requires attribution |
| BSD 2-Clause/3-Clause | 3 | ✅ Yes | Permissive |
| LGPL 2.1 | 1 | ⚠️ Conditional | CefSharp - OK for dynamic linking |
| Proprietary | 1 | ⚠️ Conditional | Stimulsoft - Requires own license |

### Conclusion

✅ **No critical license collisions.**

Since all dependencies are **dynamically linked** (not statically), all licenses are unproblematic for use under MIT.

**Notes:**

1. **CefSharp (Chromium)**: BSD 3-Clause - Dynamically linked, no restrictions
2. **Stimulsoft**: Proprietary license - Requires own license key (see below)

---

## Detailed Package Analysis

### Core / Utilities

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| NLog | 5.3.4 | BSD 3-Clause | ✅ Compatible |
| NLog.Extensions.Logging | 5.3.14 | BSD 3-Clause | ✅ Compatible |
| NLog.WindowsEventLog | 5.3.4 | BSD 3-Clause | ✅ Compatible |
| Polly | 8.5.0 | BSD 3-Clause | ✅ Compatible |
| Polly.Extensions | 8.5.0 | BSD 3-Clause | ✅ Compatible |
| Sentry | 5.0.0 | MIT | ✅ Compatible |
| Sentry.NLog | 5.0.0 | MIT | ✅ Compatible |

### Data / Persistence

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| LiteDB | 5.0.21 | MIT | ✅ Compatible |
| NMemory | 3.1.0 | MIT | ✅ Compatible |
| sqlite-net-pcl | 1.9.172 | MIT | ✅ Compatible |
| SQLitePCLRaw.bundle_e_sqlcipher | 2.1.10 | Apache 2.0 | ✅ Compatible |
| System.Runtime.Caching | 9.0.0 | MIT | ✅ Compatible |

### File Processing

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| CsvHelper | 33.0.1 | MS-PL / Apache 2.0 | ✅ Compatible |
| SharpZipLib | 1.4.2 | MIT | ✅ Compatible |
| HtmlAgilityPack | 1.11.71 | MIT | ✅ Compatible |

### HTTP / API

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| RestSharp | 112.1.0 | Apache 2.0 | ✅ Compatible |

### Cloud Services

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| CloudinaryDotNet | 1.27.1 | MIT | ✅ Compatible |

### Reactive Extensions

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| System.Reactive | 6.0.1 | MIT | ✅ Compatible |
| ReactiveUI | 20.1.63 | MIT | ✅ Compatible |
| Splat | 15.2.22 | MIT | ✅ Compatible |
| Splat.NLog | 15.2.22 | MIT | ✅ Compatible |

### Windows Desktop (WPF/WinForms)

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| Microsoft.Xaml.Behaviors.Wpf | 1.1.135 | MIT | ✅ Compatible |
| Microsoft.Web.WebView2 | 1.0.2903.40 | BSD 3-Clause | ✅ Compatible |
| Microsoft.Win32.SystemEvents | 9.0.0 | MIT | ✅ Compatible |
| System.Configuration.ConfigurationManager | 9.0.0 | MIT | ✅ Compatible |
| System.Data.Odbc | 9.0.0 | MIT | ✅ Compatible |
| System.Data.SqlClient | 4.9.0 | MIT | ✅ Compatible |
| System.ServiceProcess.ServiceController | 9.0.0 | MIT | ✅ Compatible |
| System.Text.Encoding.CodePages | 9.0.0 | MIT | ✅ Compatible |
| System.Collections.Immutable | 9.0.0 | MIT | ✅ Compatible |

### CefSharp (Chromium Embedded Framework)

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| CefSharp.Common.NETCore | 131.3.50 | BSD 3-Clause | ✅ Compatible |
| CefSharp.OffScreen.NETCore | 131.3.50 | BSD 3-Clause | ✅ Compatible |
| CefSharp.Wpf.NETCore | 131.3.50 | BSD 3-Clause | ✅ Compatible |

**Note on CefSharp:**
- CefSharp itself is BSD 3-Clause licensed
- The underlying Chromium Embedded Framework (CEF) is BSD licensed
- No GPL/LGPL infection risk when used as a dynamic library

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

### Testing

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| Microsoft.NET.Test.Sdk | 17.12.0 | MIT | ✅ Compatible |
| xunit | 2.9.2 | Apache 2.0 | ✅ Compatible |
| xunit.runner.visualstudio | 2.8.2 | Apache 2.0 | ✅ Compatible |
| coverlet.collector | 6.0.2 | MIT | ✅ Compatible |
| FluentAssertions | 7.0.0 | Apache 2.0 | ✅ Compatible |
| Moq | 4.20.72 | BSD 3-Clause | ✅ Compatible |

### Build / Source Link

| Package | Version | License | Compatibility |
|---------|---------|---------|---------------|
| Microsoft.SourceLink.GitHub | 8.0.0 | Apache 2.0 | ✅ Compatible |

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

### LGPL (Lesser General Public License)
LGPL allows:
- Dynamic linking without license infection
- Static linking requires LGPL compliance

**Note:** All CefSharp components are dynamically linked, so LGPL requirements are satisfied.

### Proprietary (Stimulsoft)
Requires:
- Valid commercial license
- Compliance with Stimulsoft EULA
- Not redistributable without proper licensing

---

## Generated Information

This analysis was generated for BAUERGROUP.Shared as part of the .NET 10 migration.

For questions regarding licensing, contact the BAUER GROUP Development Team.
