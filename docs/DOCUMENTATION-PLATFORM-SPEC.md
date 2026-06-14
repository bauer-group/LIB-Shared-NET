# AI-Powered Documentation Platform

## Technische Spezifikation v1.0

**Projekt:** BAUERGROUP.Shared
**Erstellt:** 2026-01-18
**Status:** Konzept zur Umsetzung

---

## 1. Executive Summary

Dieses Dokument beschreibt die Architektur und Implementierung einer vollautomatischen, KI-gestützten Dokumentationsplattform für die BAUERGROUP.Shared Bibliotheken. Die Lösung generiert professionelle API-Dokumentation mit Beispielen, Diagrammen und einem interaktiven AI-Chat.

### Kernziele

- **Vollautomatisch**: Dokumentation wird bei jedem Release automatisch generiert
- **KI-gestützt**: Claude, GPT-4, oder andere LLMs generieren hochwertige Beschreibungen
- **Provider-agnostisch**: Flexible Wahl zwischen OpenAI, Anthropic, Azure OpenAI, lokalen Modellen
- **Qualitätsgesichert**: Automatische Validierung aller Code-Beispiele
- **Modern & Durchsuchbar**: Statische Site mit Volltext-Suche und AI-Chat

---

## 2. Architektur

### 2.1 Systemübersicht

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         DOCUMENTATION PIPELINE                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌────────────┐   ┌────────────┐   ┌────────────┐   ┌────────────┐      │
│  │   Roslyn   │──▶│  AI Doc    │──▶│  Quality   │──▶│   DocFX    │      │
│  │  Analyzer  │   │ Generator  │   │    Gate    │   │  Builder   │      │
│  └────────────┘   └────────────┘   └────────────┘   └────────────┘      │
│        │                │                │                │              │
│        │          ┌─────┴─────┐          │                │              │
│        │          │           │          │                │              │
│        │     ┌────┴───┐ ┌────┴───┐      │                │              │
│        │     │ OpenAI │ │ Claude │      │                │              │
│        │     │   API  │ │   API  │      │                │              │
│        │     └────────┘ └────────┘      │                │              │
│        │                                 │                │              │
│        ▼                                 ▼                ▼              │
│  ┌────────────┐                   ┌────────────┐   ┌────────────┐       │
│  │   Code     │                   │  Compiled  │   │   Static   │       │
│  │  Metadata  │                   │  Examples  │   │    Site    │       │
│  └────────────┘                   └────────────┘   └────────────┘       │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Komponenten

| Komponente | Verantwortlichkeit | Technologie |
|------------|-------------------|-------------|
| **Code Analyzer** | Extrahiert Metadaten aus C#-Code | Roslyn Compiler API |
| **AI Provider** | Abstrahiert LLM-Zugriff | Custom Interface |
| **Doc Generator** | Orchestriert Dokumentationserstellung | .NET 10 Console App |
| **Quality Gate** | Validiert generierte Dokumentation | Roslyn Scripting |
| **Site Builder** | Erstellt statische Website | DocFX 2.x |
| **Search Index** | Ermöglicht Volltext-Suche | Pagefind |
| **AI Chat** | Beantwortet Fragen zur API | RAG + LLM |

---

## 3. AI Provider Abstraktion

### 3.1 Interface Definition

```csharp
namespace BAUERGROUP.DocGen.Providers;

/// <summary>
/// Abstraktion für verschiedene AI-Provider (OpenAI, Anthropic, Azure, etc.)
/// </summary>
public interface IAIProvider
{
    /// <summary>
    /// Provider-Name für Logging und Konfiguration
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Generiert eine Completion basierend auf dem Request
    /// </summary>
    Task<AIResponse> GenerateAsync(AIRequest request, CancellationToken ct = default);

    /// <summary>
    /// Prüft ob der Provider verfügbar und konfiguriert ist
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken ct = default);

    /// <summary>
    /// Geschätzte Kosten für einen Request (in USD)
    /// </summary>
    decimal EstimateCost(AIRequest request);
}

/// <summary>
/// Request an den AI-Provider
/// </summary>
public record AIRequest
{
    public required string SystemPrompt { get; init; }
    public required string UserPrompt { get; init; }
    public string? Model { get; init; }
    public int MaxTokens { get; init; } = 4096;
    public double Temperature { get; init; } = 0.3;
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Response vom AI-Provider
/// </summary>
public record AIResponse
{
    public required string Content { get; init; }
    public required string Model { get; init; }
    public required int InputTokens { get; init; }
    public required int OutputTokens { get; init; }
    public required TimeSpan Duration { get; init; }
    public decimal? Cost { get; init; }
}
```

### 3.2 Anthropic Provider Implementation

