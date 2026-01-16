using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PocketLawyer.Application.Agents;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Options;
using PocketLawyer.Application.Models;
using PocketLawyer.Domain.Dtos;
using Xunit;
using System;

namespace PocketLawyer.Tests.Integration;

/// <summary>
/// Integration tests for EmploymentContractAgent.
/// Tests complete end-to-end flows for Basic and Premium analysis with real schemas.
/// 
/// Coverage:
/// - Complete Basic Analysis flow with real schemas
/// - Complete Premium Analysis flow with real schemas
/// - KV detection integration
/// - Error scenarios and resilience
/// - Parallel step execution
/// 
/// Requirements: 2.4, 3.2, 3.3, 8.2, 8.3
/// </summary>
public class EmploymentContractAgentIntegrationTests
{
    private readonly Mock<ILogger<EmploymentContractAgent>> _logger;
    private readonly Mock<IAiClient> _aiClient;
    private readonly ISchemaProvider _schemaProvider;
    private readonly ISchemaValidator _schemaValidator;
    private readonly Mock<IContextProvider> _contextProvider;
    private readonly Mock<IKvDetectionService> _kvDetectionService;
    private readonly Mock<IKollektivvertragService> _kvService;

    public EmploymentContractAgentIntegrationTests()
    {
        _logger = new Mock<ILogger<EmploymentContractAgent>>();
        _aiClient = new Mock<IAiClient>();
        _schemaProvider = new PocketLawyer.Infrastructure.SchemaProviders.SchemaProvider();
        _schemaValidator = new PocketLawyer.Infrastructure.SchemaValidator();
        _contextProvider = new Mock<IContextProvider>();
        _kvDetectionService = new Mock<IKvDetectionService>();
        _kvService = new Mock<IKollektivvertragService>();
        
        // Default setup for context provider
        _contextProvider.Setup(x => x.GetContextForCurrentStepAsync()).ReturnsAsync(string.Empty);
    }

    #region Complete Basic Analysis Flow Tests

    /// <summary>
    /// Tests complete Basic Analysis flow with real schemas.
    /// Validates: Requirements 2.4, 3.2, 3.3
    /// </summary>
    [Fact]
    public async Task BasicAnalysis_CompleteFlow_WithRealSchemas_Success()
    {
        // Arrange
        var agent = CreateAgent();
        var contractText = "ARBEITSVERTRAG zwischen Test GmbH und Max Mustermann für die Position Software Developer...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Arbeitsvertrag");
        SetupKvDetection(null); // No KV detected
        SetupBasicAnalysisResponses();

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
        
        var employmentBasic = basicResult.Result as EmploymentBasicAnalysisDto;
        Assert.NotNull(employmentBasic);
        Assert.NotNull(employmentBasic.General);
        Assert.NotNull(employmentBasic.Compensation);
        Assert.NotNull(employmentBasic.WorkingConditions);
        Assert.NotNull(employmentBasic.Summary);

