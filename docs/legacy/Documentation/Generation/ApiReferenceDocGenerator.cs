using PocketLawyer.Documentation.Extraction;

namespace PocketLawyer.Documentation.Generation;

/// <summary>
/// Generates API_REFERENCE.md documentation from extracted endpoint information.
/// Includes WebSocket and HTTP endpoints, message formats, error responses, and API diagrams.
/// </summary>
/// <remarks>
/// Validates Requirements: 2.1, 2.2, 2.3, 2.4
/// </remarks>
public class ApiReferenceDocGenerator
{
    private readonly MarkdownGenerator _markdown;
    private readonly MermaidDiagramGenerator _diagramGenerator;
    private readonly DocumentationLanguage _lang;

    public ApiReferenceDocGenerator()
    {
        _markdown = new MarkdownGenerator();
        _diagramGenerator = new MermaidDiagramGenerator();
        _lang = DocumentationLanguage.German;
    }

    /// <summary>
    /// Generates complete API_REFERENCE.md documentation from extracted endpoints and DTOs.
    /// Requirement 2.1: WHEN documenting WebSocket endpoints, THE System SHALL extract endpoint paths from Web/Program.cs and Infrastructure/WebSocketAnalysisHandler.cs
    /// Requirement 2.2: WHEN documenting HTTP endpoints, THE System SHALL extract routes from Web/PocketController.cs attributes
    /// </summary>
    /// <param name="endpoints">List of extracted endpoints</param>
    /// <param name="dtoFiles">List of DTO source files for message format documentation</param>
    /// <param name="exceptionFiles">List of exception source files for error documentation</param>
    /// <param name="projectRoot">Root directory of the project for file references</param>
    /// <returns>Complete API_REFERENCE.md content</returns>
    public string GenerateApiReferenceDocumentation(
        IEnumerable<EndpointInfo> endpoints,
        IEnumerable<SourceFileInfo> dtoFiles,
        IEnumerable<SourceFileInfo> exceptionFiles,
        string projectRoot = "")
    {
        _markdown.Clear();

        // Header
        _markdown.H1(_lang.ApiTitle);
        _markdown.Paragraph("Dieses Dokument beschreibt alle API-Endpunkte im PocketLawyer-System. " +
                          "Alle Informationen werden aus dem tatsächlichen Quellcode in den Web/ und Infrastructure/ Verzeichnissen extrahiert.");

        // Table of Contents
        GenerateTableOfContents(endpoints);

        // Overview
        GenerateOverviewSection(endpoints);

        // WebSocket Endpoints
        GenerateWebSocketEndpointsSection(endpoints, projectRoot);

        // HTTP Endpoints
        GenerateHttpEndpointsSection(endpoints, projectRoot);

        // Message Formats
        GenerateMessageFormatsSection(dtoFiles, projectRoot);

        // Error Handling
        GenerateErrorHandlingSection(exceptionFiles, projectRoot);

        // Diagrams
        GenerateDiagramsSection(endpoints);

        return _markdown.Content;
    }

    private void GenerateTableOfContents(IEnumerable<EndpointInfo> endpoints)
    {
        _markdown.H2("Inhaltsverzeichnis");

        var tocItems = new List<string>
        {
            MarkdownGenerator.AnchorLink(_lang.Overview, AnchorGenerator.Generate(_lang.Overview))
        };

        if (endpoints.Any(e => e.IsWebSocket))
        {
            tocItems.Add(MarkdownGenerator.AnchorLink(_lang.ApiWebSocketEndpoints, AnchorGenerator.Generate(_lang.ApiWebSocketEndpoints)));
        }

        if (endpoints.Any(e => !e.IsWebSocket))
        {
            tocItems.Add(MarkdownGenerator.AnchorLink(_lang.ApiHttpEndpoints, AnchorGenerator.Generate(_lang.ApiHttpEndpoints)));
        }

        tocItems.Add(MarkdownGenerator.AnchorLink(_lang.MessageFormats, AnchorGenerator.Generate(_lang.MessageFormats)));
        tocItems.Add(MarkdownGenerator.AnchorLink(_lang.ErrorHandling, AnchorGenerator.Generate(_lang.ErrorHandling)));
        tocItems.Add(MarkdownGenerator.AnchorLink(_lang.Diagrams, AnchorGenerator.Generate(_lang.Diagrams)));

        _markdown.UnorderedList(tocItems);
    }

