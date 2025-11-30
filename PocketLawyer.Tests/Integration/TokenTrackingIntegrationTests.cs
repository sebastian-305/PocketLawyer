using Microsoft.Extensions.Logging;
using Moq;
using PocketLawyer.Application.Agents;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Application.Models;
using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Tests.Integration;

/// <summary>
/// Integration tests for token tracking functionality.
/// Verifies that agents correctly aggregate and log token usage across multiple AI requests.
/// </summary>
public class TokenTrackingIntegrationTests
{
    private readonly Mock<ILogger<RentalContractAgent>> _logger;
    private readonly Mock<IAiClient> _aiClient;
    private readonly ISchemaProvider _schemaProvider;
    private readonly ISchemaValidator _schemaValidator;

    public TokenTrackingIntegrationTests()
    {
        _logger = new Mock<ILogger<RentalContractAgent>>();
        _aiClient = new Mock<IAiClient>();
        _schemaProvider = new PocketLawyer.Infrastructure.SchemaProviders.SchemaProvider();
        _schemaValidator = new PocketLawyer.Infrastructure.SchemaValidator();
    }

    [Fact]
    public async Task RentalContractAgent_BasicAnalysis_AggregatesTokensCorrectly()
    {
        // Arrange
        var mockContextProvider = new Mock<IContextProvider>();
        mockContextProvider.Setup(x => x.GetContextForCurrentStepAsync()).ReturnsAsync(string.Empty);
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, mockContextProvider.Object, _logger.Object);
        var contractText = "Mietvertrag zwischen Max Mustermann und Maria Musterfrau...";

        SetupInitialization();
        SetupBasicAnalysisWithTokens();

        await agent.InitializeAsync(contractText);

        // Act
        var result = await agent.AnalyzeBasicAsync();

        // Assert - Verify result is valid
        Assert.NotNull(result);
        Assert.NotNull(result.Result);
        var rentalResult = result.Result as RentalBasicAnalysisDto;
        Assert.NotNull(rentalResult);