```csharp
namespace BAUERGROUP.DocGen.Providers;

public class AnthropicProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly AnthropicOptions _options;

    public string Name => "Anthropic";

    public AnthropicProvider(IOptions<AnthropicOptions> options)
    {
        _options = options.Value;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.anthropic.com/"),
            DefaultRequestHeaders =
            {
                { "x-api-key", _options.ApiKey },
                { "anthropic-version", "2023-06-01" }
            }
        };
    }

    public async Task<AIResponse> GenerateAsync(AIRequest request, CancellationToken ct)
    {
        var model = request.Model ?? _options.DefaultModel ?? "claude-sonnet-4-20250514";
        var stopwatch = Stopwatch.StartNew();

        var payload = new
        {
            model = model,
            max_tokens = request.MaxTokens,
            temperature = request.Temperature,
            system = request.SystemPrompt,
            messages = new[]
            {
                new { role = "user", content = request.UserPrompt }
            }
        };

        var response = await _httpClient.PostAsJsonAsync("v1/messages", payload, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AnthropicResponse>(ct);
        stopwatch.Stop();

        return new AIResponse
        {
            Content = result!.Content[0].Text,
            Model = model,
            InputTokens = result.Usage.InputTokens,
            OutputTokens = result.Usage.OutputTokens,
            Duration = stopwatch.Elapsed,
            Cost = CalculateCost(model, result.Usage)
        };
    }

    private decimal CalculateCost(string model, AnthropicUsage usage)
    {
        // Preise Stand 2026 (anpassen bei Änderungen)
        var (inputPrice, outputPrice) = model switch
        {
            var m when m.Contains("opus") => (0.015m, 0.075m),
            var m when m.Contains("sonnet") => (0.003m, 0.015m),
            var m when m.Contains("haiku") => (0.00025m, 0.00125m),
            _ => (0.003m, 0.015m)
        };

        return (usage.InputTokens / 1000m * inputPrice) +
               (usage.OutputTokens / 1000m * outputPrice);
    }
}

public record AnthropicOptions
{
    public required string ApiKey { get; init; }
    public string? DefaultModel { get; init; }
}
```

### 3.3 OpenAI Provider Implementation

```csharp
namespace BAUERGROUP.DocGen.Providers;

public class OpenAIProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly OpenAIOptions _options;

    public string Name => "OpenAI";

    public OpenAIProvider(IOptions<OpenAIOptions> options)
    {
        _options = options.Value;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_options.BaseUrl ?? "https://api.openai.com/"),
            DefaultRequestHeaders =
            {
                { "Authorization", $"Bearer {_options.ApiKey}" }
            }
        };
    }

    public async Task<AIResponse> GenerateAsync(AIRequest request, CancellationToken ct)
    {
        var model = request.Model ?? _options.DefaultModel ?? "gpt-4o";
        var stopwatch = Stopwatch.StartNew();

        var payload = new
        {
            model = model,
            max_tokens = request.MaxTokens,
            temperature = request.Temperature,
            messages = new object[]
            {
                new { role = "system", content = request.SystemPrompt },
                new { role = "user", content = request.UserPrompt }
            }
        };

        var response = await _httpClient.PostAsJsonAsync("v1/chat/completions", payload, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>(ct);
        stopwatch.Stop();

        return new AIResponse
        {
            Content = result!.Choices[0].Message.Content,
            Model = model,
            InputTokens = result.Usage.PromptTokens,
            OutputTokens = result.Usage.CompletionTokens,
            Duration = stopwatch.Elapsed,
            Cost = CalculateCost(model, result.Usage)
        };
    }

    private decimal CalculateCost(string model, OpenAIUsage usage)
    {
        var (inputPrice, outputPrice) = model switch
        {
            "gpt-4o" => (0.005m, 0.015m),
            "gpt-4o-mini" => (0.00015m, 0.0006m),
            "gpt-4-turbo" => (0.01m, 0.03m),
            "o1" => (0.015m, 0.06m),
            "o1-mini" => (0.003m, 0.012m),
            _ => (0.005m, 0.015m)
        };

        return (usage.PromptTokens / 1000m * inputPrice) +
               (usage.CompletionTokens / 1000m * outputPrice);
    }
}

public record OpenAIOptions
{
    public required string ApiKey { get; init; }
    public string? BaseUrl { get; init; }  // Für Azure OpenAI oder lokale Endpunkte
    public string? DefaultModel { get; init; }
}
```

### 3.4 Provider Factory

