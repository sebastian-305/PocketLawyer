using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PocketLawyer.Application.Agents;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Application.UseCases;
using PocketLawyer.Domain.Dtos;
using PocketLawyer.Options;
using Microsoft.Extensions.Options;
using PocketLawyer.Application.UseCases;
using PocketLawyer.Infrastructure;

namespace PocketLawyer.Web;

[ApiController]
public class PocketController : ControllerBase
{
    private readonly ILogger<PocketController> _logger;

    private readonly IWebHostEnvironment _environment;
    private readonly PathOptions _paths;
    private readonly IWebSocketAnalysisService _socketService;
    private readonly IContractAnalyzerAgent _analyzeEmploymentAgent;
    private readonly IContractAnalyzerAgent _analyzeRentAgent;

    public PocketController(
        ILogger<PocketController> logger,
        IOptions<PathOptions> pathOptions,
        IWebHostEnvironment environment,
        IWebSocketAnalysisService socketService,
             [FromKeyedServices("employment")] IContractAnalyzerAgent analyzeEmploymentAgent,
        [FromKeyedServices("rent")] IContractAnalyzerAgent analyzeRentAgent)
    {
        _logger = logger;
        _paths = pathOptions.Value;
        _environment = environment;
        _socketService = socketService;
        _analyzeEmploymentAgent = analyzeEmploymentAgent;
        _analyzeRentAgent = analyzeRentAgent;
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

    [HttpGet("/employment-analysis")]
    public async Task<ActionResult> AnalyzeEmploymentViaWebSocket()
    {
        _logger.LogInformation("Request via {address}", Request.Host);
        if (!HttpContext.WebSockets.IsWebSocketRequest)
            return BadRequest();


        await _socketService.HandleAsync(HttpContext, _analyzeEmploymentAgent);
        return new EmptyResult();
    }

    [HttpGet("/rent-analysis")]
    public async Task<ActionResult> AnalyzeRentViaWebSocket()
    {
        _logger.LogInformation("Request via {address}", Request.Host);
        if (!HttpContext.WebSockets.IsWebSocketRequest)
            return BadRequest();


        await _socketService.HandleAsync(HttpContext, _analyzeRentAgent);
        return new EmptyResult();
    }
}

public enum AgentType
{
    Employment,
    Rent
}
