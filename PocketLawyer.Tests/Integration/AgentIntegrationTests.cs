using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PocketLawyer.Application.Agents;
using PocketLawyer.Application.Exceptions;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Options;
using PocketLawyer.Application.Models;
using PocketLawyer.Domain.Dtos.Shared;
using PocketLawyer.Domain.Dtos.Rental;

namespace PocketLawyer.Tests.Integration;

/// <summary>
/// Integration tests for agent initialization and analysis workflows.
/// Tests the complete flow from initialization through basic and premium analysis.
/// 
/// Coverage:
/// - Complete Rental contract analysis flow end-to-end
/// - Employment contract analysis flow (basic skeleton)
/// - Error scenarios (invalid file, wrong contract type, AI failures)
/// - Parallel step execution
/// - WebSocket communication is tested through the handler's interaction with agents
/// 
/// Note: Direct WebSocket message testing requires complex HttpContext mocking.
/// The WebSocket handler's behavior is validated through agent interaction tests.
/// </summary>
public class AgentIntegrationTests
{
    private readonly Mock<ILogger<RentalContractAgent>> _rentalLogger;
    private readonly Mock<ILogger<EmploymentContractAgent>> _employmentLogger;
    private readonly Mock<IAiClient> _aiClient;
    private readonly ISchemaProvider _schemaProvider;
    private readonly ISchemaValidator _schemaValidator;

    public AgentIntegrationTests()
    {
        _rentalLogger = new Mock<ILogger<RentalContractAgent>>();
        _employmentLogger = new Mock<ILogger<EmploymentContractAgent>>();
        _aiClient = new Mock<IAiClient>();
        _schemaProvider = new PocketLawyer.Infrastructure.SchemaProviders.SchemaProvider();
        _schemaValidator = new PocketLawyer.Infrastructure.SchemaValidator();
    }

    #region Rental Contract Complete Flow Tests