```csharp
namespace BAUERGROUP.DocGen.Providers;

public interface IAIProviderFactory
{
    IAIProvider GetProvider(string? preferredProvider = null);
    IEnumerable<IAIProvider> GetAllProviders();
}

public class AIProviderFactory : IAIProviderFactory
{
    private readonly Dictionary<string, IAIProvider> _providers;
    private readonly DocGenOptions _options;

    public AIProviderFactory(
        IEnumerable<IAIProvider> providers,
        IOptions<DocGenOptions> options)
    {
        _providers = providers.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
        _options = options.Value;
    }

    public IAIProvider GetProvider(string? preferredProvider = null)
    {
        var name = preferredProvider ?? _options.DefaultProvider ?? "Anthropic";

        if (_providers.TryGetValue(name, out var provider))
            return provider;

        throw new InvalidOperationException(
            $"AI Provider '{name}' nicht gefunden. Verfügbar: {string.Join(", ", _providers.Keys)}");
    }

    public IEnumerable<IAIProvider> GetAllProviders() => _providers.Values;
}
```

---

## 4. Code Analyzer

### 4.1 Metadaten-Modell

```csharp
namespace BAUERGROUP.DocGen.Analysis;

/// <summary>
/// Vollständige Metadaten einer zu dokumentierenden Klasse
/// </summary>
public record ClassDocContext
{
    // Identifikation
    public required string Name { get; init; }
    public required string FullName { get; init; }
    public required string Namespace { get; init; }
    public required string AssemblyName { get; init; }

    // Typ-Information
    public required TypeKind Kind { get; init; }  // Class, Interface, Struct, Enum, Record
    public required AccessModifier Access { get; init; }
    public bool IsStatic { get; init; }
    public bool IsAbstract { get; init; }
    public bool IsSealed { get; init; }
    public bool IsPartial { get; init; }

    // Vererbung & Implementierung
    public string? BaseType { get; init; }
    public IReadOnlyList<string> Interfaces { get; init; } = [];
    public IReadOnlyList<string> GenericParameters { get; init; } = [];

    // Members
    public IReadOnlyList<MethodDocContext> Methods { get; init; } = [];
    public IReadOnlyList<PropertyDocContext> Properties { get; init; } = [];
    public IReadOnlyList<EventDocContext> Events { get; init; } = [];
    public IReadOnlyList<FieldDocContext> Fields { get; init; } = [];
    public IReadOnlyList<ConstructorDocContext> Constructors { get; init; } = [];

    // Analyse-Ergebnisse
    public IReadOnlyList<string> DetectedPatterns { get; init; } = [];  // Singleton, Factory, etc.
    public ThreadSafetyLevel ThreadSafety { get; init; }
    public bool ImplementsIDisposable { get; init; }
    public int CyclomaticComplexity { get; init; }

    // Abhängigkeiten
    public IReadOnlyList<string> Dependencies { get; init; } = [];
    public IReadOnlyList<string> UsedBy { get; init; } = [];

    // Quellcode
    public required string SourceCode { get; init; }
    public string? ExistingXmlDoc { get; init; }
    public IReadOnlyList<string> TestUsageExamples { get; init; } = [];

    // Datei-Info
    public required string FilePath { get; init; }
    public int StartLine { get; init; }
    public int EndLine { get; init; }
}

public record MethodDocContext
{
    public required string Name { get; init; }
    public required string ReturnType { get; init; }
    public required AccessModifier Access { get; init; }
    public IReadOnlyList<ParameterDocContext> Parameters { get; init; } = [];
    public IReadOnlyList<string> ThrownExceptions { get; init; } = [];
    public bool IsAsync { get; init; }
    public bool IsStatic { get; init; }
    public bool IsVirtual { get; init; }
    public bool IsOverride { get; init; }
    public string? ExistingXmlDoc { get; init; }
    public required string SourceCode { get; init; }
}

public record PropertyDocContext
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required AccessModifier Access { get; init; }
    public bool HasGetter { get; init; }
    public bool HasSetter { get; init; }
    public bool IsRequired { get; init; }
    public string? DefaultValue { get; init; }
    public string? ExistingXmlDoc { get; init; }
}

public record ParameterDocContext
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public bool IsOptional { get; init; }
    public string? DefaultValue { get; init; }
    public bool IsParams { get; init; }
    public bool IsOut { get; init; }
    public bool IsRef { get; init; }
}

public enum TypeKind { Class, Interface, Struct, Enum, Record, Delegate }
public enum AccessModifier { Public, Internal, Protected, Private, ProtectedInternal }
public enum ThreadSafetyLevel { Unknown, NotThreadSafe, ThreadSafe, Immutable }
```

### 4.2 Roslyn Analyzer Implementation