    private void GenerateOverviewSection(IEnumerable<EndpointInfo> endpoints)
    {
        _markdown.H2(_lang.Overview);

        var webSocketCount = endpoints.Count(e => e.IsWebSocket);
        var httpCount = endpoints.Count(e => !e.IsWebSocket);

        _markdown.Paragraph($"Die PocketLawyer API bietet {webSocketCount} WebSocket-Endpunkt(e) und {httpCount} HTTP-Endpunkt(e) " +
                          "für Vertragsanalyse und Schema-Abruf.");

        _markdown.H3(_lang.BaseUrl);
        _markdown.Paragraph("Die API wird unter der Basis-URL der Anwendung gehostet. Standardmäßig ist dies:");
        _markdown.UnorderedList(new[]
        {
            $"{MarkdownGenerator.Bold("Entwicklung:")} {MarkdownGenerator.InlineCode("http://localhost:5112")}",
            $"{MarkdownGenerator.Bold("Produktion:")} Konfiguriert über die Hosting-Umgebung"
        });

        _markdown.H3(_lang.Authentication);
        _markdown.Paragraph("Derzeit erfordert die API keine Authentifizierung. " +
                          "Dies kann sich in zukünftigen Versionen für Produktionsumgebungen ändern.");
    }

    private void GenerateWebSocketEndpointsSection(IEnumerable<EndpointInfo> endpoints, string projectRoot)
    {
        var webSocketEndpoints = endpoints.Where(e => e.IsWebSocket).ToList();

        if (!webSocketEndpoints.Any())
        {
            return; // Skip section if no WebSocket endpoints
        }

        _markdown.H2(_lang.ApiWebSocketEndpoints);
        _markdown.Paragraph("WebSocket-Endpunkte bieten Echtzeit-Vertragsanalyse. " +
                          "Clients verbinden sich über das WebSocket-Protokoll und senden Vertragsdateien zur Analyse.");

        foreach (var endpoint in webSocketEndpoints.OrderBy(e => e.Path))
        {
            GenerateWebSocketEndpointDocumentation(endpoint, projectRoot);
        }
    }

