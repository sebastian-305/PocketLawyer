using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PocketLawyer.Documentation.Extraction;

/// <summary>
/// Extracts API endpoint information from controller files and Program.cs.
/// Analyzes HTTP routes, WebSocket endpoints, route attributes, parameters, and handler methods.
/// </summary>
/// <remarks>
/// Validates Requirements: 2.1, 2.2
/// </remarks>
public class EndpointExtractor
{
    private readonly SourceFileAnalyzer _sourceFileAnalyzer;

    public EndpointExtractor()
    {
        _sourceFileAnalyzer = new SourceFileAnalyzer();
    }

    /// <summary>
    /// Extracts all endpoints from a controller file.
    /// </summary>
    /// <param name="filePath">Path to the controller C# file</param>
    /// <returns>List of endpoint information</returns>
    public List<EndpointInfo> ExtractEndpointsFromController(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Controller file not found: {filePath}");
        }

        var sourceCode = File.ReadAllText(filePath);
        return ExtractEndpointsFromControllerSource(sourceCode, filePath);
    }

    /// <summary>
    /// Extracts all endpoints from controller source code.
    /// </summary>
    /// <param name="sourceCode">The C# source code to analyze</param>
    /// <param name="filePath">Optional file path for reference</param>
    /// <returns>List of endpoint information</returns>
    public List<EndpointInfo> ExtractEndpointsFromControllerSource(string sourceCode, string? filePath = null)
    {
        var sourceFileInfo = _sourceFileAnalyzer.AnalyzeSource(sourceCode, filePath);
        var endpoints = new List<EndpointInfo>();

        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetRoot();

        // Find controller classes (classes with [ApiController] attribute or ending with "Controller")
        var controllerClasses = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(c => IsControllerClass(c))
            .ToList();

        foreach (var controllerClass in controllerClasses)
        {
            var controllerName = controllerClass.Identifier.Text;
            var controllerRoutePrefix = ExtractControllerRoutePrefix(controllerClass);

            // Extract endpoints from methods
            var methods = controllerClass.Members.OfType<MethodDeclarationSyntax>();
            foreach (var method in methods)
            {
                var methodEndpoints = ExtractEndpointsFromMethod(method, controllerName, controllerRoutePrefix, filePath);
                endpoints.AddRange(methodEndpoints);
            }
        }

        return endpoints;
    }

    /// <summary>
    /// Extracts WebSocket endpoint mappings from Program.cs.
    /// </summary>
    /// <param name="filePath">Path to Program.cs</param>
    /// <returns>List of WebSocket endpoint information</returns>
    public List<EndpointInfo> ExtractWebSocketEndpointsFromProgram(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Program file not found: {filePath}");
        }

        var sourceCode = File.ReadAllText(filePath);
        return ExtractWebSocketEndpointsFromProgramSource(sourceCode, filePath);
    }

    /// <summary>
    /// Extracts WebSocket endpoint mappings from Program.cs source code.
    /// </summary>
    /// <param name="sourceCode">The C# source code to analyze</param>
    /// <param name="filePath">Optional file path for reference</param>
    /// <returns>List of WebSocket endpoint information</returns>
    public List<EndpointInfo> ExtractWebSocketEndpointsFromProgramSource(string sourceCode, string? filePath = null)
    {
        var endpoints = new List<EndpointInfo>();

        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetRoot();

        // Look for app.UseWebSockets() to confirm WebSocket support
        var useWebSocketsCalls = root.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(inv => inv.Expression.ToString().Contains("UseWebSockets"))
            .ToList();

        if (useWebSocketsCalls.Any())
        {
            // WebSocket endpoints are configured in the controller, not in Program.cs
            // This method documents that WebSocket support is enabled
            endpoints.Add(new EndpointInfo
            {
                Path = "WebSocket Support Enabled",
                HttpMethod = "WebSocket",
                ControllerName = "Program",
                ActionName = "UseWebSockets",
                FilePath = filePath ?? string.Empty,
                IsWebSocket = true,
                Description = "WebSocket middleware is configured in Program.cs"
            });
        }

        return endpoints;
    }

    private bool IsControllerClass(ClassDeclarationSyntax classDeclaration)
    {
        // Check if class has [ApiController] attribute
        var hasApiControllerAttribute = classDeclaration.AttributeLists
            .SelectMany(al => al.Attributes)
            .Any(a => a.Name.ToString().Contains("ApiController"));

        // Check if class name ends with "Controller"
        var nameEndsWithController = classDeclaration.Identifier.Text.EndsWith("Controller");

        // Check if class inherits from ControllerBase or Controller
        var inheritsFromController = classDeclaration.BaseList?.Types
            .Any(t => t.ToString().Contains("ControllerBase") || t.ToString().Contains("Controller")) ?? false;

        return hasApiControllerAttribute || nameEndsWithController || inheritsFromController;
    }

    private string? ExtractControllerRoutePrefix(ClassDeclarationSyntax classDeclaration)
    {
        // Look for [Route] attribute on the controller class
        var routeAttribute = classDeclaration.AttributeLists
            .SelectMany(al => al.Attributes)
            .FirstOrDefault(a => a.Name.ToString().Contains("Route"));

        if (routeAttribute?.ArgumentList?.Arguments.Count > 0)
        {
            var routeValue = routeAttribute.ArgumentList.Arguments[0].ToString();
            return routeValue.Trim('"');
        }

        return null;
    }

    private List<EndpointInfo> ExtractEndpointsFromMethod(
        MethodDeclarationSyntax method,
        string controllerName,
        string? controllerRoutePrefix,
        string? filePath)
    {
        var endpoints = new List<EndpointInfo>();

        // Extract HTTP method attributes (HttpGet, HttpPost, etc.)
        var httpAttributes = method.AttributeLists
            .SelectMany(al => al.Attributes)
            .Where(a => IsHttpMethodAttribute(a.Name.ToString()))
            .ToList();

        if (!httpAttributes.Any())
        {
            return endpoints; // Not an endpoint method
        }

        foreach (var httpAttribute in httpAttributes)
        {
            var endpoint = new EndpointInfo
            {
                ControllerName = controllerName,
                ActionName = method.Identifier.Text,
                FilePath = filePath ?? string.Empty,
                HttpMethod = ExtractHttpMethod(httpAttribute.Name.ToString()),
                Path = ExtractRoutePath(httpAttribute, controllerRoutePrefix),
                Parameters = ExtractParameters(method),
                ReturnType = method.ReturnType.ToString(),
                Description = ExtractXmlDocComment(method),
                IsWebSocket = IsWebSocketEndpoint(method)
            };

            endpoints.Add(endpoint);
        }

        return endpoints;
    }

    private bool IsHttpMethodAttribute(string attributeName)
    {
        var httpMethods = new[] { "HttpGet", "HttpPost", "HttpPut", "HttpDelete", "HttpPatch", "HttpHead", "HttpOptions" };
        return httpMethods.Any(m => attributeName.Contains(m));
    }

    private string ExtractHttpMethod(string attributeName)
    {
        if (attributeName.Contains("HttpGet")) return "GET";
        if (attributeName.Contains("HttpPost")) return "POST";
        if (attributeName.Contains("HttpPut")) return "PUT";
        if (attributeName.Contains("HttpDelete")) return "DELETE";
        if (attributeName.Contains("HttpPatch")) return "PATCH";
        if (attributeName.Contains("HttpHead")) return "HEAD";
        if (attributeName.Contains("HttpOptions")) return "OPTIONS";
        return "UNKNOWN";
    }

    private string ExtractRoutePath(AttributeSyntax httpAttribute, string? controllerRoutePrefix)
    {
        string? routePath = null;

        // Extract route from attribute argument
        if (httpAttribute.ArgumentList?.Arguments.Count > 0)
        {
            var firstArg = httpAttribute.ArgumentList.Arguments[0].ToString();
            routePath = firstArg.Trim('"');
        }

        // Combine with controller route prefix if present
        if (!string.IsNullOrEmpty(controllerRoutePrefix) && !string.IsNullOrEmpty(routePath))
        {
            return $"{controllerRoutePrefix.TrimEnd('/')}/{routePath.TrimStart('/')}";
        }

        return routePath ?? string.Empty;
    }

    private List<EndpointParameterInfo> ExtractParameters(MethodDeclarationSyntax method)
    {
        var parameters = new List<EndpointParameterInfo>();

        foreach (var parameter in method.ParameterList.Parameters)
        {
            var paramInfo = new EndpointParameterInfo
            {
                Name = parameter.Identifier.Text,
                Type = parameter.Type?.ToString() ?? "unknown",
                DefaultValue = parameter.Default?.Value.ToString()
            };

            // Extract parameter source attributes ([FromBody], [FromQuery], [FromRoute], [FromServices], etc.)
            var sourceAttribute = parameter.AttributeLists
                .SelectMany(al => al.Attributes)
                .FirstOrDefault(a => a.Name.ToString().StartsWith("From"));

            if (sourceAttribute != null)
            {
                paramInfo.Source = sourceAttribute.Name.ToString();
            }
            else
            {
                // Infer source based on parameter type and position
                paramInfo.Source = InferParameterSource(parameter, method);
            }

            parameters.Add(paramInfo);
        }

        return parameters;
    }

    private string InferParameterSource(ParameterSyntax parameter, MethodDeclarationSyntax method)
    {
        var paramType = parameter.Type?.ToString() ?? string.Empty;

        // Service injection parameters
        if (paramType.Contains("Agent") || paramType.Contains("Service") || paramType.Contains("Logger"))
        {
            return "FromServices";
        }

        // Complex types are typically from body
        if (paramType.Contains("Dto") || paramType.Contains("Request") || paramType.Contains("Model"))
        {
            return "FromBody";
        }

        // Simple types in route path
        var httpAttribute = method.AttributeLists
            .SelectMany(al => al.Attributes)
            .FirstOrDefault(a => IsHttpMethodAttribute(a.Name.ToString()));

        if (httpAttribute?.ArgumentList?.Arguments.Count > 0)
        {
            var routePath = httpAttribute.ArgumentList.Arguments[0].ToString();
            if (routePath.Contains($"{{{parameter.Identifier.Text}}}"))
            {
                return "FromRoute";
            }
        }

        // Default to query for simple types
        return "FromQuery";
    }

    private string? ExtractXmlDocComment(MethodDeclarationSyntax method)
    {
        var trivia = method.GetLeadingTrivia();
        var docCommentTrivia = trivia
            .Where(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                       t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
            .FirstOrDefault();

        if (docCommentTrivia.HasStructure)
        {
            var structure = docCommentTrivia.GetStructure();
            var summaryElement = structure?.DescendantNodes()
                .OfType<XmlElementSyntax>()
                .FirstOrDefault(e => e.StartTag.Name.ToString() == "summary");

            if (summaryElement != null)
            {
                var summaryText = summaryElement.Content.ToString()
                    .Trim()
                    .Replace("\n", " ")
                    .Replace("\r", "");
                
                // Remove extra whitespace
                return System.Text.RegularExpressions.Regex.Replace(summaryText, @"\s+", " ");
            }
        }

        return null;
    }

    private bool IsWebSocketEndpoint(MethodDeclarationSyntax method)
    {
        // Check if method body contains WebSocket-related code
        var methodBody = method.Body?.ToString() ?? method.ExpressionBody?.ToString() ?? string.Empty;
        
        return methodBody.Contains("WebSocket") || 
               methodBody.Contains("IsWebSocketRequest") ||
               method.ParameterList.Parameters.Any(p => p.Type?.ToString().Contains("Agent") == true);
    }
}

/// <summary>
/// Information about an API endpoint.
/// </summary>
public class EndpointInfo
{
    public string Path { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string ControllerName { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public List<EndpointParameterInfo> Parameters { get; set; } = new();
    public string ReturnType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsWebSocket { get; set; }
}

/// <summary>
/// Information about an endpoint parameter.
/// </summary>
public class EndpointParameterInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty; // FromBody, FromQuery, FromRoute, FromServices, etc.
    public string? DefaultValue { get; set; }
}