```csharp
namespace BAUERGROUP.DocGen.Analysis;

public class RoslynCodeAnalyzer
{
    private readonly ILogger<RoslynCodeAnalyzer> _logger;

    public async Task<IReadOnlyList<ClassDocContext>> AnalyzeProjectAsync(
        string projectPath,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Analysiere Projekt: {Project}", projectPath);

        // MSBuild Workspace laden
        using var workspace = MSBuildWorkspace.Create();
        var project = await workspace.OpenProjectAsync(projectPath, cancellationToken: ct);
        var compilation = await project.GetCompilationAsync(ct);

        if (compilation is null)
            throw new InvalidOperationException($"Compilation für {projectPath} fehlgeschlagen");

        var results = new List<ClassDocContext>();

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var root = await syntaxTree.GetRootAsync(ct);

            // Alle Typ-Deklarationen finden
            var typeDeclarations = root.DescendantNodes()
                .OfType<TypeDeclarationSyntax>()
                .Where(IsPublicApi);

            foreach (var typeDecl in typeDeclarations)
            {
                var symbol = semanticModel.GetDeclaredSymbol(typeDecl);
                if (symbol is INamedTypeSymbol namedType)
                {
                    var context = AnalyzeType(namedType, typeDecl, semanticModel, compilation);
                    results.Add(context);
                }
            }
        }

        _logger.LogInformation("Gefunden: {Count} öffentliche Typen", results.Count);
        return results;
    }

    private ClassDocContext AnalyzeType(
        INamedTypeSymbol symbol,
        TypeDeclarationSyntax syntax,
        SemanticModel model,
        Compilation compilation)
    {
        return new ClassDocContext
        {
            Name = symbol.Name,
            FullName = symbol.ToDisplayString(),
            Namespace = symbol.ContainingNamespace.ToDisplayString(),
            AssemblyName = symbol.ContainingAssembly.Name,

            Kind = MapTypeKind(symbol.TypeKind),
            Access = MapAccessibility(symbol.DeclaredAccessibility),
            IsStatic = symbol.IsStatic,
            IsAbstract = symbol.IsAbstract,
            IsSealed = symbol.IsSealed,

            BaseType = symbol.BaseType?.ToDisplayString(),
            Interfaces = symbol.Interfaces.Select(i => i.ToDisplayString()).ToList(),

            Methods = AnalyzeMethods(symbol),
            Properties = AnalyzeProperties(symbol),
            Constructors = AnalyzeConstructors(symbol),

            DetectedPatterns = DetectDesignPatterns(symbol, syntax),
            ThreadSafety = AnalyzeThreadSafety(symbol),
            ImplementsIDisposable = symbol.AllInterfaces.Any(i =>
                i.ToDisplayString() == "System.IDisposable"),

            Dependencies = GetDependencies(symbol, compilation),

            SourceCode = syntax.ToFullString(),
            ExistingXmlDoc = symbol.GetDocumentationCommentXml(),
            TestUsageExamples = FindTestUsages(symbol, compilation),

            FilePath = syntax.SyntaxTree.FilePath,
            StartLine = syntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
            EndLine = syntax.GetLocation().GetLineSpan().EndLinePosition.Line + 1
        };
    }

    private IReadOnlyList<string> DetectDesignPatterns(
        INamedTypeSymbol symbol,
        TypeDeclarationSyntax syntax)
    {
        var patterns = new List<string>();

        // Singleton Pattern
        if (HasPrivateConstructor(symbol) && HasStaticInstanceProperty(symbol))
            patterns.Add("Singleton");

        // Factory Pattern
        if (symbol.Name.EndsWith("Factory") || HasFactoryMethods(symbol))
            patterns.Add("Factory");

        // Builder Pattern
        if (HasFluentMethods(symbol))
            patterns.Add("Builder/Fluent");

        // Repository Pattern
        if (symbol.Name.EndsWith("Repository") || ImplementsRepository(symbol))
            patterns.Add("Repository");

        // Observer Pattern (Events)
        if (symbol.GetMembers().OfType<IEventSymbol>().Any())
            patterns.Add("Observer");

        return patterns;
    }

    private ThreadSafetyLevel AnalyzeThreadSafety(INamedTypeSymbol symbol)
    {
        // Prüfe auf Thread-Safety Attribute
        if (HasThreadSafeAttribute(symbol))
            return ThreadSafetyLevel.ThreadSafe;

        // Prüfe auf Immutability (readonly struct, record)
        if (symbol.IsReadOnly || symbol.IsRecord)
            return ThreadSafetyLevel.Immutable;

        // Prüfe auf Lock-Statements oder Concurrent Collections
        if (UsesLockingMechanisms(symbol))
            return ThreadSafetyLevel.ThreadSafe;

        return ThreadSafetyLevel.Unknown;
    }
}
```

---

## 5. Dokumentations-Generator

### 5.1 Prompt Templates

