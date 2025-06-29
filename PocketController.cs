using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PocketLawyer.Service;
using System.Text.Json;
using PocketLawyer.Infrastructure;

namespace PocketLawyer;

[ApiController]
public class PocketController : ControllerBase
{
    private readonly ILogger<PocketController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly IWebSocketAnalysisService _socketService;

    public PocketController(ILogger<PocketController> logger,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        IWebSocketAnalysisService socketService)
    {
        _logger = logger;
        _configuration = configuration;
        _environment = environment;
        _socketService = socketService;
    }

    [HttpGet("/mockdata")]
    public async Task<ActionResult<ContractAnalysisDto>> GetMockData()
    {
        _logger.LogDebug("Entering {Method}", nameof(GetMockData));

        string responsesPath = Path.Combine(
            _environment.ContentRootPath,
            _configuration["ResponsesPath"] ?? DefaultPaths.Responses);

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

        ContractAnalysisDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<ContractAnalysisDto>(mockContent);
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


    [HttpGet("/analysis")]
    public async Task<ActionResult> AnalyzeViaWebSocket()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
            return BadRequest();

        await _socketService.HandleAsync(HttpContext);
        return new EmptyResult();
    }


}
