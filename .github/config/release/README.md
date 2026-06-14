# Semantic Release Konfiguration für .NET Projekte

Diese Dokumentation beschreibt die Semantic Release Konfiguration für automatische Versionierung und Release-Erstellung.

## Übersicht

[Semantic Release](https://semantic-release.gitbook.io/) automatisiert den gesamten Release-Prozess basierend auf [Conventional Commits](https://www.conventionalcommits.org/).

### Versionsschema

| Commit-Prefix | Bedeutung | Version-Bump |
|---------------|-----------|--------------|
| `feat!:` oder `BREAKING CHANGE:` | Breaking Change | Major (1.0.0 → 2.0.0) |
| `feat:` | Neues Feature | Minor (1.0.0 → 1.1.0) |
| `fix:` | Bugfix | Patch (1.0.0 → 1.0.1) |
| `perf:` | Performance | Patch (1.0.0 → 1.0.1) |
| `revert:` | Revert | Patch (1.0.0 → 1.0.1) |
| `refactor:` | Refactoring | Patch (1.0.0 → 1.0.1) |
| `deps:` | Dependency-Update (Dependabot) | Patch (1.0.0 → 1.0.1) |
| `docs:`, `chore:`, `style:`, `test:`, `ci:`, `build:` | Keine Änderung | Kein Release |

> Die Release-Regeln sind explizit über `releaseRules` im `commit-analyzer`-Plugin
> definiert (siehe unten). `deps:` ist bewusst release-auslösend, damit
> Dependabot-Updates dieser geteilten Bibliothek automatisch auf NuGet
> veröffentlicht werden und Konsumenten die aktualisierten (transitiven)
> Pakete erhalten.

## Plugin-Konfiguration

### Aktuelle Konfiguration (`semantic-release.json`)

```json
{
  "branches": ["main"],
  "plugins": [
    ["@semantic-release/commit-analyzer", {
      "releaseRules": [
        { "breaking": true, "release": "major" },
        { "type": "feat", "release": "minor" },
        { "type": "fix", "release": "patch" },
        { "type": "perf", "release": "patch" },
        { "type": "revert", "release": "patch" },
        { "type": "refactor", "release": "patch" },
        { "type": "deps", "release": "patch" }
      ]
    }],
    "@semantic-release/release-notes-generator",
    ["@semantic-release/changelog", {
      "changelogFile": "CHANGELOG.md"
    }],
    ["semantic-release-dotnet", {
      "paths": ["Directory.Build.props"]
    }],
    ["@semantic-release/git", {
      "assets": ["CHANGELOG.md", "Directory.Build.props"],
      "message": "chore(release): ${nextRelease.version} [automated]\n\n${nextRelease.notes}"
    }],
    "@semantic-release/github"
  ]
}
```

### Plugin-Lifecycle

| Phase | Plugin | Beschreibung |
|-------|--------|--------------|
| **verifyConditions** | Alle | Prüft Voraussetzungen (Git, GitHub Token, etc.) |
| **analyzeCommits** | `commit-analyzer` | Analysiert Commits, bestimmt Version-Bump |
| **generateNotes** | `release-notes-generator` | Erstellt Release Notes aus Commits |
| **prepare** | `changelog` | Aktualisiert CHANGELOG.md |
| **prepare** | `semantic-release-dotnet` | Aktualisiert VersionPrefix in .NET Dateien |
| **prepare** | `git` | Committed Änderungen ins Repository |
| **publish** | `github` | Erstellt GitHub Release mit Tag |

---

## Entwickler-Anleitung: Workflow erweitern

Der `modules-semantic-release.yml` Workflow aus `bauer-group/automation-templates` unterstützt zwei Methoden zur .NET-Versionierung.

### Option 1: semantic-release-dotnet (empfohlen)

Nutzt das dedizierte .NET Plugin - sauberer und wartbarer.

**Workflow (`dotnet-release.yml`):**

```yaml
release:
  uses: bauer-group/automation-templates/.github/workflows/modules-semantic-release.yml@main
  with:
    target-branch: "main"
    extra-plugins: "semantic-release-dotnet"  # Plugin installieren
  secrets: inherit
```

**Config (`semantic-release.json`):**

```json
["semantic-release-dotnet", {
  "paths": ["Directory.Build.props"]
}]
```

**Vorteile:**
- Saubere Syntax
- Unterstützt mehrere Dateien
- Unterstützt Glob-Patterns (`src/**/*.csproj`)
- Erkennt `<Version>` und `<VersionPrefix>` automatisch

### Option 2: @semantic-release/exec (Fallback)

Verwendet sed-Befehle - funktioniert ohne extra Plugins.

**Workflow (`dotnet-release.yml`):**

```yaml
release:
  uses: bauer-group/automation-templates/.github/workflows/modules-semantic-release.yml@main
  with:
    target-branch: "main"
    # Kein extra-plugins nötig
  secrets: inherit
```

**Config (`semantic-release.json`):**

```json
["@semantic-release/exec", {
  "prepareCmd": "sed -i 's/<VersionPrefix>[^<]*</<VersionPrefix>${nextRelease.version}</g' Directory.Build.props"
}]
```

**Vorteile:**
- Immer verfügbar (Standard-Plugin)
- Keine zusätzlichen Dependencies
- Volle Kontrolle über den Befehl

---

## Konfigurationsbeispiele

### 1. Zentrale Versionierung mit Directory.Build.props

```json
["semantic-release-dotnet", {
  "paths": ["Directory.Build.props"]
}]
```

**Directory.Build.props:**

```xml
<PropertyGroup>
  <VersionPrefix>1.0.0</VersionPrefix>
</PropertyGroup>
```

### 2. Einzelne Projekte

```json
["semantic-release-dotnet", {
  "paths": ["src/MyProject/MyProject.csproj"]
}]
```

### 3. Mehrere Projekte

```json
["semantic-release-dotnet", {
  "paths": [
    "src/ProjectA/ProjectA.csproj",
    "src/ProjectB/ProjectB.csproj"
  ]
}]
```

### 4. Glob-Pattern

```json
["semantic-release-dotnet", {
  "paths": ["src/**/*.csproj"]
}]
```

### 5. Gemischter Ansatz

```json
["semantic-release-dotnet", {
  "paths": [
    "Directory.Build.props",
    "tools/StandaloneApp/StandaloneApp.csproj"
  ]
}]
```

### 6. Mit @semantic-release/exec (Alternative)

```json
["@semantic-release/exec", {
  "prepareCmd": "sed -i 's|<VersionPrefix>.*</VersionPrefix>|<VersionPrefix>${nextRelease.version}</VersionPrefix>|g' Directory.Build.props"
}]
```

## Git-Plugin Konfiguration

Das `@semantic-release/git` Plugin muss alle geänderten Dateien kennen:

```json
["@semantic-release/git", {
  "assets": [
    "CHANGELOG.md",
    "Directory.Build.props"
  ],
  "message": "chore(release): ${nextRelease.version} [automated]\n\n${nextRelease.notes}"
}]
```

**Wichtig:** Bei Glob-Patterns oder mehreren Dateien:

```json
["@semantic-release/git", {
  "assets": [
    "CHANGELOG.md",
    "Directory.Build.props",
    "src/**/*.csproj"
  ],
  "message": "chore(release): ${nextRelease.version} [automated]\n\n${nextRelease.notes}"
}]
```

## Branches-Konfiguration

### Standard (nur main)

```json
{
  "branches": ["main"]
}
```

### Mit Pre-Releases

```json
{
  "branches": [
    "main",
    { "name": "develop", "prerelease": "beta" },
    { "name": "next", "prerelease": "rc" }
  ]
}
```

Ergibt Versionen wie:
- `main` → `1.2.3`
- `develop` → `1.2.4-beta.1`
- `next` → `1.2.4-rc.1`

## GitHub Actions Workflow

### Erforderliche Secrets

| Secret | Beschreibung |
|--------|--------------|
| `GITHUB_TOKEN` | Automatisch verfügbar, für GitHub Release |
| `NUGET_API_KEY` | Für NuGet.org Publishing (optional) |

### Erforderliche Permissions

```yaml
permissions:
  contents: write      # Für Git Push und Release
  issues: write        # Für Release Notes
  pull-requests: write # Für PR Comments
```

### Workflow-Integration

```yaml
release:
  name: 📦 Create Release
  uses: bauer-group/automation-templates/.github/workflows/modules-semantic-release.yml@main
  with:
    target-branch: 'main'
    force-release: false
    extra-plugins: 'semantic-release-dotnet'  # Für .NET Versionierung
  secrets: inherit
```

## Installation

### NPM Dependencies

Die Semantic Release Pipeline benötigt folgende npm-Pakete:

```bash
npm install -D semantic-release \
  @semantic-release/changelog \
  @semantic-release/git \
  @semantic-release/github \
  semantic-release-dotnet
```

### Lokaler Test

```bash
# Dry-Run (ohne tatsächliches Release)
npx semantic-release --dry-run

# Mit Debug-Output
DEBUG=semantic-release:* npx semantic-release --dry-run
```

## Troubleshooting

### Kein Release erstellt

1. **Commits prüfen:** Mindestens ein `feat:` oder `fix:` Commit seit letztem Tag
2. **Branch prüfen:** Muss in `branches` konfiguriert sein
3. **Token prüfen:** `GITHUB_TOKEN` muss write-Berechtigung haben

### Version nicht aktualisiert

1. **Pfad prüfen:** Datei muss in `paths` konfiguriert sein
2. **Format prüfen:** `<VersionPrefix>` oder `<Version>` Tag muss existieren
3. **Git assets prüfen:** Datei muss in `@semantic-release/git` assets sein

### CHANGELOG nicht aktualisiert

1. **Plugin-Reihenfolge:** `@semantic-release/changelog` muss vor `@semantic-release/git` kommen
2. **Assets prüfen:** `CHANGELOG.md` muss in git assets sein

## Referenzen

- [Semantic Release Dokumentation](https://semantic-release.gitbook.io/)
- [semantic-release-dotnet Plugin](https://github.com/nickolaj-jepsen/semantic-release-dotnet)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [bauer-group/automation-templates](https://github.com/bauer-group/automation-templates)