```csharp
namespace BAUERGROUP.DocGen.Generation;

public static class PromptTemplates
{
    public const string SystemPrompt = """
        Du bist ein erfahrener .NET-Dokumentationsexperte mit 15+ Jahren Erfahrung.

        Deine Aufgabe ist es, professionelle XML-Dokumentation für C#-Code zu erstellen.

        ## Anforderungen an die Dokumentation:

        1. **<summary>**: 2-3 prägnante Sätze, die den Zweck der Klasse/Methode erklären
           - Beginne NICHT mit "Diese Klasse..." oder "Diese Methode..."
           - Verwende aktive Sprache
           - Erkläre das "Was" und "Warum"

        2. **<remarks>**: Zusätzliche Details zur Implementierung
           - Design-Entscheidungen
           - Thread-Safety Hinweise
           - Performance-Charakteristiken
           - Abhängigkeiten zu anderen Komponenten

        3. **<example>**: Mindestens ein vollständiges, kompilierbares Code-Beispiel
           - Realistischer Anwendungsfall
           - Fehlerbehandlung inkludieren
           - Kommentare im Code für Klarheit

        4. **<param>**: Für jeden Parameter
           - Zweck des Parameters
           - Gültige Wertebereiche
           - Standardwerte wenn optional

        5. **<returns>**: Beschreibung des Rückgabewerts
           - Was wird zurückgegeben?
           - Mögliche Null-Werte erwähnen

        6. **<exception>**: Für jede geworfene Exception
           - Unter welchen Bedingungen wird sie geworfen?

        ## Formatierung:
        - Verwende <para> für Absätze in längeren Beschreibungen
        - Verwende <see cref="..."/> für Verweise auf andere Typen
        - Verwende <c>...</c> für Inline-Code
        - Verwende <list type="bullet"> für Aufzählungen

        ## Sprache:
        - Dokumentation auf Englisch (Projektstandard)
        - Technisch präzise aber verständlich
        - Konsistent mit existierender Dokumentation im Projekt
        """;

    public const string ClassDocPrompt = """
        Erstelle vollständige XML-Dokumentation für folgende C#-Klasse:

        ## Klassen-Information:
        - **Name**: {ClassName}
        - **Namespace**: {Namespace}
        - **Assembly**: {Assembly}
        - **Basis-Klasse**: {BaseType}
        - **Interfaces**: {Interfaces}
        - **Erkannte Patterns**: {DetectedPatterns}
        - **Thread-Safety**: {ThreadSafety}

        ## Quellcode:
        ```csharp
        {SourceCode}
        ```

        ## Existierende Dokumentation (falls vorhanden):
        {ExistingXmlDoc}

        ## Beispiel-Verwendung aus Tests:
        ```csharp
        {TestUsages}
        ```

        ---

        Generiere die XML-Dokumentation im folgenden Format:

        ```xml
        <summary>
        ...
        </summary>
        <remarks>
        ...
        </remarks>
        <example>
        <code>
        ...
        </code>
        </example>
        ```

        Dokumentiere auch alle öffentlichen Methoden und Properties.
        """;

    public const string DiagramPrompt = """
        Basierend auf der folgenden Klasse, erstelle ein Mermaid-Diagramm:

        ## Klasse:
        {ClassName}

        ## Beziehungen:
        - Basis-Klasse: {BaseType}
        - Interfaces: {Interfaces}
        - Abhängigkeiten: {Dependencies}
        - Wird verwendet von: {UsedBy}

        ## Quellcode:
        ```csharp
        {SourceCode}
        ```

        Erstelle ein passendes Diagramm:
        - Klassendiagramm für komplexe Hierarchien
        - Sequenzdiagramm für wichtige Workflows
        - Flowchart für komplexe Logik

        Antwort im Format:
        ```mermaid
        ...
        ```
        """;
}
```

### 5.2 Documentation Generator Service

