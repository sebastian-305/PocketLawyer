using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PocketLawyer.Application.Agents;
using PocketLawyer.Application.Exceptions;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Application.UseCases;
using PocketLawyer.Domain.Dtos.Shared;
using PocketLawyer.Domain.Dtos.Rental;
using PocketLawyer.Domain.Dtos.Employment;
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
    public async Task<ActionResult> GetEmploymentMockData()
    {
        _logger.LogInformation("Entering {Method}", nameof(GetEmploymentMockData));

        string responsesPath = Path.Combine(
            _environment.ContentRootPath,
            _paths.ResponsesPath);

        string basicFilePath = Path.Combine(responsesPath, "employment_basic_mock.json");
        string premiumFilePath = Path.Combine(responsesPath, "employment_premium_mock.json");

        if (!System.IO.File.Exists(basicFilePath) || !System.IO.File.Exists(premiumFilePath))
            return StatusCode(StatusCodes.Status500InternalServerError, "Keine Employment-Beispieldaten gefunden.");

        try
        {
            string basicContent = await System.IO.File.ReadAllTextAsync(basicFilePath);
            string premiumContent = await System.IO.File.ReadAllTextAsync(premiumFilePath);

            var basicDto = JsonSerializer.Deserialize<EmploymentBasicAnalysisDto>(basicContent);
            var premiumDto = JsonSerializer.Deserialize<EmploymentPremiumAnalysisDto>(premiumContent);

            if (basicDto == null || premiumDto == null)
                return BadRequest("Invalid employment mock data");

            // Return combined format expected by frontend
            var combined = new
            {
                basic = basicDto,
                premium = premiumDto
            };

            return Ok(combined);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read or parse employment mock data");
            return StatusCode(StatusCodes.Status500InternalServerError, "Fehler beim Lesen der Beispieldaten.");
        }
    }

    [HttpGet("/rental-mockdata")]
    public async Task<ActionResult> GetRentalMockData()
    {
        _logger.LogInformation("Entering {Method}", nameof(GetRentalMockData));

        string responsesPath = Path.Combine(
            _environment.ContentRootPath,
            _paths.ResponsesPath);

        string basicFilePath = Path.Combine(responsesPath, "rental_basic_mock.json");
        string premiumFilePath = Path.Combine(responsesPath, "rental_premium_mock.json");

        if (!System.IO.File.Exists(basicFilePath) || !System.IO.File.Exists(premiumFilePath))
            return StatusCode(StatusCodes.Status500InternalServerError, "Keine Rental-Beispieldaten gefunden.");

        try
        {
            string basicContent = await System.IO.File.ReadAllTextAsync(basicFilePath);
            string premiumContent = await System.IO.File.ReadAllTextAsync(premiumFilePath);

            var basicDto = JsonSerializer.Deserialize<RentalBasicAnalysisDto>(basicContent);
            var premiumDto = JsonSerializer.Deserialize<RentalPremiumAnalysisDto>(premiumContent);

            if (basicDto == null || premiumDto == null)
                return BadRequest("Invalid rental mock data");

            // Return combined format expected by frontend
            var combined = new
            {
                basic = basicDto,
                premium = premiumDto
            };

            return Ok(combined);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read or parse rental mock data");
            return StatusCode(StatusCodes.Status500InternalServerError, "Fehler beim Lesen der Beispieldaten.");
        }
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
