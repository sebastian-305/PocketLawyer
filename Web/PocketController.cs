using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PocketLawyer.Application.Interfaces;
using System.Text.Json;
using PocketLawyer.Domain.Dtos;
using PocketLawyer.Options;
using Microsoft.Extensions.Options;

namespace PocketLawyer.Web;

[ApiController]
public class PocketController : ControllerBase
{
    private readonly ILogger<PocketController> _logger;

    private readonly IWebHostEnvironment _environment;
    private readonly PathOptions _paths;
    private readonly IWebSocketAnalysisService _socketService;

    public PocketController(
        ILogger<PocketController> logger,
        IOptions<PathOptions> pathOptions,
        IWebHostEnvironment environment,
        IWebSocketAnalysisService socketService)
    {
        _logger = logger;
        _paths = pathOptions.Value;
        _environment = environment;
        _socketService = socketService;
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


        await _socketService.HandleAsync(HttpContext, AgentType.Employment);
        return new EmptyResult();
    }

    [HttpGet("/rent-analysis")]
    public async Task<ActionResult> AnalyzeRentViaWebSocket()
    {
        _logger.LogInformation("Request via {address}", Request.Host);
        if (!HttpContext.WebSockets.IsWebSocketRequest)
            return BadRequest();


        await _socketService.HandleAsync(HttpContext, AgentType.Rent);
        return new EmptyResult();
    }
}

public enum AgentType
{
    Employment,
    Rent
}