```csharp
namespace BAUERGROUP.DocGen.Generation;

public class DocumentationGenerator
{
    private readonly IAIProviderFactory _providerFactory;
    private readonly ILogger<DocumentationGenerator> _logger;

    public async Task<GeneratedDocumentation> GenerateAsync(
        ClassDocContext context,
        GenerationOptions options,
        CancellationToken ct = default)
    {
        var provider = _providerFactory.GetProvider(options.PreferredProvider);
        _logger.LogInformation(
            "Generiere Dokumentation für {Class} mit {Provider}",
            context.FullName, provider.Name);

        // Pass 1: Hauptdokumentation generieren
        var docRequest = new AIRequest
        {
            SystemPrompt = PromptTemplates.SystemPrompt,
            UserPrompt = BuildClassPrompt(context),
            Model = options.Model,
            Temperature = 0.3,
            MaxTokens = 4096
        };

        var docResponse = await provider.GenerateAsync(docRequest, ct);
        var xmlDoc = ParseXmlDocumentation(docResponse.Content);

        // Pass 2: Diagramme generieren (optional)
        string? diagram = null;
        if (options.GenerateDiagrams && ShouldHaveDiagram(context))
        {
            var diagramRequest = new AIRequest
            {
                SystemPrompt = "Du erstellst technische Mermaid-Diagramme.",
                UserPrompt = BuildDiagramPrompt(context),
                Model = options.DiagramModel ?? "claude-haiku-3-5-20241022",
                Temperature = 0.2,
                MaxTokens = 2048
            };

            var diagramResponse = await provider.GenerateAsync(diagramRequest, ct);
            diagram = ExtractMermaidDiagram(diagramResponse.Content);
        }

        return new GeneratedDocumentation
        {
            TypeName = context.FullName,
            XmlDocumentation = xmlDoc,
            MermaidDiagram = diagram,
            GeneratedAt = DateTimeOffset.UtcNow,
            Provider = provider.Name,
            Model = docResponse.Model,
            TokensUsed = docResponse.InputTokens + docResponse.OutputTokens,
            Cost = docResponse.Cost
        };
    }

    private string BuildClassPrompt(ClassDocContext context)
    {
        return PromptTemplates.ClassDocPrompt
            .Replace("{ClassName}", context.Name)
            .Replace("{Namespace}", context.Namespace)
            .Replace("{Assembly}", context.AssemblyName)
            .Replace("{BaseType}", context.BaseType ?? "object")
            .Replace("{Interfaces}", string.Join(", ", context.Interfaces))
            .Replace("{DetectedPatterns}", string.Join(", ", context.DetectedPatterns))
            .Replace("{ThreadSafety}", context.ThreadSafety.ToString())
            .Replace("{SourceCode}", context.SourceCode)
            .Replace("{ExistingXmlDoc}", context.ExistingXmlDoc ?? "(keine)")
            .Replace("{TestUsages}", string.Join("\n\n", context.TestUsageExamples));
    }
}

public record GenerationOptions
{
    public string? PreferredProvider { get; init; }
    public string? Model { get; init; }
    public bool GenerateDiagrams { get; init; } = true;
    public string? DiagramModel { get; init; }
    public int MaxParallelRequests { get; init; } = 5;
    public int MinQualityScore { get; init; } = 80;
}

public record GeneratedDocumentation
{
    public required string TypeName { get; init; }
    public required XmlDocumentation XmlDocumentation { get; init; }
    public string? MermaidDiagram { get; init; }
    public required DateTimeOffset GeneratedAt { get; init; }
    public required string Provider { get; init; }
    public required string Model { get; init; }
    public required int TokensUsed { get; init; }
    public decimal? Cost { get; init; }
}
```

---

## 6. Quality Gate

### 6.1 Validierung

```csharp
namespace BAUERGROUP.DocGen.Validation;

public class DocumentationQualityGate
{
    private readonly ILogger<DocumentationQualityGate> _logger;

    public async Task<QualityReport> ValidateAsync(
        GeneratedDocumentation doc,
        ClassDocContext context,
        CancellationToken ct = default)
    {
        var report = new QualityReport { TypeName = doc.TypeName };

        // 1. Strukturelle Validierung
        report.HasSummary = !string.IsNullOrWhiteSpace(doc.XmlDocumentation.Summary);
        report.HasRemarks = !string.IsNullOrWhiteSpace(doc.XmlDocumentation.Remarks);
        report.HasExamples = doc.XmlDocumentation.Examples.Any();

        // 2. Code-Beispiele kompilieren
        foreach (var example in doc.XmlDocumentation.Examples)
        {
            var compileResult = await CompileExampleAsync(example, context, ct);
            report.ExampleResults.Add(compileResult);
        }

        // 3. Vollständigkeit prüfen
        report.MethodsCoverage = CalculateMethodCoverage(doc, context);
        report.PropertiesCoverage = CalculatePropertyCoverage(doc, context);
        report.ParametersCoverage = CalculateParameterCoverage(doc, context);

        // 4. Qualitäts-Score berechnen
        report.Score = CalculateScore(report);

        _logger.LogInformation(
            "Quality Score für {Type}: {Score}/100",
            doc.TypeName, report.Score);

        return report;
    }

    private async Task<ExampleCompileResult> CompileExampleAsync(
        string example,
        ClassDocContext context,
        CancellationToken ct)
    {
        // Roslyn Scripting für Kompilierung
        var options = ScriptOptions.Default
            .AddReferences(typeof(object).Assembly)
            .AddImports(context.Namespace);

        try
        {
            var script = CSharpScript.Create(example, options);
            var diagnostics = script.Compile(ct);

            var errors = diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList();

            return new ExampleCompileResult
            {
                Code = example,
                Success = !errors.Any(),
                Errors = errors.Select(e => e.GetMessage()).ToList()
            };
        }
        catch (Exception ex)
        {
            return new ExampleCompileResult
            {
                Code = example,
                Success = false,
                Errors = [ex.Message]
            };
        }
    }

    private int CalculateScore(QualityReport report)
    {
        var score = 0;

        // Basis-Struktur (40 Punkte)
        if (report.HasSummary) score += 20;
        if (report.HasRemarks) score += 10;
        if (report.HasExamples) score += 10;

        // Beispiele kompilieren (20 Punkte)
        if (report.ExampleResults.All(e => e.Success))
            score += 20;
        else if (report.ExampleResults.Any(e => e.Success))
            score += 10;

        // Coverage (40 Punkte)
        score += (int)(report.MethodsCoverage * 15);
        score += (int)(report.PropertiesCoverage * 15);
        score += (int)(report.ParametersCoverage * 10);

        return Math.Min(100, score);
    }
}

public record QualityReport
{
    public required string TypeName { get; init; }
    public bool HasSummary { get; set; }
    public bool HasRemarks { get; set; }
    public bool HasExamples { get; set; }
    public List<ExampleCompileResult> ExampleResults { get; } = [];
    public double MethodsCoverage { get; set; }
    public double PropertiesCoverage { get; set; }
    public double ParametersCoverage { get; set; }
    public int Score { get; set; }
    public bool Passed => Score >= 80;
}
```