        // Verify metadata
        Assert.Equal("AT", basicResult.Metadata.CountryCode);
        Assert.Equal("de", basicResult.Metadata.Language);
        Assert.False(basicResult.Metadata.IsPreview);
    }

    /// <summary>
    /// Tests Basic Analysis with all fields populated correctly.
    /// Validates: Requirements 3.2, 3.3
    /// </summary>
    [Fact]
    public async Task BasicAnalysis_AllFieldsPopulated_Success()
    {
        // Arrange
        var agent = CreateAgent();
        var contractText = "ARBEITSVERTRAG zwischen Test GmbH und Max Mustermann...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Arbeitsvertrag");
        SetupKvDetection(null);
        SetupBasicAnalysisResponses();

        await agent.InitializeAsync(contractText);

        // Act
        var result = await agent.AnalyzeBasicAsync();

        // Assert
        var analysis = (EmploymentBasicAnalysisDto)result.Result;
        
        // Verify General data
        Assert.Equal("Test GmbH", analysis.General.Arbeitgeber);
        Assert.Equal("Max Mustermann", analysis.General.Arbeitnehmer);
        Assert.Equal("Software Developer", analysis.General.Position);
        Assert.Equal("01.01.2024", analysis.General.Startdatum);
        Assert.Equal("unbefristet", analysis.General.Vertragsart);
        Assert.Equal("3 Monate", analysis.General.Probezeit);

        // Verify Compensation data
        Assert.Equal("4000 EUR", analysis.Compensation.GehaltBrutto);
        Assert.Equal("2800 EUR", analysis.Compensation.GehaltNettoCa);
        Assert.Equal("monatlich", analysis.Compensation.Zahlungsweise);
        Assert.Equal("13. und 14. Gehalt", analysis.Compensation.Sonderzahlungen);
        Assert.Equal("Überstundenzuschläge", analysis.Compensation.Zuschlaege);
        Assert.Equal(2, analysis.Compensation.Zusatzleistungen.Length);

        // Verify Working Conditions data
        Assert.Equal("40 Stunden", analysis.WorkingConditions.Wochenarbeitszeit);
        Assert.Equal("Vollzeit", analysis.WorkingConditions.Arbeitszeitmodell);
        Assert.Equal("Abgeltung durch Zeitausgleich", analysis.WorkingConditions.Ueberstundenregelung);
        Assert.Equal("25 Tage", analysis.WorkingConditions.Urlaubstage);
        Assert.Equal("Wien", analysis.WorkingConditions.Arbeitsort);
        Assert.Equal("2 Tage pro Woche", analysis.WorkingConditions.RemoteArbeit);

        // Verify Summary data
        Assert.Equal("Guter Vertrag", analysis.Summary.Kurzfazit);
        Assert.Equal("Kein Kollektivvertrag erkannt", analysis.Summary.Kollektivvertrag);
        Assert.Equal("Positiv", analysis.Summary.Gesamtbewertung);
        Assert.Equal("Grün", analysis.Summary.RiskLevel);
    }

    #endregion

    #region Complete Premium Analysis Flow Tests

    /// <summary>
    /// Tests complete Premium Analysis flow with real schemas.
    /// Validates: Requirements 8.2, 8.3
    /// </summary>
    [Fact]
    public async Task PremiumAnalysis_CompleteFlow_WithRealSchemas_Success()
    {
        // Arrange
        var agent = CreateAgent();
        var contractText = "ARBEITSVERTRAG zwischen Test GmbH und Max Mustermann...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Arbeitsvertrag");
        SetupKvDetection(null);
        SetupPremiumAnalysisResponses();

        // Act - Initialize
        var initResult = await agent.InitializeAsync(contractText);

        // Assert - Initialization
        Assert.True(initResult.Success);
        Assert.True(agent.IsInitialized);

        // Act - Premium Analysis
        var premiumResult = await agent.AnalyzePremiumAsync();

        // Assert - Premium Analysis
        Assert.NotNull(premiumResult);
        Assert.NotNull(premiumResult.Result);
        Assert.NotNull(premiumResult.Metadata);
        
        var employmentPremium = premiumResult.Result as EmploymentPremiumAnalysisDto;
        Assert.NotNull(employmentPremium);
        Assert.NotNull(employmentPremium.CompensationBenefits);
        Assert.NotNull(employmentPremium.WorkingTimeVacation);
        Assert.NotNull(employmentPremium.TerminationNotice);
        Assert.NotNull(employmentPremium.NonCompeteConfidentiality);
        Assert.NotNull(employmentPremium.RightsDuties);
        Assert.NotNull(employmentPremium.Summary);

        // Verify metadata
        Assert.Equal("AT", premiumResult.Metadata.CountryCode);
        Assert.Equal("de", premiumResult.Metadata.Language);
        Assert.False(premiumResult.Metadata.IsPreview);
    }

    /// <summary>
    /// Tests Premium Analysis with all sections populated correctly.
    /// Validates: Requirements 8.2, 8.3
    /// </summary>
    [Fact]
    public async Task PremiumAnalysis_AllSectionsPopulated_Success()
    {
        // Arrange
        var agent = CreateAgent();
        var contractText = "ARBEITSVERTRAG zwischen Test GmbH und Max Mustermann...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Arbeitsvertrag");
        SetupKvDetection(null);
        SetupPremiumAnalysisResponses();

        await agent.InitializeAsync(contractText);

        // Act
        var result = await agent.AnalyzePremiumAsync();

        // Assert
        var analysis = (EmploymentPremiumAnalysisDto)result.Result;
        
        // Verify all premium sections exist and have content
        Assert.NotNull(analysis.CompensationBenefits);
        Assert.Equal("Vergütungsklauseln Zusammenfassung", analysis.CompensationBenefits.Zusammenfassung);
        Assert.Equal("Neutral", analysis.CompensationBenefits.RisikoScore);
        Assert.Equal("Grün", analysis.CompensationBenefits.RiskLevel);

        Assert.NotNull(analysis.WorkingTimeVacation);
        Assert.Equal("Arbeitszeitklauseln Zusammenfassung", analysis.WorkingTimeVacation.Zusammenfassung);
        
        Assert.NotNull(analysis.TerminationNotice);
        Assert.Equal("Kündigungsklauseln Zusammenfassung", analysis.TerminationNotice.Zusammenfassung);
        
        Assert.NotNull(analysis.NonCompeteConfidentiality);
        Assert.Equal("Wettbewerbsklauseln Zusammenfassung", analysis.NonCompeteConfidentiality.Zusammenfassung);
        
        Assert.NotNull(analysis.RightsDuties);
        Assert.Equal("Rechte und Pflichten Zusammenfassung", analysis.RightsDuties.Zusammenfassung);

        // Verify Summary
        Assert.NotNull(analysis.Summary);
        Assert.Equal("Gesamtfazit Test", analysis.Summary.Gesamtfazit);
        Assert.Equal("Gesamtbewertung Test", analysis.Summary.Gesamtbewertung);
    }

    #endregion

    #region KV Detection Integration Tests

    /// <summary>
    /// Tests KV detection integration during initialization.
    /// Validates: Requirements 2.4
    /// </summary>
    [Fact]
    public async Task KvDetection_WhenKvDetected_StoresKvNameAndText()
    {
        // Arrange
        var agent = CreateAgent();
        var contractText = "ARBEITSVERTRAG zwischen Test GmbH und Max Mustermann...";
        var kvName = "kollektivvertrag_handel";
        var kvText = "Kollektivvertrag für Angestellte im Handel...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Arbeitsvertrag");
        SetupKvDetection(kvName, kvText);

        // Act
        var initResult = await agent.InitializeAsync(contractText);

        // Assert
        Assert.True(initResult.Success);
        Assert.True(agent.IsInitialized);
        
        // Verify KV detection was called
        _kvDetectionService.Verify(x => x.DetectAsync(contractText), Times.Once);
        _kvService.Verify(x => x.GetKvFileContentsAsync(kvName), Times.Once);
    }

    /// <summary>
    /// Tests KV detection when no KV is found.
    /// Validates: Requirements 2.4
    /// </summary>
    [Fact]
    public async Task KvDetection_WhenNoKvDetected_SucceedsWithoutKv()
    {
        // Arrange
        var agent = CreateAgent();
        var contractText = "ARBEITSVERTRAG zwischen Test GmbH und Max Mustermann...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Arbeitsvertrag");
        SetupKvDetection(null); // No KV detected

        // Act
        var initResult = await agent.InitializeAsync(contractText);

        // Assert
        Assert.True(initResult.Success);
        Assert.True(agent.IsInitialized);
        
        // Verify KV detection was called but no KV was loaded
        _kvDetectionService.Verify(x => x.DetectAsync(contractText), Times.Once);
        _kvService.Verify(x => x.GetKvFileContentsAsync(It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Tests that KV text is included in Premium Analysis prompts.
    /// Validates: Requirements 2.4, 8.2
    /// </summary>
    [Fact]
    public async Task PremiumAnalysis_WithKvDetected_IncludesKvInPrompts()
    {
        // Arrange
        var agent = CreateAgent();
        var contractText = "ARBEITSVERTRAG zwischen Test GmbH und Max Mustermann...";
        var kvName = "kollektivvertrag_handel";
        var kvText = "Kollektivvertrag für Angestellte im Handel...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Arbeitsvertrag");
        SetupKvDetection(kvName, kvText);
        SetupPremiumAnalysisResponses();

        await agent.InitializeAsync(contractText);

        // Track prompts sent to AI
        var promptsSent = new List<string>();
        _aiClient.Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
            .Callback<string, object?>((prompt, schema) => promptsSent.Add(prompt))
            .ReturnsAsync((string prompt, object? schema) =>
            {
                // Return appropriate response based on prompt content
                if (prompt.Contains("Vergütungs- und Zusatzleistungsklauseln") || prompt.Contains("Einhaltung des Kollektivvertrags") ||
                    prompt.Contains("Arbeitszeit- und Urlaubsklauseln") || prompt.Contains("Arbeitszeitregelungen") ||
                    prompt.Contains("Kündigungs- und Kündigungsfristklauseln") || prompt.Contains("Kündigungsfristen") ||
                    prompt.Contains("Wettbewerbs- und Vertraulichkeitsklauseln") || prompt.Contains("Konkurrenzklauseln") ||
                    prompt.Contains("Rechte- und Pflichtenklauseln") || prompt.Contains("Rechte und Pflichten"))
                {
                    return new AiResponse(CreatePremiumSectionResponse(), 800, 300, 1100);
                }
                if (prompt.Contains("Premium-Zusammenfassung") || (prompt.Contains("Gesamtzusammenfassung") && prompt.Contains("Premium")))
                {
                    return new AiResponse(CreatePremiumSummaryResponse(), 800, 300, 1100);
                }
                return new AiResponse("{}", 100, 10, 110);
            });

        // Act
        await agent.AnalyzePremiumAsync();

        // Assert - Verify KV text was included in premium analysis prompts
        var premiumPrompts = promptsSent.Where(p => 
            p.Contains("Vergütungs-") || 
            p.Contains("Arbeitszeit-") || 
            p.Contains("Kündigungs-") ||
            p.Contains("Wettbewerbs-") ||
            p.Contains("Rechte-")).ToList();
        
        Assert.NotEmpty(premiumPrompts);
        // Each premium prompt should contain the KV text
        foreach (var prompt in premiumPrompts)
        {
            Assert.Contains(kvText, prompt);
        }
    }

    /// <summary>
    /// Tests that KV detection failure doesn't prevent initialization.
    /// Validates: Requirements 2.4
    /// </summary>
    [Fact]
    public async Task KvDetection_WhenDetectionFails_ContinuesWithoutKv()
    {
        // Arrange
        var agent = CreateAgent();
        var contractText = "ARBEITSVERTRAG zwischen Test GmbH und Max Mustermann...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Arbeitsvertrag");
        
        // Setup KV detection to throw exception
        _kvDetectionService
            .Setup(x => x.DetectAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("KV detection service unavailable"));

        // Act
        var initResult = await agent.InitializeAsync(contractText);

        // Assert - Should succeed despite KV detection failure
        Assert.True(initResult.Success);
        Assert.True(agent.IsInitialized);
        
        // Verify KV detection was attempted
        _kvDetectionService.Verify(x => x.DetectAsync(contractText), Times.Once);
        // But KV service should not have been called
        _kvService.Verify(x => x.GetKvFileContentsAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Error Scenario Tests

    /// <summary>
    /// Tests that analysis continues when one step fails.
    /// Validates: Requirements 3.3
    /// </summary>
    [Fact]
    public async Task BasicAnalysis_WhenOneStepFails_OtherStepsContinue()
    {
        // Arrange
        var agent = CreateAgent();
        var contractText = "ARBEITSVERTRAG zwischen Test GmbH und Max Mustermann...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Arbeitsvertrag");
        SetupKvDetection(null);

        await agent.InitializeAsync(contractText);

        // Setup one step to fail, others to succeed
        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("allgemeinen Vertragsdaten") || s.Contains("Arbeitgeber")),
            It.IsAny<object?>()))
            .ThrowsAsync(new Exception("AI service unavailable"));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Vergütungsregelungen") || s.Contains("Gehalt Brutto")),
            It.IsAny<object?>()))
            .ReturnsAsync(new AiResponse(CreateBasicCompensationResponse(), 500, 200, 700));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Arbeitsbedingungen") || s.Contains("Wochenarbeitszeit")),
            It.IsAny<object?>()))
            .ReturnsAsync(new AiResponse(CreateBasicWorkingConditionsResponse(), 500, 200, 700));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Zusammenfassung") && s.Contains("Kurzfazit")),
            It.IsAny<object?>()))
            .ReturnsAsync(new AiResponse(CreateBasicSummaryResponse(), 500, 200, 700));

        // Act
        var result = await agent.AnalyzeBasicAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Result);
        
        var analysis = (EmploymentBasicAnalysisDto)result.Result;
        
        // Failed step should have error message
        Assert.Contains("Fehler", analysis.General.Arbeitgeber);
        
        // Other steps should have succeeded
        Assert.Equal("4000 EUR", analysis.Compensation.GehaltBrutto);
        Assert.Equal("40 Stunden", analysis.WorkingConditions.Wochenarbeitszeit);
        Assert.Equal("Guter Vertrag", analysis.Summary.Kurzfazit);
    }

    /// <summary>
    /// Tests that Premium Analysis continues when one step fails.
    /// Validates: Requirements 8.3
    /// </summary>
    [Fact]
    public async Task PremiumAnalysis_WhenOneStepFails_OtherStepsContinue()
    {
        // Arrange
        var agent = CreateAgent();
        var contractText = "ARBEITSVERTRAG zwischen Test GmbH und Max Mustermann...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Arbeitsvertrag");
        SetupKvDetection(null);

        await agent.InitializeAsync(contractText);

        // Setup one step to fail, others to succeed
        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Vergütungs- und Zusatzleistungsklauseln") || s.Contains("Einhaltung des Kollektivvertrags")),
            It.IsAny<object?>()))
            .ThrowsAsync(new Exception("AI service unavailable"));

        // Setup other steps to succeed
        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Arbeitszeit- und Urlaubsklauseln") || s.Contains("Arbeitszeitregelungen")),
            It.IsAny<object?>()))
            .ReturnsAsync(new AiResponse(CreatePremiumSectionResponse(), 800, 300, 1100));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Kündigungs- und Kündigungsfristklauseln") || s.Contains("Kündigungsfristen")),
            It.IsAny<object?>()))
            .ReturnsAsync(new AiResponse(CreatePremiumSectionResponse(), 800, 300, 1100));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Wettbewerbs- und Vertraulichkeitsklauseln") || s.Contains("Konkurrenzklauseln")),
            It.IsAny<object?>()))
            .ReturnsAsync(new AiResponse(CreatePremiumSectionResponse(), 800, 300, 1100));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Rechte- und Pflichtenklauseln") || s.Contains("Rechte und Pflichten")),
            It.IsAny<object?>()))
            .ReturnsAsync(new AiResponse(CreatePremiumSectionResponse(), 800, 300, 1100));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Premium-Zusammenfassung") || (s.Contains("Gesamtzusammenfassung") && s.Contains("Premium"))),
            It.IsAny<object?>()))
            .ReturnsAsync(new AiResponse(CreatePremiumSummaryResponse(), 800, 300, 1100));

        // Act
        var result = await agent.AnalyzePremiumAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Result);
        
        var analysis = (EmploymentPremiumAnalysisDto)result.Result;
        
        // Failed step should have error message
        Assert.Contains("Fehler", analysis.CompensationBenefits.Zusammenfassung);
        
        // Other steps should have succeeded
        Assert.DoesNotContain("Fehler", analysis.WorkingTimeVacation.Zusammenfassung);
        Assert.DoesNotContain("Fehler", analysis.TerminationNotice.Zusammenfassung);
        Assert.DoesNotContain("Fehler", analysis.NonCompeteConfidentiality.Zusammenfassung);
        Assert.DoesNotContain("Fehler", analysis.RightsDuties.Zusammenfassung);
    }

    /// <summary>
    /// Tests parallel execution of Basic Analysis steps.
    /// Validates: Requirements 3.2
    /// </summary>
    [Fact]
    public async Task BasicAnalysis_ParallelStepExecution_AllStepsComplete()
    {
        // Arrange
        var agent = CreateAgent();
        var contractText = "ARBEITSVERTRAG zwischen Test GmbH und Max Mustermann...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Arbeitsvertrag");
        SetupKvDetection(null);

        await agent.InitializeAsync(contractText);

        // Track call times to verify parallelism
        var callTimes = new System.Collections.Concurrent.ConcurrentBag<DateTime>();
        
        _aiClient.Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
            .Returns(async (string prompt, object? schema) =>
            {
                callTimes.Add(DateTime.UtcNow);
                await Task.Delay(10); // Simulate AI delay
                
                // Return appropriate response based on prompt
                if (prompt.Contains("allgemeinen Vertragsdaten") || prompt.Contains("Arbeitgeber"))
                    return new AiResponse(CreateBasicGeneralResponse(), 500, 200, 700);
                else if (prompt.Contains("Vergütungsregelungen") || prompt.Contains("Gehalt Brutto"))
                    return new AiResponse(CreateBasicCompensationResponse(), 500, 200, 700);
                else if (prompt.Contains("Arbeitsbedingungen") || prompt.Contains("Wochenarbeitszeit"))
                    return new AiResponse(CreateBasicWorkingConditionsResponse(), 500, 200, 700);
                else if (prompt.Contains("Zusammenfassung") && prompt.Contains("Kurzfazit"))
                    return new AiResponse(CreateBasicSummaryResponse(), 500, 200, 700);
                else
                    return new AiResponse("{}", 100, 10, 110);
            });

        // Act
        var result = await agent.AnalyzeBasicAsync();

        // Assert - All calls should happen within a short time window (indicating parallelism)
        Assert.Equal(4, callTimes.Count);
        var timeSpan = callTimes.Max() - callTimes.Min();
        Assert.True(timeSpan.TotalMilliseconds < 100); // All started within 100ms
        
        // Verify result is complete
        Assert.NotNull(result);
        var analysis = (EmploymentBasicAnalysisDto)result.Result;
        Assert.NotNull(analysis.General);
        Assert.NotNull(analysis.Compensation);
        Assert.NotNull(analysis.WorkingConditions);
        Assert.NotNull(analysis.Summary);
    }

    /// <summary>
    /// Tests parallel execution of Premium Analysis steps.
    /// Validates: Requirements 8.2
    /// </summary>
    [Fact]
    public async Task PremiumAnalysis_ParallelStepExecution_AllStepsComplete()
    {
        // Arrange
        var agent = CreateAgent();
        var contractText = "ARBEITSVERTRAG zwischen Test GmbH und Max Mustermann...";

        SetupReadabilityCheck(true);
        SetupContractTypeDetection("Arbeitsvertrag");
        SetupKvDetection(null);

        await agent.InitializeAsync(contractText);

        // Track call times to verify parallelism
        var callTimes = new System.Collections.Concurrent.ConcurrentBag<DateTime>();
        
        _aiClient.Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
            .Returns(async (string prompt, object? schema) =>
            {
                callTimes.Add(DateTime.UtcNow);
                await Task.Delay(10); // Simulate AI delay
                
                // Return appropriate response based on prompt
                if (prompt.Contains("Vergütungs- und Zusatzleistungsklauseln") || prompt.Contains("Einhaltung des Kollektivvertrags") ||
                    prompt.Contains("Arbeitszeit- und Urlaubsklauseln") || prompt.Contains("Arbeitszeitregelungen") ||
                    prompt.Contains("Kündigungs- und Kündigungsfristklauseln") || prompt.Contains("Kündigungsfristen") ||
                    prompt.Contains("Wettbewerbs- und Vertraulichkeitsklauseln") || prompt.Contains("Konkurrenzklauseln") ||
                    prompt.Contains("Rechte- und Pflichtenklauseln") || prompt.Contains("Rechte und Pflichten"))
                    return new AiResponse(CreatePremiumSectionResponse(), 800, 300, 1100);
                else if (prompt.Contains("Premium-Zusammenfassung") || (prompt.Contains("Gesamtzusammenfassung") && prompt.Contains("Premium")))
                    return new AiResponse(CreatePremiumSummaryResponse(), 800, 300, 1100);
                else
                    return new AiResponse("{}", 100, 10, 110);
            });

        // Act
        var result = await agent.AnalyzePremiumAsync();

        // Assert - All calls should happen within a short time window (indicating parallelism)
        Assert.Equal(6, callTimes.Count); // 5 sections + 1 summary
        var timeSpan = callTimes.Max() - callTimes.Min();
        Assert.True(timeSpan.TotalMilliseconds < 100); // All started within 100ms
        
        // Verify result is complete
        Assert.NotNull(result);
        var analysis = (EmploymentPremiumAnalysisDto)result.Result;
        Assert.NotNull(analysis.CompensationBenefits);
        Assert.NotNull(analysis.WorkingTimeVacation);
        Assert.NotNull(analysis.TerminationNotice);
        Assert.NotNull(analysis.NonCompeteConfidentiality);
        Assert.NotNull(analysis.RightsDuties);
        Assert.NotNull(analysis.Summary);
    }

    #endregion

    #region Helper Methods

    private EmploymentContractAgent CreateAgent()
    {
        return new EmploymentContractAgent(
            _aiClient.Object,
            _schemaProvider,
            _schemaValidator,
            _contextProvider.Object,
            _kvDetectionService.Object,
            _kvService.Object,
            Microsoft.Extensions.Options.Options.Create(new ContextOptions { IncludeKvContentInAnalysis = true }), _logger.Object);
    }

    private void SetupReadabilityCheck(bool isReadable)
    {
        var response = new AiResponse(
            $"{{\"isReadable\": {isReadable.ToString().ToLower()}}}", 
            100, 50, 150);
        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("lesbar") || s.Contains("Prüfe")),
            It.IsAny<BinaryData>()))
            .ReturnsAsync(response);
    }

    private void SetupContractTypeDetection(string contractType)
    {
        var response = new AiResponse(
            $"{{\"contractType\": \"{contractType}\"}}", 
            100, 50, 150);
        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Vertragstyp") || s.Contains("Erkenne die Art")),
            It.IsAny<BinaryData>()))
            .ReturnsAsync(response);
    }

    private void SetupKvDetection(string? kvName, string? kvText = null)
    {
        if (string.IsNullOrEmpty(kvName))
        {
            _kvDetectionService
                .Setup(x => x.DetectAsync(It.IsAny<string>()))
                .ReturnsAsync(new KvDetectionResult { KvName = string.Empty, AllEqual = false });
        }
        else
        {
            _kvDetectionService
                .Setup(x => x.DetectAsync(It.IsAny<string>()))
                .ReturnsAsync(new KvDetectionResult { KvName = kvName, AllEqual = true });
            
            if (!string.IsNullOrEmpty(kvText))
            {
                _kvService
                    .Setup(x => x.GetKvFileContentsAsync(kvName))
                    .ReturnsAsync(new[] { kvText });
            }
        }
    }

    private void SetupBasicAnalysisResponses()
    {
        // Setup separate handlers for each analysis step
        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Arbeitgeber") || s.Contains("allgemeinen Vertragsdaten")),
            It.IsAny<BinaryData>()))
            .ReturnsAsync(new AiResponse(CreateBasicGeneralResponse(), 500, 200, 700));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Vergütungsregelungen") || s.Contains("Gehalt Brutto")),
            It.IsAny<BinaryData>()))
            .ReturnsAsync(new AiResponse(CreateBasicCompensationResponse(), 500, 200, 700));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Arbeitsbedingungen") || s.Contains("Wochenarbeitszeit")),
            It.IsAny<BinaryData>()))
            .ReturnsAsync(new AiResponse(CreateBasicWorkingConditionsResponse(), 500, 200, 700));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Zusammenfassung") && s.Contains("Kurzfazit")),
            It.IsAny<BinaryData>()))
            .ReturnsAsync(new AiResponse(CreateBasicSummaryResponse(), 500, 200, 700));
    }

    private void SetupPremiumAnalysisResponses()
    {
        // Setup separate handlers for each premium analysis step
        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Vergütungs- und Zusatzleistungsklauseln") || s.Contains("Einhaltung des Kollektivvertrags")),
            It.IsAny<BinaryData>()))
            .ReturnsAsync(new AiResponse(CreatePremiumSectionResponse("Vergütungsklauseln"), 800, 300, 1100));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Arbeitszeit- und Urlaubsklauseln") || s.Contains("Arbeitszeitregelungen")),
            It.IsAny<BinaryData>()))
            .ReturnsAsync(new AiResponse(CreatePremiumSectionResponse("Arbeitszeitklauseln"), 800, 300, 1100));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Kündigungs- und Kündigungsfristklauseln") || s.Contains("Kündigungsfristen")),
            It.IsAny<BinaryData>()))
            .ReturnsAsync(new AiResponse(CreatePremiumSectionResponse("Kündigungsklauseln"), 800, 300, 1100));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Wettbewerbs- und Vertraulichkeitsklauseln") || s.Contains("Konkurrenzklauseln")),
            It.IsAny<BinaryData>()))
            .ReturnsAsync(new AiResponse(CreatePremiumSectionResponse("Wettbewerbsklauseln"), 800, 300, 1100));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Rechte- und Pflichtenklauseln") || s.Contains("Rechte und Pflichten")),
            It.IsAny<BinaryData>()))
            .ReturnsAsync(new AiResponse(CreatePremiumSectionResponse("Rechte und Pflichten"), 800, 300, 1100));

        _aiClient.Setup(x => x.SendPromptWithMetadata(
            It.Is<string>(s => s.Contains("Premium-Zusammenfassung") || (s.Contains("Gesamtzusammenfassung") && s.Contains("Premium"))),
            It.IsAny<BinaryData>()))
            .ReturnsAsync(new AiResponse(CreatePremiumSummaryResponse(), 800, 300, 1100));
    }

    private string CreateBasicGeneralResponse()
    {
        return @"{
            ""arbeitgeber"": ""Test GmbH"",
            ""arbeitnehmer"": ""Max Mustermann"",
            ""position"": ""Software Developer"",
            ""startdatum"": ""01.01.2024"",
            ""vertragsart"": ""unbefristet"",
            ""probezeit"": ""3 Monate""
        }";
    }

    private string CreateBasicCompensationResponse()
    {
        return @"{
            ""gehaltBrutto"": ""4000 EUR"",
            ""gehaltNettoCa"": ""2800 EUR"",
            ""zahlungsweise"": ""monatlich"",
            ""sonderzahlungen"": ""13. und 14. Gehalt"",
            ""zuschlaege"": ""Überstundenzuschläge"",
            ""zusatzleistungen"": [""Firmenwagen"", ""Laptop""]
        }";
    }

    private string CreateBasicWorkingConditionsResponse()
    {
        return @"{
            ""wochenarbeitszeit"": ""40 Stunden"",
            ""arbeitszeitmodell"": ""Vollzeit"",
            ""ueberstundenregelung"": ""Abgeltung durch Zeitausgleich"",
            ""urlaubstage"": ""25 Tage"",
            ""arbeitsort"": ""Wien"",
            ""remoteArbeit"": ""2 Tage pro Woche""
        }";
    }

    private string CreateBasicSummaryResponse()
    {
        return @"{
            ""kurzfazit"": ""Guter Vertrag"",
            ""kollektivvertrag"": ""Kein Kollektivvertrag erkannt"",
            ""gesamtbewertung"": ""Positiv"",
            ""riskLevel"": ""Grün""
        }";
    }

    private string CreatePremiumSectionResponse(string? prefix = null)
    {
        var summaryText = prefix != null ? $"{prefix} Zusammenfassung" : "Test Zusammenfassung";
        return $@"{{
            ""zusammenfassung"": ""{summaryText}"",
            ""bewertung"": ""Test Bewertung"",
            ""rechtsgrundlage"": ""Test Rechtsgrundlage"",
            ""risikoScore"": ""Neutral"",
            ""riskLevel"": ""Grün""
        }}";
    }

    private string CreatePremiumSummaryResponse()
    {
        return @"{
            ""gesamtfazit"": ""Gesamtfazit Test"",
            ""gesamtbewertung"": ""Gesamtbewertung Test"",
            ""handlungsempfehlungen"": [""Empfehlung 1"", ""Empfehlung 2""]
        }";
    }

    #endregion
}
