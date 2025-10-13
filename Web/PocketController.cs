using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PocketLawyer.Application.Interfaces;
using System.Text.Json;
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
    private readonly IContractAnalyzerService _analyzeContractUseCase;
    public PocketController(
        ILogger<PocketController> logger,
        IOptions<PathOptions> pathOptions,
        IWebHostEnvironment environment,
        IWebSocketAnalysisService socketService,
        IContractAnalyzerService analyzeContractUseCase)
    {
        _logger = logger;
        _paths = pathOptions.Value;
        _environment = environment;
        _socketService = socketService;
        _analyzeContractUseCase = analyzeContractUseCase;
    }

    [HttpGet("/mockdata")]
    public async Task<ActionResult<ContractAnalysisDto>> GetMockData()
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
        _logger.LogInformation("Request via {address}", Request.Host);
        if (!HttpContext.WebSockets.IsWebSocketRequest)
            return BadRequest();


        await _socketService.HandleAsync(HttpContext);
        return new EmptyResult();
    }

    [HttpPost("/lease")]
    public async Task<ActionResult> AnalyseLease()
    {
        await _analyzeContractUseCase.ProcessContractRequest<FileContentDto>(new FileContentDto("Test.txt", "Das ist ein Test"), PromptBuilder.BuildAnalysisPrompt, null);


        return new EmptyResult();
    }


}