---

## 7. Konfiguration

### 7.1 appsettings.json

```json
{
  "DocGen": {
    "DefaultProvider": "Anthropic",
    "MinQualityScore": 80,
    "MaxParallelRequests": 5,
    "GenerateDiagrams": true,
    "OutputFormat": "DocFX",
    "OutputPath": "./docs/api"
  },
  "Providers": {
    "Anthropic": {
      "ApiKey": "${ANTHROPIC_API_KEY}",
      "DefaultModel": "claude-sonnet-4-20250514"
    },
    "OpenAI": {
      "ApiKey": "${OPENAI_API_KEY}",
      "DefaultModel": "gpt-4o"
    },
    "AzureOpenAI": {
      "ApiKey": "${AZURE_OPENAI_API_KEY}",
      "BaseUrl": "https://your-resource.openai.azure.com/",
      "DefaultModel": "gpt-4o",
      "ApiVersion": "2024-02-15-preview"
    }
  },
  "DocFX": {
    "Template": "modern",
    "SiteName": "BAUERGROUP.Shared API Reference",
    "BaseUrl": "https://docs.bauergroup.dev"
  }
}
```

### 7.2 CLI Verwendung

```bash
# Standard-Generierung mit Anthropic
dotnet docgen generate --source ./src --output ./docs/api

# Mit spezifischem Provider
dotnet docgen generate --provider OpenAI --model gpt-4o

# Nur Validierung (keine API-Kosten)
dotnet docgen check --source ./src --min-coverage 90

# Einzelne Klasse dokumentieren
dotnet docgen generate --type "BAUERGROUP.Shared.Core.Logging.BGLogger"

# Mit Kosten-Limit
dotnet docgen generate --max-cost 5.00

# Trockenlauf (zeigt was generiert würde)
dotnet docgen generate --dry-run
```

---

## 8. GitHub Actions Workflow

```yaml
name: 📚 Generate Documentation

on:
  push:
    branches: [main]
    paths:
      - 'src/**/*.cs'
      - 'docs/**'
  pull_request:
    paths:
      - 'src/**/*.cs'
  workflow_dispatch:
    inputs:
      provider:
        description: 'AI Provider'
        type: choice
        options:
          - Anthropic
          - OpenAI
        default: Anthropic
      force-regenerate:
        description: 'Regenerate all documentation'
        type: boolean
        default: false

env:
  DOTNET_VERSION: '10.0.x'

jobs:
  # PR: Nur prüfen
  check:
    if: github.event_name == 'pull_request'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Check Documentation Coverage
        run: |
          dotnet run --project tools/DocGen -- check \
            --source ./src \
            --min-coverage 80

      - name: Post Coverage Report
        uses: actions/github-script@v7
        with:
          script: |
            const fs = require('fs');
            const report = fs.readFileSync('docs/coverage-report.md', 'utf8');
            github.rest.issues.createComment({
              issue_number: context.issue.number,
              owner: context.repo.owner,
              repo: context.repo.repo,
              body: report
            });

  # Main: Generieren und deployen
  generate:
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0  # Für Git-History

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Cache NuGet packages
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: nuget-${{ hashFiles('**/*.csproj') }}

      - name: Generate Documentation
        env:
          ANTHROPIC_API_KEY: ${{ secrets.ANTHROPIC_API_KEY }}
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
        run: |
          dotnet run --project tools/DocGen -- generate \
            --source ./src \
            --output ./docs/api \
            --provider ${{ inputs.provider || 'Anthropic' }} \
            --quality-gate 80 \
            ${{ inputs.force-regenerate && '--force' || '' }}

      - name: Install DocFX
        run: dotnet tool install -g docfx

      - name: Build Documentation Site
        run: docfx docs/docfx.json

      - name: Add Search Index
        run: |
          npm install -g pagefind
          pagefind --source docs/_site --output-path docs/_site/pagefind

      - name: Deploy to GitHub Pages
        uses: peaceiris/actions-gh-pages@v4
        with:
          github_token: ${{ secrets.GITHUB_TOKEN }}
          publish_dir: ./docs/_site
          cname: docs.bauergroup.dev

      - name: Report Costs
        run: |
          echo "## Documentation Generation Report" >> $GITHUB_STEP_SUMMARY
          cat docs/generation-report.md >> $GITHUB_STEP_SUMMARY
```

