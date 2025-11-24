using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PocketLawyer.Application.Agents;
using PocketLawyer.Application.Exceptions;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Application.UseCases;
using PocketLawyer.Domain.Dtos;
using PocketLawyer.Infrastructure;
using PocketLawyer.Options;
using System.Text.Json;

namespace PocketLawyer.Web;

[ApiController]
public class PocketController : ControllerBase
{
    private readonly ILogger<PocketController> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly PathOptions _paths;
    private readonly WebSocketAnalysisHandler _webSocketHandler;
    private readonly ISchemaProvider _schemaProvider;

    public PocketController(
        ILogger<PocketController> logger,
        IOptions<PathOptions> pathOptions,
        IWebHostEnvironment environment,
        WebSocketAnalysisHandler webSocketHandler,
        ISchemaProvider schemaProvider)
    {
        _logger = logger;
        _paths = pathOptions.Value;
        _environment = environment;
        _webSocketHandler = webSocketHandler;
        _schemaProvider = schemaProvider;
    }

    /// <summary>
    /// Retrieves the JSON schema for a specific contract analysis step.
    /// </summary>
    /// <param name="contractType">Type of contract (e.g., "Rental", "Employment")</param>
    /// <param name="tier">Analysis tier (e.g., "Basic", "Premium")</param>
    /// <param name="step">Analysis step name (e.g., "General", "Costs")</param>
    /// <returns>JSON schema as JsonObject</returns>
    [HttpGet("/api/schemas/{contractType}/{tier}/{step}")]
    public ActionResult<object> GetSchema(string contractType, string tier, string step)
    {
        _logger.LogInformation("Retrieving schema for {ContractType}/{Tier}/{Step}", contractType, tier, step);

        try
        {
            var schema = _schemaProvider.GetSchemaForEndpoint(contractType, tier, step);
            return Ok(schema);
        }
        catch (SchemaNotFoundException ex)
        {
            _logger.LogWarning(ex, "Schema not found: {SchemaKey}", ex.SchemaKey);
            return NotFound(new { error = ex.Message, schemaKey = ex.SchemaKey });
        }
    }

    [HttpGet("/employment-mockdata")]
    public async Task<ActionResult<RentContractAnalysisDto>> GetMockData()
    {
        _logger.LogInformation("Entering {Method}", nameof(GetMockData));

        string responsesPath = Path.Combine(
            _environment.ContentRootPath,
            _paths.ResponsesPath);

        Directory.CreateDirectory(responsesPath);

        string mockContent;
        try
        {
            var files = Directory.GetFiles(responsesPath);
            if (files.Length == 0)
                return StatusCode(StatusCodes.Status500InternalServerError, "Keine Beispieldaten gefunden.");

            string filePath = files[Random.Shared.Next(files.Length)];
            mockContent = await System.IO.File.ReadAllTextAsync(filePath);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to read mock data");
            return StatusCode(StatusCodes.Status500InternalServerError, "Fehler beim Lesen der Beispieldaten.");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "No permission to access mock data");
            return StatusCode(StatusCodes.Status500InternalServerError, "Fehler beim Lesen der Beispieldaten.");
        }

        RentContractAnalysisDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<RentContractAnalysisDto>(mockContent);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse mock data");
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Fehler beim Lesen der Beispieldaten.");
        }
        if (dto == null)
            return BadRequest("Invalid mock data");

        return Ok(dto);
    }

    /// <summary>
    /// WebSocket endpoint for employment contract analysis.
    /// The agent is injected by the DI container based on the endpoint.
    /// </summary>
    [HttpGet("/employment-analysis")]
    public async Task<ActionResult> AnalyzeEmploymentViaWebSocket(
        [FromServices] EmploymentContractAgent agent)
    {
        _logger.LogInformation("Employment contract analysis request via {address}", Request.Host);
        if (!HttpContext.WebSockets.IsWebSocketRequest)
            return BadRequest("WebSocket connection required");

        await _webSocketHandler.HandleAsync(HttpContext, agent);
        return new EmptyResult();
    }

    /// <summary>
    /// WebSocket endpoint for rental contract analysis.
    /// The agent is injected by the DI container based on the endpoint.
    /// </summary>
    [HttpGet("/rental-analysis")]
    public async Task<ActionResult> AnalyzeRentalViaWebSocket(
        [FromServices] RentalContractAgent agent)
    {
        _logger.LogInformation("Rental contract analysis request via {address}", Request.Host);
        if (!HttpContext.WebSockets.IsWebSocketRequest)
            return BadRequest("WebSocket connection required");

        await _webSocketHandler.HandleAsync(HttpContext, agent);
        return new EmptyResult();
    }
}