    private void GenerateWebSocketEndpointDocumentation(EndpointInfo endpoint, string projectRoot)
    {
        _markdown.H3($"WebSocket: {endpoint.Path}");

        // Source reference
        var relativePath = string.IsNullOrEmpty(projectRoot)
            ? endpoint.FilePath
            : Path.GetRelativePath(projectRoot, endpoint.FilePath).Replace("\\", "/");
        _markdown.Paragraph($"**{_lang.Source}:** {MarkdownGenerator.MethodReference(endpoint.ActionName, endpoint.ControllerName, relativePath)}");

        // Description
        if (!string.IsNullOrEmpty(endpoint.Description))
        {
            _markdown.Paragraph(endpoint.Description);
        }

        // Connection
        _markdown.H4("Verbindung");
        _markdown.Paragraph($"Verbinden zu: {MarkdownGenerator.InlineCode($"ws://{{host}}{endpoint.Path}")}");

        // Message Format
        _markdown.H4("Nachrichtenformat");
        _markdown.Paragraph("Senden Sie Vertragsdateien als WebSocket-Nachrichten im folgenden Format:");

        var messageFormatParam = endpoint.Parameters.FirstOrDefault(p => p.Type.Contains("Dto") || p.Type.Contains("File"));
        if (messageFormatParam != null)
        {
            _markdown.Paragraph($"Nachrichtentyp: {MarkdownGenerator.InlineCode(messageFormatParam.Type)}");
            _markdown.Paragraph($"Siehe Abschnitt {MarkdownGenerator.AnchorLink(_lang.MessageFormats, AnchorGenerator.Generate(_lang.MessageFormats))} für Details.");
        }

        // Response Format
        _markdown.H4("Antwortformat");
        if (!string.IsNullOrEmpty(endpoint.ReturnType) && endpoint.ReturnType != "void" && endpoint.ReturnType != "Task")
        {
            var returnType = endpoint.ReturnType
                .Replace("Task<", "")
                .Replace(">", "")
                .Replace("IActionResult", "JSON")
                .Trim();
            _markdown.Paragraph($"Antworttyp: {MarkdownGenerator.InlineCode(returnType)}");
            _markdown.Paragraph($"Siehe Abschnitt {MarkdownGenerator.AnchorLink(_lang.MessageFormats, AnchorGenerator.Generate(_lang.MessageFormats))} für Details.");
        }

        // Parameters
        if (endpoint.Parameters.Any())
        {
            _markdown.H4(_lang.Parameters);
            var paramRows = endpoint.Parameters
                .Where(p => !p.Source.Contains("Services")) // Exclude injected services
                .Select(p => new[]
                {
                    p.Name,
                    p.Type,
                    p.Source,
                    p.DefaultValue ?? "-"
                });

            if (paramRows.Any())
            {
                _markdown.Table(
                    new[] { _lang.Name, _lang.Type, _lang.Source, _lang.Default },
                    paramRows
                );
            }
        }

        _markdown.BlankLine();
    }

    private void GenerateHttpEndpointsSection(IEnumerable<EndpointInfo> endpoints, string projectRoot)
    {
        var httpEndpoints = endpoints.Where(e => !e.IsWebSocket).ToList();

        if (!httpEndpoints.Any())
        {
            return; // Skip section if no HTTP endpoints
        }

        _markdown.H2(_lang.ApiHttpEndpoints);
        _markdown.Paragraph("HTTP-Endpunkte bieten synchrone Operationen für Schema-Abruf und andere Hilfsfunktionen.");

        // Group by HTTP method
        var groupedEndpoints = httpEndpoints
            .GroupBy(e => e.HttpMethod)
            .OrderBy(g => g.Key);

        foreach (var group in groupedEndpoints)
        {
            _markdown.H3($"{group.Key} Endpunkte");

            foreach (var endpoint in group.OrderBy(e => e.Path))
            {
                GenerateHttpEndpointDocumentation(endpoint, projectRoot);
            }
        }
    }

    private void GenerateHttpEndpointDocumentation(EndpointInfo endpoint, string projectRoot)
    {
        _markdown.H4($"{endpoint.HttpMethod} {endpoint.Path}");

        // Source reference
        var relativePath = string.IsNullOrEmpty(projectRoot)
            ? endpoint.FilePath
            : Path.GetRelativePath(projectRoot, endpoint.FilePath).Replace("\\", "/");
        _markdown.Paragraph($"**{_lang.Source}:** {MarkdownGenerator.MethodReference(endpoint.ActionName, endpoint.ControllerName, relativePath)}");

        // Description
        if (!string.IsNullOrEmpty(endpoint.Description))
        {
            _markdown.Paragraph(endpoint.Description);
        }

        // Parameters
        if (endpoint.Parameters.Any())
        {
            _markdown.H5(_lang.Parameters);
            var paramRows = endpoint.Parameters
                .Where(p => !p.Source.Contains("Services")) // Exclude injected services
                .Select(p => new[]
                {
                    p.Name,
                    p.Type,
                    p.Source,
                    p.DefaultValue ?? "-"
                });

            if (paramRows.Any())
            {
                _markdown.Table(
                    new[] { _lang.Name, _lang.Type, _lang.Source, _lang.Default },
                    paramRows
                );
            }
        }

        // Response
        _markdown.H5("Antwort");
        if (!string.IsNullOrEmpty(endpoint.ReturnType))
        {
            var returnType = endpoint.ReturnType
                .Replace("Task<", "")
                .Replace(">", "")
                .Replace("IActionResult", "JSON")
                .Trim();
            _markdown.Paragraph($"Gibt zurück: {MarkdownGenerator.InlineCode(returnType)}");
        }

        // Example
        _markdown.H5("Beispielanfrage");
        var exampleUrl = endpoint.Path;
        
        // Replace route parameters with example values
        if (exampleUrl.Contains("{contractType}"))
            exampleUrl = exampleUrl.Replace("{contractType}", "employment");
        if (exampleUrl.Contains("{tier}"))
            exampleUrl = exampleUrl.Replace("{tier}", "basic");
        if (exampleUrl.Contains("{step}"))
            exampleUrl = exampleUrl.Replace("{step}", "1");

        _markdown.CodeBlock($"{endpoint.HttpMethod} {exampleUrl} HTTP/1.1\nHost: localhost:5112", "http");

        _markdown.BlankLine();
    }

    private void GenerateMessageFormatsSection(IEnumerable<SourceFileInfo> dtoFiles, string projectRoot)
    {
        _markdown.H2(_lang.MessageFormats);
        _markdown.Paragraph("Dieser Abschnitt beschreibt die Datenstrukturen, die in API-Anfragen und -Antworten verwendet werden. " +
                          "Alle DTOs sind im Verzeichnis Domain/Dtos/ definiert.");

        // Requirement 2.3: WHEN documenting message formats, THE System SHALL extract DTO structures from Domain/Dtos/ files
        _markdown.Paragraph($"Für vollständige DTO-Dokumentation siehe {MarkdownGenerator.Link("DATA_MODELS.md", "DATA_MODELS.md")}.");

        // List key DTOs used in API
        _markdown.H3("Wichtige DTOs");

        var keyDtoNames = new[]
        {
            "WebSocketFileDto",
            "EmploymentContractAnalysisDto",
            "RentContractAnalysisDto",
            "AnalysisDto",
            "ContractValidationDto",
            "FileContentDto"
        };

        var keyDtos = dtoFiles
            .SelectMany(f => f.Classes.Concat(f.Records).Concat(f.Interfaces))
            .Where(c => keyDtoNames.Contains(c.Name))
            .ToList();

        foreach (var dto in keyDtos.OrderBy(d => d.Name))
        {
            GenerateKeyDtoSummary(dto, projectRoot);
        }

        if (!keyDtos.Any())
        {
            _markdown.Paragraph(MarkdownGenerator.Italic("Keine wichtigen DTOs gefunden. Siehe DATA_MODELS.md für alle verfügbaren DTOs."));
        }
    }

    private void GenerateKeyDtoSummary(ClassInfo dto, string projectRoot)
    {
        _markdown.H4(dto.Name);

        // Namespace and source
        _markdown.Paragraph($"**{_lang.Namespace}:** {MarkdownGenerator.InlineCode(dto.Namespace)}");

        // Properties summary
        if (dto.Properties.Any())
        {
            _markdown.Paragraph($"**{_lang.Properties}:** ({dto.Properties.Count})");

            var propertyList = dto.Properties
                .Take(10) // Limit to first 10 properties for summary
                .Select(p => $"{MarkdownGenerator.InlineCode(p.Name)}: {MarkdownGenerator.InlineCode(p.Type)}");

            _markdown.UnorderedList(propertyList, 1);

            if (dto.Properties.Count > 10)
            {
                _markdown.Paragraph(MarkdownGenerator.Italic($"... und {dto.Properties.Count - 10} weitere Eigenschaften. Siehe DATA_MODELS.md für vollständige Details."));
            }
        }

        _markdown.BlankLine();
    }

    private void GenerateErrorHandlingSection(IEnumerable<SourceFileInfo> exceptionFiles, string projectRoot)
    {
        _markdown.H2(_lang.ErrorHandling);
        _markdown.Paragraph("Die API verwendet Standard-HTTP-Statuscodes und benutzerdefinierte Exception-Typen für die Fehlerbehandlung. " +
                          "Alle benutzerdefinierten Exceptions sind im Verzeichnis Application/Exceptions/ definiert.");

        // Requirement 2.4: WHEN documenting error responses, THE System SHALL extract exception types from Application/Exceptions/ files
        _markdown.H3("HTTP-Statuscodes");
        _markdown.Table(
            new[] { "Statuscode", _lang.Description },
            new[]
            {
                new[] { "200 OK", "Anfrage erfolgreich" },
                new[] { "400 Bad Request", "Ungültige Anfrageparameter oder fehlerhafte Daten" },
                new[] { "404 Not Found", "Angeforderte Ressource nicht gefunden" },
                new[] { "500 Internal Server Error", "Serverfehler während der Verarbeitung" }
            }
        );

        _markdown.H3("Exception-Typen");
        _markdown.Paragraph("Die folgenden benutzerdefinierten Exception-Typen können während API-Operationen auftreten:");

        var exceptions = exceptionFiles
            .SelectMany(f => f.Classes)
            .Where(c => c.Name.EndsWith("Exception"))
            .OrderBy(c => c.Name)
            .ToList();

        foreach (var exception in exceptions)
        {
            GenerateExceptionDocumentation(exception, projectRoot);
        }

        if (!exceptions.Any())
        {
            _markdown.Paragraph(MarkdownGenerator.Italic("Keine benutzerdefinierten Exceptions gefunden."));
        }
    }

    private void GenerateExceptionDocumentation(ClassInfo exception, string projectRoot)
    {
        _markdown.H4(exception.Name);

        // Source reference
        var exceptionFile = $"Application/Exceptions/{exception.Name}.cs";
        _markdown.Paragraph($"**{_lang.Source}:** {MarkdownGenerator.FileReference(exceptionFile)}");

        // Base class
        if (!string.IsNullOrEmpty(exception.BaseClass))
        {
            _markdown.Paragraph($"**{_lang.InheritsFrom}:** {MarkdownGenerator.InlineCode(exception.BaseClass)}");
        }

        // Description from XML comments (if available)
        // Note: XML comments are extracted by SourceFileAnalyzer if present
        var xmlComment = exception.Properties.FirstOrDefault()?.Attributes
            .FirstOrDefault(a => a.Name == "XmlComment")?.Arguments.FirstOrDefault();
        
        if (!string.IsNullOrEmpty(xmlComment))
        {
            _markdown.Paragraph(xmlComment);
        }

        _markdown.BlankLine();
    }

    private void GenerateDiagramsSection(IEnumerable<EndpointInfo> endpoints)
    {
        _markdown.H2(_lang.Diagrams);

        // API Endpoint Overview
        _markdown.H3("API-Endpunkt-Übersicht");
        _markdown.Paragraph("Überblick über alle API-Endpunkte:");

        GenerateEndpointOverviewDiagram(endpoints);

        // Message Flow Diagram
        _markdown.H3("WebSocket-Nachrichtenfluss");
        _markdown.Paragraph("Typischer WebSocket-Interaktionsablauf:");

        GenerateWebSocketFlowDiagram();
    }

    private void GenerateEndpointOverviewDiagram(IEnumerable<EndpointInfo> endpoints)
    {
        _markdown.Raw("```mermaid\n");
        _markdown.Raw("graph LR\n");
        _markdown.Raw("    Client[Client Application]\n");
        _markdown.Raw("    API[PocketLawyer API]\n");
        _markdown.Raw("    \n");
        _markdown.Raw("    Client -->|WebSocket| API\n");
        _markdown.Raw("    Client -->|HTTP| API\n");
        _markdown.Raw("    \n");

        // Add WebSocket endpoints
        var wsEndpoints = endpoints.Where(e => e.IsWebSocket).ToList();
        if (wsEndpoints.Any())
        {
            _markdown.Raw("    subgraph WebSocket Endpoints\n");
            foreach (var endpoint in wsEndpoints)
            {
                var safeName = endpoint.Path.Replace("/", "_").Replace("-", "_");
                _markdown.Raw($"        WS{safeName}[{endpoint.Path}]\n");
            }
            _markdown.Raw("    end\n");
            _markdown.Raw("    \n");
        }

        // Add HTTP endpoints
        var httpEndpoints = endpoints.Where(e => !e.IsWebSocket).Take(5).ToList(); // Limit for diagram clarity
        if (httpEndpoints.Any())
        {
            _markdown.Raw("    subgraph HTTP Endpoints\n");
            foreach (var endpoint in httpEndpoints)
            {
                var safeName = endpoint.Path.Replace("/", "_").Replace("-", "_").Replace("{", "").Replace("}", "");
                _markdown.Raw($"        HTTP{safeName}[{endpoint.HttpMethod} {endpoint.Path}]\n");
            }
            _markdown.Raw("    end\n");
        }

        _markdown.Raw("```\n\n");
    }

    private void GenerateWebSocketFlowDiagram()
    {
        _markdown.Raw("```mermaid\n");
        _markdown.Raw("sequenceDiagram\n");
        _markdown.Raw("    participant Client\n");
        _markdown.Raw("    participant API\n");
        _markdown.Raw("    participant Agent\n");
        _markdown.Raw("    participant AIService\n");
        _markdown.Raw("    \n");
        _markdown.Raw("    Client->>API: Connect WebSocket\n");
        _markdown.Raw("    API-->>Client: Connection Established\n");
        _markdown.Raw("    Client->>API: Send Contract File\n");
        _markdown.Raw("    API->>Agent: Process Contract\n");
        _markdown.Raw("    Agent->>AIService: Analyze Content\n");
        _markdown.Raw("    AIService-->>Agent: Analysis Result\n");
        _markdown.Raw("    Agent-->>API: Formatted Response\n");
        _markdown.Raw("    API-->>Client: Send Analysis Result\n");
        _markdown.Raw("    Client->>API: Close Connection\n");
        _markdown.Raw("```\n\n");
    }

    /// <summary>
    /// Writes the generated documentation to a file.
    /// </summary>
    public void WriteToFile(string filePath)
    {
        _markdown.WriteToFile(filePath);
    }
}