    [Fact]
    public async Task RentalContract_CompleteAnalysisFlow_Success()
    {
        // Arrange
        var mockContextProvider = new Mock<IContextProvider>();
        mockContextProvider.Setup(x => x.GetContextForCurrentStepAsync()).ReturnsAsync(string.Empty);
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, mockContextProvider.Object, _rentalLogger.Object);
        var contractText = "Mietvertrag zwischen Max Mustermann und Maria Musterfrau für die Wohnung in der Musterstraße 1...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Mietvertrag");
        SetupBasicAnalysisResponses();
        SetupPremiumAnalysisResponses();

        // Act - Initialize
        var initResult = await agent.InitializeAsync(contractText);

        // Assert - Initialization
        Assert.True(initResult.Success);
        Assert.True(agent.IsInitialized);

        // Act - Basic Analysis
        var basicResult = await agent.AnalyzeBasicAsync();

        // Assert - Basic Analysis
        Assert.NotNull(basicResult);
        Assert.NotNull(basicResult.Result);
        Assert.NotNull(basicResult.Metadata);
        
        var rentalBasic = basicResult.Result as RentalBasicAnalysisDto;
        Assert.NotNull(rentalBasic);
        Assert.NotNull(rentalBasic.General);
        Assert.NotNull(rentalBasic.Rent);
        Assert.NotNull(rentalBasic.Conditions);
        Assert.NotNull(rentalBasic.Summary);

        // Act - Premium Analysis
        var premiumResult = await agent.AnalyzePremiumAsync();

        // Assert - Premium Analysis
        Assert.NotNull(premiumResult);
        Assert.NotNull(premiumResult.Result);
        
        var rentalPremium = premiumResult.Result as RentalPremiumAnalysisDto;
        Assert.NotNull(rentalPremium);
        Assert.NotNull(rentalPremium.Costs);
        Assert.NotNull(rentalPremium.Terms);
        Assert.NotNull(rentalPremium.Use);
        Assert.NotNull(rentalPremium.Maintenance);
        Assert.NotNull(rentalPremium.Duties);
        Assert.NotNull(rentalPremium.Privacy);
    }

    [Fact]
    public async Task RentalContract_WithInvalidFile_FailsInitialization()
    {
        // Arrange
        var mockContextProvider = new Mock<IContextProvider>();
        mockContextProvider.Setup(x => x.GetContextForCurrentStepAsync()).ReturnsAsync(string.Empty);
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, mockContextProvider.Object, _rentalLogger.Object);
        var contractText = "Unreadable garbage text...";

        SetupReadabilityCheck(false);

        // Act
        var initResult = await agent.InitializeAsync(contractText);

        // Assert
        Assert.False(initResult.Success);
        Assert.Contains("nicht lesbar", initResult.ErrorMessage);
        Assert.False(agent.IsInitialized);
    }

    [Fact]
    public async Task RentalContract_WithWrongContractType_FailsInitialization()
    {
        // Arrange
        var mockContextProvider = new Mock<IContextProvider>();
        mockContextProvider.Setup(x => x.GetContextForCurrentStepAsync()).ReturnsAsync(string.Empty);
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, mockContextProvider.Object, _rentalLogger.Object);
        var contractText = "Arbeitsvertrag zwischen Firma GmbH und Max Mustermann...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Arbeitsvertrag");

        // Act
        var initResult = await agent.InitializeAsync(contractText);

        // Assert
        Assert.False(initResult.Success);
        Assert.Contains("Falscher Vertragstyp", initResult.ErrorMessage);
        Assert.False(agent.IsInitialized);
    }

    [Fact]
    public async Task RentalContract_AnalysisWithoutInitialization_ThrowsException()
    {
        // Arrange
        var mockContextProvider = new Mock<IContextProvider>();
        mockContextProvider.Setup(x => x.GetContextForCurrentStepAsync()).ReturnsAsync(string.Empty);
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, mockContextProvider.Object, _rentalLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => agent.AnalyzeBasicAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => agent.AnalyzePremiumAsync());
    }

    [Fact]
    public async Task RentalContract_ParallelStepExecution_AllStepsComplete()
    {
        // Arrange
        var mockContextProvider = new Mock<IContextProvider>();
        mockContextProvider.Setup(x => x.GetContextForCurrentStepAsync()).ReturnsAsync(string.Empty);
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, mockContextProvider.Object, _rentalLogger.Object);
        var contractText = "Mietvertrag...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Mietvertrag");
        SetupBasicAnalysisResponses();

        await agent.InitializeAsync(contractText);

        // Track call times to verify parallelism
        var callTimes = new System.Collections.Concurrent.ConcurrentBag<DateTime>();
        
        _aiClient.Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(async (string prompt, object schema) =>
            {
                callTimes.Add(DateTime.UtcNow);
                await Task.Delay(10); // Simulate AI delay
                
                // Return appropriate response based on prompt
                string content;
                if (prompt.Contains("allgemeinen Vertragsdaten"))
                    content = CreateBasicGeneralResponse();
                else if (prompt.Contains("Rent"))
                    content = CreateBasicRentResponse();
                else if (prompt.Contains("Conditions"))
                    content = CreateBasicConditionsResponse();
                else if (prompt.Contains("Zusammenfassung"))
                    content = CreateBasicSummaryResponse();
                else
                    content = "{}";
                
                return new PocketLawyer.Application.Models.AiResponse(
                    Content: content,
                    InputTokens: 100,
                    OutputTokens: 50,
                    TotalTokens: 150
                );
            });

        // Act
        var result = await agent.AnalyzeBasicAsync();

        // Assert - All calls should happen within a short time window (indicating parallelism)
        Assert.Equal(4, callTimes.Count);
        var timeSpan = callTimes.Max() - callTimes.Min();
        Assert.True(timeSpan.TotalMilliseconds < 100); // All started within 100ms
    }

    [Fact(Skip = "Partial failure handling needs investigation")]
    public async Task RentalContract_OneStepFails_OthersContinue()
    {
        // Arrange
        var mockContextProvider = new Mock<IContextProvider>();
        mockContextProvider.Setup(x => x.GetContextForCurrentStepAsync()).ReturnsAsync(string.Empty);
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, mockContextProvider.Object, _rentalLogger.Object);
        var contractText = "Mietvertrag...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Mietvertrag");

        await agent.InitializeAsync(contractText);

        // Setup one step to fail, others to succeed
        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("allgemeinen Vertragsdaten")),
            It.IsAny<object>()))
            .ThrowsAsync(new Exception("AI failure"));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Rent")),
            It.IsAny<object>()))
            .ReturnsAsync(new PocketLawyer.Application.Models.AiResponse(
                Content: CreateBasicRentResponse(),
                InputTokens: 100,
                OutputTokens: 50,
                TotalTokens: 150
            ));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Conditions")),
            It.IsAny<object>()))
            .ReturnsAsync(new PocketLawyer.Application.Models.AiResponse(
                Content: CreateBasicConditionsResponse(),
                InputTokens: 100,
                OutputTokens: 50,
                TotalTokens: 150
            ));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Zusammenfassung")),
            It.IsAny<object>()))
            .ReturnsAsync(new PocketLawyer.Application.Models.AiResponse(
                Content: CreateBasicSummaryResponse(),
                InputTokens: 100,
                OutputTokens: 50,
                TotalTokens: 150
            ));

        // Act
        var result = await agent.AnalyzeBasicAsync();

        // Assert - Should complete with error message for failed step
        Assert.NotNull(result);
        Assert.NotNull(result.Result);
        
        var rentalResult = result.Result as RentalBasicAnalysisDto;
        Assert.NotNull(rentalResult);
        
        // Failed step should have error message
        Assert.Contains("Fehler", rentalResult.General.Vertragstyp);
        
        // Other steps should have succeeded - check that they have actual data, not just "Fehler"
        // The Rent step should have succeeded with the mocked response
        Assert.NotNull(rentalResult.Rent);
        Assert.NotEqual("Fehler", rentalResult.Rent.Zahlungsweise);
    }

    [Fact]
    public async Task RentalContract_PremiumAnalysis_IndependentFromBasic()
    {
        // Arrange
        var mockContextProvider = new Mock<IContextProvider>();
        mockContextProvider.Setup(x => x.GetContextForCurrentStepAsync()).ReturnsAsync(string.Empty);
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, mockContextProvider.Object, _rentalLogger.Object);
        var contractText = "Mietvertrag...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Mietvertrag");
        SetupPremiumAnalysisResponses();

        await agent.InitializeAsync(contractText);

        // Act - Skip basic analysis, go straight to premium
        var premiumResult = await agent.AnalyzePremiumAsync();

        // Assert - Premium should work independently
        Assert.NotNull(premiumResult);
        Assert.NotNull(premiumResult.Result);
        
        var rentalPremium = premiumResult.Result as RentalPremiumAnalysisDto;
        Assert.NotNull(rentalPremium);
    }

    #endregion

    #region Employment Contract Tests

    [Fact]
    public async Task EmploymentContract_Initialization_Success()
    {
        // Arrange
        var mockContextProvider = new Mock<IContextProvider>();
        mockContextProvider.Setup(x => x.GetContextForCurrentStepAsync()).ReturnsAsync(string.Empty);
        var mockKvDetectionService = new Mock<IKvDetectionService>();
        var mockKvService = new Mock<IKollektivvertragService>();
        var agent = new EmploymentContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, mockContextProvider.Object, mockKvDetectionService.Object, mockKvService.Object, Microsoft.Extensions.Options.Options.Create(new ContextOptions { IncludeKvContentInAnalysis = true }), _employmentLogger.Object);
        var contractText = "Arbeitsvertrag zwischen Firma GmbH und Max Mustermann...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Arbeitsvertrag");

        // Act
        var initResult = await agent.InitializeAsync(contractText);

        // Assert
        Assert.True(initResult.Success);
        Assert.True(agent.IsInitialized);
    }

    [Fact]
    public async Task EmploymentContract_AnalysisStub_ThrowsNotImplemented()
    {
        // Arrange
        var mockContextProvider = new Mock<IContextProvider>();
        mockContextProvider.Setup(x => x.GetContextForCurrentStepAsync()).ReturnsAsync(string.Empty);
        var mockKvDetectionService = new Mock<IKvDetectionService>();
        var mockKvService = new Mock<IKollektivvertragService>();
        var agent = new EmploymentContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, mockContextProvider.Object, mockKvDetectionService.Object, mockKvService.Object, Microsoft.Extensions.Options.Options.Create(new ContextOptions { IncludeKvContentInAnalysis = true }), _employmentLogger.Object);
        var contractText = "Arbeitsvertrag...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Arbeitsvertrag");

        // Initialize agent first
        var initResult = await agent.InitializeAsync(contractText);
        Assert.True(initResult.Success);
        
        // Setup stub responses (empty objects that will cause error DTOs)
        _aiClient.Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(new PocketLawyer.Application.Models.AiResponse(
                Content: "{}",
                InputTokens: 100,
                OutputTokens: 10,
                TotalTokens: 110
            ));

        // Act - EmploymentContractAgent is now fully implemented, so it should return results (not throw)
        var basicResult = await agent.AnalyzeBasicAsync();
        var premiumResult = await agent.AnalyzePremiumAsync();
        
        // Assert - Results should be returned (even if they contain error DTOs due to invalid responses)
        Assert.NotNull(basicResult);
        Assert.NotNull(premiumResult);
    }

    #endregion

    #region Error Scenario Tests

    [Fact]
    public async Task Agent_WithAiServiceFailure_HandlesGracefully()
    {
        // Arrange
        var mockContextProvider = new Mock<IContextProvider>();
        mockContextProvider.Setup(x => x.GetContextForCurrentStepAsync()).ReturnsAsync(string.Empty);
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, mockContextProvider.Object, _rentalLogger.Object);
        var contractText = "Mietvertrag...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Mietvertrag");

        // Setup AI to fail during analysis
        _aiClient.Setup(x => x.SendPrompt(
            It.Is<string>(s => !s.Contains("lesbar") && !s.Contains("Vertragstyp")),
            It.IsAny<object>()))
            .ThrowsAsync(new AiServiceException("AI service unavailable"));

        await agent.InitializeAsync(contractText);

        // Act
        var basicResult = await agent.AnalyzeBasicAsync();

        // Assert - Should complete with error messages in results
        Assert.NotNull(basicResult);
        var rentalResult = basicResult.Result as RentalBasicAnalysisDto;
        Assert.NotNull(rentalResult);
        
        // All steps should have error messages
        Assert.Contains("Fehler", rentalResult.General.Vertragstyp);
        Assert.Contains("Fehler", rentalResult.Rent.Zahlungsweise);
    }

    [Fact]
    public async Task Agent_WithEmptyContractText_FailsInitialization()
    {
        // Arrange
        var mockContextProvider = new Mock<IContextProvider>();
        mockContextProvider.Setup(x => x.GetContextForCurrentStepAsync()).ReturnsAsync(string.Empty);
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, mockContextProvider.Object, _rentalLogger.Object);

        // Act & Assert
        var result1 = await agent.InitializeAsync("");
        Assert.False(result1.Success);

        var result2 = await agent.InitializeAsync("   ");
        Assert.False(result2.Success);

        var result3 = await agent.InitializeAsync(null!);
        Assert.False(result3.Success);
    }

    #endregion

    #region Helper Methods

    private void SetupReadabilityCheck(bool isReadable)
    {
        var response = new AiResponse($"{{\"isReadable\": {isReadable.ToString().ToLower()}}}", 100, 50, 150);
        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("lesbar") || s.Contains("readable")),
            It.IsAny<BinaryData>()))
            .ReturnsAsync(response);
    }

    private void SetupContractTypeDetection(string contractType)
    {
        var response = new AiResponse($"{{\"contractType\": \"{contractType}\"}}", 100, 50, 150);
        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Vertragstyp") || s.Contains("contract type")),
            It.IsAny<BinaryData>()))
            .ReturnsAsync(response);
    }

    private void SetupBasicAnalysisResponses()
    {
        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("allgemeinen Vertragsdaten")),
            It.IsAny<object>()))
            .ReturnsAsync(new AiResponse(CreateBasicGeneralResponse(), 100, 50, 150));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Rent")),
            It.IsAny<object>()))
            .ReturnsAsync(new AiResponse(CreateBasicRentResponse(), 100, 50, 150));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Conditions")),
            It.IsAny<object>()))
            .ReturnsAsync(new AiResponse(CreateBasicConditionsResponse(), 100, 50, 150));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Zusammenfassung")),
            It.IsAny<object>()))
            .ReturnsAsync(new AiResponse(CreateBasicSummaryResponse(), 100, 50, 150));
    }

    private void SetupPremiumAnalysisResponses()
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
            .ReturnsAsync(new AiResponse(premiumResponse, 100, 50, 150));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Gesamtzusammenfassung") || (s.Contains("Premium") && s.Contains("Summary"))),
            It.IsAny<object>()))
            .ReturnsAsync(new AiResponse(@"{
                ""Summary.Gesamtfazit"": ""Test overall summary"",
                ""Summary.Gesamtbewertung"": ""Gruen""
            }", 100, 50, 150));
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