---

## 9. Projektstruktur

```
BAUERGROUP.Shared/
├── src/                              # Bibliotheks-Quellcode
├── tests/                            # Tests
├── tools/
│   └── DocGen/                       # Dokumentations-Tool
│       ├── DocGen.csproj
│       ├── Program.cs
│       ├── Commands/
│       │   ├── GenerateCommand.cs
│       │   ├── CheckCommand.cs
│       │   └── ValidateCommand.cs
│       ├── Analysis/
│       │   ├── RoslynCodeAnalyzer.cs
│       │   └── Models.cs
│       ├── Providers/
│       │   ├── IAIProvider.cs
│       │   ├── AnthropicProvider.cs
│       │   ├── OpenAIProvider.cs
│       │   └── AIProviderFactory.cs
│       ├── Generation/
│       │   ├── DocumentationGenerator.cs
│       │   └── PromptTemplates.cs
│       └── Validation/
│           └── DocumentationQualityGate.cs
├── docs/
│   ├── docfx.json                    # DocFX Konfiguration
│   ├── toc.yml                       # Inhaltsverzeichnis
│   ├── index.md                      # Startseite
│   ├── api/                          # Auto-generierte API-Docs
│   ├── articles/                     # Manuelle Guides
│   │   ├── getting-started.md
│   │   ├── logging.md
│   │   └── error-tracking.md
│   └── _site/                        # Generierte Website
└── .github/
    └── workflows/
        └── generate-docs.yml
```

---

## 10. Implementierungsplan

### Phase 1: Foundation (Woche 1-2)
- [ ] Tool-Projekt `DocGen` erstellen
- [ ] AI Provider Interface + Anthropic Implementation
- [ ] Basis Roslyn Analyzer
- [ ] CLI Grundstruktur

### Phase 2: Generation (Woche 3-4)
- [ ] Prompt Templates entwickeln
- [ ] Documentation Generator
- [ ] OpenAI Provider
- [ ] Parallel Processing

### Phase 3: Quality (Woche 5)
- [ ] Quality Gate Implementation
- [ ] Code-Beispiel Kompilierung
- [ ] Coverage Reports

### Phase 4: Integration (Woche 6)
- [ ] DocFX Integration
- [ ] GitHub Actions Workflow
- [ ] GitHub Pages Deployment

### Phase 5: Enhancement (Woche 7-8)
- [ ] Mermaid Diagramm-Generierung
- [ ] Pagefind Suche
- [ ] AI Chat (optional)

---

## 11. Anhang

### A. Geschätzte Kosten pro Release

| Provider | Modell | Input Tokens | Output Tokens | Kosten |
|----------|--------|--------------|---------------|--------|
| Anthropic | Claude Sonnet | ~150.000 | ~115.000 | ~$2.20 |
| Anthropic | Claude Haiku | ~50.000 | ~40.000 | ~$0.07 |
| OpenAI | GPT-4o | ~150.000 | ~115.000 | ~$2.50 |
| OpenAI | GPT-4o-mini | ~150.000 | ~115.000 | ~$0.10 |

**Empfehlung**: Claude Sonnet für Hauptdokumentation, Haiku für Diagramme

### B. Referenzen

- [Roslyn Documentation](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/)
- [DocFX Documentation](https://dotnet.github.io/docfx/)
- [Anthropic API Reference](https://docs.anthropic.com/claude/reference/)
- [OpenAI API Reference](https://platform.openai.com/docs/api-reference)

---

*Dokument erstellt: 2026-01-18*
*Version: 1.0*