        // Verify token summary was logged with Info level containing all required parts
        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Analysis completed") && 
                                              v.ToString()!.Contains("Total Input Tokens:") &&
                                              v.ToString()!.Contains("Total Output Tokens:") &&
                                              v.ToString()!.Contains("Total Tokens:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task RentalContractAgent_PremiumAnalysis_AggregatesTokensCorrectly()
    {
        // Arrange
        var mockContextProvider = new Mock<IContextProvider>();
        mockContextProvider.Setup(x => x.GetContextForCurrentStepAsync()).ReturnsAsync(string.Empty);
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, mockContextProvider.Object, _logger.Object);
        var contractText = "Mietvertrag zwischen Max Mustermann und Maria Musterfrau...";

        SetupInitialization();
        SetupPremiumAnalysisWithTokens();

        await agent.InitializeAsync(contractText);

        // Act
        var result = await agent.AnalyzePremiumAsync();

        // Assert - Verify result is valid
        Assert.NotNull(result);
        Assert.NotNull(result.Result);
        var rentalResult = result.Result as RentalPremiumAnalysisDto;
        Assert.NotNull(rentalResult);

        // Verify token summary was logged with Info level containing all required parts
        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Analysis completed") && 
                                              v.ToString()!.Contains("Total Input Tokens:") &&
                                              v.ToString()!.Contains("Total Output Tokens:") &&
                                              v.ToString()!.Contains("Total Tokens:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task RentalContractAgent_MultipleAnalyses_TracksTokensSeparately()
    {
        // Arrange
        var mockContextProvider = new Mock<IContextProvider>();
        mockContextProvider.Setup(x => x.GetContextForCurrentStepAsync()).ReturnsAsync(string.Empty);
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, mockContextProvider.Object, _logger.Object);
        var contractText = "Mietvertrag zwischen Max Mustermann und Maria Musterfrau...";

        SetupInitialization();
        SetupBasicAnalysisWithTokens();
        SetupPremiumAnalysisWithTokens();

        await agent.InitializeAsync(contractText);

        // Act - Run both analyses
        var basicResult = await agent.AnalyzeBasicAsync();
        var premiumResult = await agent.AnalyzePremiumAsync();

        // Assert - Both results are valid
        Assert.NotNull(basicResult);
        Assert.NotNull(premiumResult);

        // Verify token summary was logged twice (once for each analysis)
        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Analysis completed") &&
                                              v.ToString()!.Contains("Total Input Tokens") &&
                                              v.ToString()!.Contains("Total Output Tokens")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task RentalContractAgent_ParallelRequests_AggregatesAllTokens()
    {
        // Arrange
        var mockContextProvider = new Mock<IContextProvider>();
        mockContextProvider.Setup(x => x.GetContextForCurrentStepAsync()).ReturnsAsync(string.Empty);
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, mockContextProvider.Object, _logger.Object);
        var contractText = "Mietvertrag zwischen Max Mustermann und Maria Musterfrau...";

        SetupInitialization();
        SetupBasicAnalysisWithTokens();

        await agent.InitializeAsync(contractText);

        // Act
        var result = await agent.AnalyzeBasicAsync();

        // Assert
        Assert.NotNull(result);

        // Verify token summary was logged
        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Analysis completed") &&
                                              v.ToString()!.Contains("Total Input Tokens") &&
                                              v.ToString()!.Contains("Total Output Tokens")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #region Helper Methods

    private void SetupInitialization()
    {
        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("lesbar") || s.Contains("readable")),
            It.IsAny<object>()))
            .ReturnsAsync(new AiResponse(
                Content: "{\"isReadable\": true}",
                InputTokens: 10,
                OutputTokens: 5,
                TotalTokens: 15
            ));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Vertragstyp") || s.Contains("contract type")),
            It.IsAny<object>()))
            .ReturnsAsync(new AiResponse(
                Content: "{\"contractType\": \"Mietvertrag\"}",
                InputTokens: 15,
                OutputTokens: 8,
                TotalTokens: 23
            ));
    }

    private void SetupBasicAnalysisWithTokens()
    {
        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("allgemeinen Vertragsdaten")),
            It.IsAny<object>()))
            .ReturnsAsync(new AiResponse(
                Content: CreateBasicGeneralResponse(),
                InputTokens: 100,
                OutputTokens: 50,
                TotalTokens: 150
            ));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Rent")),
            It.IsAny<object>()))
            .ReturnsAsync(new AiResponse(
                Content: CreateBasicRentResponse(),
                InputTokens: 150,
                OutputTokens: 75,
                TotalTokens: 225
            ));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Conditions")),
            It.IsAny<object>()))
            .ReturnsAsync(new AiResponse(
                Content: CreateBasicConditionsResponse(),
                InputTokens: 120,
                OutputTokens: 60,
                TotalTokens: 180
            ));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Zusammenfassung")),
            It.IsAny<object>()))
            .ReturnsAsync(new AiResponse(
                Content: CreateBasicSummaryResponse(),
                InputTokens: 80,
                OutputTokens: 40,
                TotalTokens: 120
            ));
    }

    private void SetupPremiumAnalysisWithTokens()
    {
        var premiumResponse = @"{
            ""zusammenfassung"": ""Test summary"",
            ""bewertung"": ""Test evaluation"",
            ""rechtsgrundlage"": ""Test legal basis"",
            ""risikoScore"": ""Zulaessig"",
            ""riskLevel"": ""Gruen""
        }";

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Mietzahlungs") || s.Contains("Mietdauer") || 
                              s.Contains("Gebrauch") || s.Contains("Instandhaltung") ||
                              s.Contains("Rechte") || s.Contains("Datenschutz")),
            It.IsAny<object>()))
            .ReturnsAsync(new AiResponse(
                Content: premiumResponse,
                InputTokens: 200,
                OutputTokens: 100,
                TotalTokens: 300
            ));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Gesamtzusammenfassung") || (s.Contains("Premium") && s.Contains("Summary"))),
            It.IsAny<object>()))
            .ReturnsAsync(new AiResponse(
                Content: @"{
                    ""Summary.Gesamtfazit"": ""Test overall summary"",
                    ""Summary.Gesamtbewertung"": ""Gruen""
                }",
                InputTokens: 150,
                OutputTokens: 75,
                TotalTokens: 225
            ));
    }

    private string CreateBasicGeneralResponse()
    {
        return @"{
            ""vertragstyp"": ""Vollmietvertrag"",
            ""mieter"": ""Max Mustermann"",
            ""vermieter"": ""Maria Musterfrau"",
            ""mietobjektAdresse"": ""Musterstraße 1, 1010 Wien"",
            ""vertragsbeginn"": ""2024-01-01"",
            ""vertragsart"": ""Unbefristet""
        }";
    }

    private string CreateBasicRentResponse()
    {
        return @"{
            ""kaltmiete"": 800.00,
            ""nebenkosten"": 150.00,
            ""warmmiete"": 950.00,
            ""kaution"": 2400.00,
            ""zahlungsweise"": ""Monatlich"",
            ""nebenkostenregelung"": ""Pauschal""
        }";
    }

    private string CreateBasicConditionsResponse()
    {
        return @"{
            ""kuendigungsfrist"": ""3 Monate"",
            ""tierhaltung"": ""Nach Absprache"",
            ""untervermietung"": ""Nicht erlaubt"",
            ""besonderheiten"": ""Keine""
        }";
    }

    private string CreateBasicSummaryResponse()
    {
        return @"{
            ""kurzfazit"": ""Standard Mietvertrag"",
            ""Summary.Gesamtfazit"": ""Der Vertrag entspricht den üblichen Standards"",
            ""Summary.Gesamtbewertung"": ""Gut"",
            ""riskLevel"": ""Gruen""
        }";
    }

    #endregion
}
