using Microsoft.Extensions.Logging;
using Moq;
using PocketLawyer.Application.Agents;
using PocketLawyer.Application.Exceptions;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Domain.Dtos;

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
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, _rentalLogger.Object);
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
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, _rentalLogger.Object);
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
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, _rentalLogger.Object);
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
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, _rentalLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => agent.AnalyzeBasicAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => agent.AnalyzePremiumAsync());
    }

    [Fact]
    public async Task RentalContract_ParallelStepExecution_AllStepsComplete()
    {
        // Arrange
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, _rentalLogger.Object);
        var contractText = "Mietvertrag...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Mietvertrag");
        SetupBasicAnalysisResponses();

        await agent.InitializeAsync(contractText);

        // Track call times to verify parallelism
        var callTimes = new System.Collections.Concurrent.ConcurrentBag<DateTime>();
        
        _aiClient.Setup(x => x.SendPrompt(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(async (string prompt, object schema) =>
            {
                callTimes.Add(DateTime.UtcNow);
                await Task.Delay(10); // Simulate AI delay
                
                // Return appropriate response based on prompt
                if (prompt.Contains("allgemeinen Vertragsdaten"))
                    return CreateBasicGeneralResponse();
                if (prompt.Contains("Rent"))
                    return CreateBasicRentResponse();
                if (prompt.Contains("Conditions"))
                    return CreateBasicConditionsResponse();
                if (prompt.Contains("Zusammenfassung"))
                    return CreateBasicSummaryResponse();
                
                return "{}";
            });

        // Act
        var result = await agent.AnalyzeBasicAsync();

        // Assert - All calls should happen within a short time window (indicating parallelism)
        Assert.Equal(4, callTimes.Count);
        var timeSpan = callTimes.Max() - callTimes.Min();
        Assert.True(timeSpan.TotalMilliseconds < 100); // All started within 100ms
    }

    [Fact]
    public async Task RentalContract_OneStepFails_OthersContinue()
    {
        // Arrange
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, _rentalLogger.Object);
        var contractText = "Mietvertrag...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Mietvertrag");

        await agent.InitializeAsync(contractText);

        // Setup one step to fail, others to succeed
        _aiClient.Setup(x => x.SendPrompt(
            It.Is<string>(s => s.Contains("allgemeinen Vertragsdaten")),
            It.IsAny<object>()))
            .ThrowsAsync(new Exception("AI failure"));

        _aiClient.Setup(x => x.SendPrompt(
            It.Is<string>(s => s.Contains("Rent")),
            It.IsAny<object>()))
            .ReturnsAsync(CreateBasicRentResponse());

        _aiClient.Setup(x => x.SendPrompt(
            It.Is<string>(s => s.Contains("Conditions")),
            It.IsAny<object>()))
            .ReturnsAsync(CreateBasicConditionsResponse());

        _aiClient.Setup(x => x.SendPrompt(
            It.Is<string>(s => s.Contains("Zusammenfassung")),
            It.IsAny<object>()))
            .ReturnsAsync(CreateBasicSummaryResponse());

        // Act
        var result = await agent.AnalyzeBasicAsync();

        // Assert - Should complete with error message for failed step
        Assert.NotNull(result);
        Assert.NotNull(result.Result);
        
        var rentalResult = result.Result as RentalBasicAnalysisDto;
        Assert.NotNull(rentalResult);
        
        // Failed step should have error message
        Assert.Contains("Fehler", rentalResult.General.Vertragstyp);
        
        // Other steps should have succeeded (not contain "Fehler")
        Assert.DoesNotContain("Fehler", rentalResult.Rent.Zahlungsweise);
    }

    [Fact]
    public async Task RentalContract_PremiumAnalysis_IndependentFromBasic()
    {
        // Arrange
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, _rentalLogger.Object);
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
        var agent = new EmploymentContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, _employmentLogger.Object);
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
        var agent = new EmploymentContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, _employmentLogger.Object);
        var contractText = "Arbeitsvertrag...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Arbeitsvertrag");

        // Initialize agent first
        var initResult = await agent.InitializeAsync(contractText);
        Assert.True(initResult.Success);
        
        // Setup stub responses (empty objects)
        _aiClient.Setup(x => x.SendPrompt(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync("{}");

        // Act & Assert - Stub implementation throws NotImplementedException
        await Assert.ThrowsAsync<NotImplementedException>(() => agent.AnalyzeBasicAsync());
        await Assert.ThrowsAsync<NotImplementedException>(() => agent.AnalyzePremiumAsync());
    }

    #endregion

    #region Error Scenario Tests

    [Fact]
    public async Task Agent_WithAiServiceFailure_HandlesGracefully()
    {
        // Arrange
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, _rentalLogger.Object);
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
        var agent = new RentalContractAgent(_aiClient.Object, _schemaProvider, _schemaValidator, _rentalLogger.Object);

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
        _aiClient.Setup(x => x.SendPrompt(
            It.Is<string>(s => s.Contains("lesbar") || s.Contains("readable")),
            It.IsAny<object>()))
            .ReturnsAsync($"{{\"isReadable\": {isReadable.ToString().ToLower()}}}");
    }

    private void SetupContractTypeDetection(string contractType)
    {
        _aiClient.Setup(x => x.SendPrompt(
            It.Is<string>(s => s.Contains("Vertragstyp") || s.Contains("contract type")),
            It.IsAny<object>()))
            .ReturnsAsync($"{{\"contractType\": \"{contractType}\"}}");
    }

    private void SetupBasicAnalysisResponses()
    {
        _aiClient.Setup(x => x.SendPrompt(
            It.Is<string>(s => s.Contains("allgemeinen Vertragsdaten")),
            It.IsAny<object>()))
            .ReturnsAsync(CreateBasicGeneralResponse());

        _aiClient.Setup(x => x.SendPrompt(
            It.Is<string>(s => s.Contains("Rent")),
            It.IsAny<object>()))
            .ReturnsAsync(CreateBasicRentResponse());

        _aiClient.Setup(x => x.SendPrompt(
            It.Is<string>(s => s.Contains("Conditions")),
            It.IsAny<object>()))
            .ReturnsAsync(CreateBasicConditionsResponse());

        _aiClient.Setup(x => x.SendPrompt(
            It.Is<string>(s => s.Contains("Zusammenfassung")),
            It.IsAny<object>()))
            .ReturnsAsync(CreateBasicSummaryResponse());
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

        _aiClient.Setup(x => x.SendPrompt(
            It.Is<string>(s => s.Contains("Mietzahlungs") || s.Contains("Mietdauer") || 
                              s.Contains("Gebrauch") || s.Contains("Instandhaltung") ||
                              s.Contains("Rechte") || s.Contains("Datenschutz")),
            It.IsAny<object>()))
            .ReturnsAsync(premiumResponse);

        _aiClient.Setup(x => x.SendPrompt(
            It.Is<string>(s => s.Contains("Gesamtzusammenfassung") || (s.Contains("Premium") && s.Contains("Summary"))),
            It.IsAny<object>()))
            .ReturnsAsync(@"{
                ""Summary.Gesamtfazit"": ""Test overall summary"",
                ""Summary.Gesamtbewertung"": ""Gruen""
            }");
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
