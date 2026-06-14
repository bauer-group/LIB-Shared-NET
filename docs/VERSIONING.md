# Versioning Guide

This library follows [Semantic Versioning 2.0.0](https://semver.org/) (SemVer).

## Version Format

```
MAJOR.MINOR.PATCH
  │     │     └── Bug fixes, no API changes
  │     └──────── New features, backward compatible
  └────────────── Breaking changes
```

**Current Version:** 2.x

## For Consuming Projects

### Using Version Ranges (Recommended)

To automatically receive compatible updates within a major version line, use version ranges in your project files:

```xml
<!-- Floating version: automatically uses latest 2.x -->
<PackageReference Include="BAUERGROUP.Shared.Core" Version="2.*" />

<!-- Explicit range: 2.0.0 up to (but not including) 3.0.0 -->
<PackageReference Include="BAUERGROUP.Shared.Core" Version="[2.0.0, 3.0.0)" />

<!-- Minimum version: 2.1.0 or higher (including 3.x, 4.x, etc.) -->
<PackageReference Include="BAUERGROUP.Shared.Core" Version="2.1.0" />
```

### Version Range Syntax

| Notation | Description | Example |
|----------|-------------|---------|
| `2.*` | Any 2.x version | 2.0.0, 2.1.9, 2.99.0 |
| `[2.0.0, 3.0.0)` | >= 2.0.0 and < 3.0.0 | 2.0.0 to 2.x.x |
| `[2.1.0,]` | >= 2.1.0 (no upper bound) | 2.1.0 and above |
| `2.1.0` | >= 2.1.0 (NuGet default) | 2.1.0 and above |

### Recommended Configuration

For production applications that need stability within a major version:

```xml
<ItemGroup>
  <PackageReference Include="BAUERGROUP.Shared.Core" Version="[2.0.0, 3.0.0)" />
  <PackageReference Include="BAUERGROUP.Shared.API" Version="[2.0.0, 3.0.0)" />
  <PackageReference Include="BAUERGROUP.Shared.Data" Version="[2.0.0, 3.0.0)" />
  <PackageReference Include="BAUERGROUP.Shared.Desktop" Version="[2.0.0, 3.0.0)" />
</ItemGroup>
```

## Our Compatibility Promise

Within the **2.x version line**, we guarantee:

- No breaking changes to public APIs
- No removal of public types, methods, or properties
- No changes to method signatures
- Backward compatible behavior

**Breaking changes** (if necessary) will only occur in a new major version (e.g., 3.0.0).

## Updating Packages

To update to the latest compatible version:

```bash
# Update all packages to latest compatible versions
dotnet restore

# Or force update a specific package
dotnet add package BAUERGROUP.Shared.Core
```

## Release Notes

See [GitHub Releases](https://github.com/bauer-group/LIB-Shared-NET/releases) for detailed changelogs.
