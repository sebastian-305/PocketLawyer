using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PocketLawyer.Application.Agents;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Options;
using PocketLawyer.Application.Models;
using PocketLawyer.Domain.Dtos;
using System.Reflection;
using Xunit;

namespace PocketLawyer.Tests.Application.Agents;

/// <summary>
/// Unit tests for Premium Analysis steps in EmploymentContractAgent.
/// Tests KV context inclusion and error handling for each step.
/// Requirements: 7.3, 9.5, 11.2
/// </summary>
public class EmploymentContractAgentPremiumStepsTests
{
    private static Mock<IContextProvider> CreateMockContextProvider(string context = "")
    {
        var mock = new Mock<IContextProvider>();
        mock.Setup(x => x.GetContextForCurrentStepAsync()).ReturnsAsync(context);
        return mock;
    }

    private static EmploymentContractAgent CreateInitializedAgent(
        IAiClient aiClient,
        ISchemaProvider schemaProvider,
        ISchemaValidator schemaValidator,
        IContextProvider contextProvider,
        string contractText = "ARBEITSVERTRAG...",
        string? kvName = null,
        string? kvText = null)
    {
        var mockLogger = new Mock<ILogger<EmploymentContractAgent>>();
        var mockKvDetectionService = new Mock<IKvDetectionService>();
        var mockKvService = new Mock<IKollektivvertragService>();

        if (kvName != null)
        {
            mockKvDetectionService
                .Setup(x => x.DetectAsync(It.IsAny<string>()))
                .ReturnsAsync(new KvDetectionResult { KvName = kvName, AllEqual = true });
            
            if (kvText != null)
            {
                mockKvService
                    .Setup(x => x.GetKvFileContentsAsync(kvName))
                    .ReturnsAsync(new[] { kvText });
            }
        }
        else
        {
            mockKvDetectionService
                .Setup(x => x.DetectAsync(It.IsAny<string>()))
                .ReturnsAsync(new KvDetectionResult { KvName = string.Empty, AllEqual = false });
        }

        var agent = new EmploymentContractAgent(
            aiClient,
            schemaProvider,
            schemaValidator,
            contextProvider,
            mockKvDetectionService.Object,
            mockKvService.Object,
            Microsoft.Extensions.Options.Options.Create(new ContextOptions { IncludeKvContentInAnalysis = true }), mockLogger.Object);

        // Use reflection to set the agent as initialized without calling InitializeAsync
        // This avoids the initialization AI calls interfering with our test mocks
        var contractTextField = typeof(EmploymentContractAgent).BaseType!
            .GetField("_contractText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        contractTextField!.SetValue(agent, contractText);

        var isInitializedField = typeof(EmploymentContractAgent).BaseType!
            .GetField("_isInitialized", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        isInitializedField!.SetValue(agent, true);

        // Set KV fields if provided
        if (kvName != null)
        {
            var kvNameField = typeof(EmploymentContractAgent)
                .GetField("_detectedKvName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            kvNameField!.SetValue(agent, kvName);

            if (kvText != null)
            {
                var kvTextField = typeof(EmploymentContractAgent)
                    .GetField("_detectedKvText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                kvTextField!.SetValue(agent, kvText);
            }
        }

        return agent;
    }

    private static async Task<T> InvokePrivateMethodAsync<T>(object obj, string methodName)
    {
        var method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (method == null)
        {
            throw new InvalidOperationException($"Method {methodName} not found");
        }

        var task = (Task<T>)method.Invoke(obj, null)!;
        return await task;
    }

    #region AnalyzeCompensationBenefitsClausesAsync Tests

    [Fact]
    public async Task AnalyzeCompensationBenefitsClausesAsync_WithKvContext_CompletesSuccessfully()
    {
        // Arrange
        var kvText = "Kollektivvertrag Handel - Mindestgehalt: 2000 EUR";

        var mockAiClient = new Mock<IAiClient>();
        mockAiClient
            .Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
            .ReturnsAsync((string prompt, object? schema) =>
            {
                // Verify KV context is in the prompt
                Assert.Contains("Kollektivvertrag Context", prompt);
                Assert.Contains(kvText, prompt);
                
                return new AiResponse(
                    Content: System.Text.Json.JsonSerializer.Serialize(new
                    {
                        zusammenfassung = "Test",
                        bewertung = "Test",
                        rechtsgrundlage = "Test",
                        risikoScore = "Neutral",
                        riskLevel = "Grün"
                    }),
                    InputTokens: 500,
                    OutputTokens: 200,
                    TotalTokens: 700
                );
            });

        var mockSchemaProvider = new Mock<ISchemaProvider>();
        mockSchemaProvider
            .Setup(x => x.GetSchema(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(BinaryData.FromString("{}"));
        mockSchemaProvider
            .Setup(x => x.GetSchemaForEndpoint(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new System.Text.Json.Nodes.JsonObject());

        var mockSchemaValidator = new Mock<ISchemaValidator>();
        mockSchemaValidator
            .Setup(x => x.Validate(It.IsAny<System.Text.Json.Nodes.JsonObject>(), It.IsAny<System.Text.Json.Nodes.JsonObject>()))
            .Returns(true);

        var mockContextProvider = CreateMockContextProvider("Reference context");

        var agent = CreateInitializedAgent(
            mockAiClient.Object,
            mockSchemaProvider.Object,
            mockSchemaValidator.Object,
            mockContextProvider.Object,
            kvName: "kollektivvertrag_handel",
            kvText: kvText
        );

        // Act
        var result = await InvokePrivateMethodAsync<PremiumSectionDto>(agent, "AnalyzeCompensationBenefitsClausesAsync");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Zusammenfassung);
        Assert.Equal("Neutral", result.RisikoScore);
        mockAiClient.Verify(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()), Times.Once);
    }

    [Fact]
    public async Task AnalyzeCompensationBenefitsClausesAsync_HandlesErrorGracefully()
    {
        // Arrange
        var mockAiClient = new Mock<IAiClient>();
        mockAiClient
            .Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
            .ThrowsAsync(new Exception("AI service unavailable"));

        var mockSchemaProvider = new Mock<ISchemaProvider>();
        mockSchemaProvider
            .Setup(x => x.GetSchema(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(BinaryData.FromString("{}"));
        mockSchemaProvider
            .Setup(x => x.GetSchemaForEndpoint(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new System.Text.Json.Nodes.JsonObject());

        var mockSchemaValidator = new Mock<ISchemaValidator>();
        var mockContextProvider = CreateMockContextProvider();

        var agent = CreateInitializedAgent(
            mockAiClient.Object,
            mockSchemaProvider.Object,
            mockSchemaValidator.Object,
            mockContextProvider.Object
        );

        // Act
        var result = await InvokePrivateMethodAsync<PremiumSectionDto>(agent, "AnalyzeCompensationBenefitsClausesAsync");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Fehler", result.Zusammenfassung);
        Assert.Contains("Vergütungs- und Zusatzleistungsklauseln", result.Zusammenfassung);
        Assert.Equal("Nachteilhaft", result.RisikoScore);
        Assert.Equal("Gelb", result.RiskLevel);
    }

    #endregion

    #region AnalyzeWorkingTimeVacationAsync Tests

    [Fact]
    public async Task AnalyzeWorkingTimeVacationAsync_WithKvContext_CompletesSuccessfully()
    {
        // Arrange
        var kvText = "Kollektivvertrag Handel - Urlaubsanspruch: 30 Tage";

        var mockAiClient = new Mock<IAiClient>();
        mockAiClient
            .Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
            .ReturnsAsync((string prompt, object? schema) =>
            {
                // Verify KV context is in the prompt
                Assert.Contains("Kollektivvertrag Context", prompt);
                Assert.Contains(kvText, prompt);
                
                return new AiResponse(
                    Content: System.Text.Json.JsonSerializer.Serialize(new
                    {
                        zusammenfassung = "Test",
                        bewertung = "Test",
                        rechtsgrundlage = "Test",
                        risikoScore = "Neutral",
                        riskLevel = "Grün"
                    }),
                    InputTokens: 500,
                    OutputTokens: 200,
                    TotalTokens: 700
                );
            });

        var mockSchemaProvider = new Mock<ISchemaProvider>();
        mockSchemaProvider
            .Setup(x => x.GetSchema(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(BinaryData.FromString("{}"));
        mockSchemaProvider
            .Setup(x => x.GetSchemaForEndpoint(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new System.Text.Json.Nodes.JsonObject());

        var mockSchemaValidator = new Mock<ISchemaValidator>();
        mockSchemaValidator
            .Setup(x => x.Validate(It.IsAny<System.Text.Json.Nodes.JsonObject>(), It.IsAny<System.Text.Json.Nodes.JsonObject>()))
            .Returns(true);

        var mockContextProvider = CreateMockContextProvider("Reference context");

        var agent = CreateInitializedAgent(
            mockAiClient.Object,
            mockSchemaProvider.Object,
            mockSchemaValidator.Object,
            mockContextProvider.Object,
            kvName: "kollektivvertrag_handel",
            kvText: kvText
        );

        // Act
        var result = await InvokePrivateMethodAsync<PremiumSectionDto>(agent, "AnalyzeWorkingTimeVacationAsync");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Zusammenfassung);
        mockAiClient.Verify(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()), Times.Once);
    }

    [Fact]
    public async Task AnalyzeWorkingTimeVacationAsync_HandlesErrorGracefully()
    {
        // Arrange
        var mockAiClient = new Mock<IAiClient>();
        mockAiClient
            .Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
            .ThrowsAsync(new Exception("AI service unavailable"));

        var mockSchemaProvider = new Mock<ISchemaProvider>();
        mockSchemaProvider
            .Setup(x => x.GetSchema(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(BinaryData.FromString("{}"));
        mockSchemaProvider
            .Setup(x => x.GetSchemaForEndpoint(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new System.Text.Json.Nodes.JsonObject());

        var mockSchemaValidator = new Mock<ISchemaValidator>();
        var mockContextProvider = CreateMockContextProvider();

        var agent = CreateInitializedAgent(
            mockAiClient.Object,
            mockSchemaProvider.Object,
            mockSchemaValidator.Object,
            mockContextProvider.Object
        );

        // Act
        var result = await InvokePrivateMethodAsync<PremiumSectionDto>(agent, "AnalyzeWorkingTimeVacationAsync");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Fehler", result.Zusammenfassung);
        Assert.Contains("Arbeitszeit- und Urlaubsklauseln", result.Zusammenfassung);
        Assert.Equal("Nachteilhaft", result.RisikoScore);
        Assert.Equal("Gelb", result.RiskLevel);
    }

    #endregion

    #region AnalyzeTerminationNoticeAsync Tests

    [Fact]
    public async Task AnalyzeTerminationNoticeAsync_WithKvContext_CompletesSuccessfully()
    {
        // Arrange
        var kvText = "Kollektivvertrag Handel - Kündigungsfrist: 6 Wochen";

        var mockAiClient = new Mock<IAiClient>();
        mockAiClient
            .Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
            .ReturnsAsync((string prompt, object? schema) =>
            {
                // Verify KV context is in the prompt
                Assert.Contains("Kollektivvertrag Context", prompt);
                Assert.Contains(kvText, prompt);
                
                return new AiResponse(
                    Content: System.Text.Json.JsonSerializer.Serialize(new
                    {
                        zusammenfassung = "Test",
                        bewertung = "Test",
                        rechtsgrundlage = "Test",
                        risikoScore = "Neutral",
                        riskLevel = "Grün"
                    }),
                    InputTokens: 500,
                    OutputTokens: 200,
                    TotalTokens: 700
                );
            });

        var mockSchemaProvider = new Mock<ISchemaProvider>();
        mockSchemaProvider
            .Setup(x => x.GetSchema(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(BinaryData.FromString("{}"));
        mockSchemaProvider
            .Setup(x => x.GetSchemaForEndpoint(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new System.Text.Json.Nodes.JsonObject());

        var mockSchemaValidator = new Mock<ISchemaValidator>();
        mockSchemaValidator
            .Setup(x => x.Validate(It.IsAny<System.Text.Json.Nodes.JsonObject>(), It.IsAny<System.Text.Json.Nodes.JsonObject>()))
            .Returns(true);

        var mockContextProvider = CreateMockContextProvider("Reference context");

        var agent = CreateInitializedAgent(
            mockAiClient.Object,
            mockSchemaProvider.Object,
            mockSchemaValidator.Object,
            mockContextProvider.Object,
            kvName: "kollektivvertrag_handel",
            kvText: kvText
        );

        // Act
        var result = await InvokePrivateMethodAsync<PremiumSectionDto>(agent, "AnalyzeTerminationNoticeAsync");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Zusammenfassung);
        mockAiClient.Verify(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()), Times.Once);
    }

    [Fact]
    public async Task AnalyzeTerminationNoticeAsync_HandlesErrorGracefully()
    {
        // Arrange
        var mockAiClient = new Mock<IAiClient>();
        mockAiClient
            .Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
            .ThrowsAsync(new Exception("AI service unavailable"));

        var mockSchemaProvider = new Mock<ISchemaProvider>();
        mockSchemaProvider
            .Setup(x => x.GetSchema(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(BinaryData.FromString("{}"));
        mockSchemaProvider
            .Setup(x => x.GetSchemaForEndpoint(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new System.Text.Json.Nodes.JsonObject());

        var mockSchemaValidator = new Mock<ISchemaValidator>();
        var mockContextProvider = CreateMockContextProvider();

        var agent = CreateInitializedAgent(
            mockAiClient.Object,
            mockSchemaProvider.Object,
            mockSchemaValidator.Object,
            mockContextProvider.Object
        );

        // Act
        var result = await InvokePrivateMethodAsync<PremiumSectionDto>(agent, "AnalyzeTerminationNoticeAsync");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Fehler", result.Zusammenfassung);
        Assert.Contains("Kündigungs- und Kündigungsfristklauseln", result.Zusammenfassung);
        Assert.Equal("Nachteilhaft", result.RisikoScore);
        Assert.Equal("Gelb", result.RiskLevel);
    }

    #endregion

    #region AnalyzeNonCompeteConfidentialityAsync Tests

    [Fact]
    public async Task AnalyzeNonCompeteConfidentialityAsync_WithKvContext_CompletesSuccessfully()
    {
        // Arrange
        var kvText = "Kollektivvertrag Handel - Wettbewerbsverbot: 6 Monate";

        var mockAiClient = new Mock<IAiClient>();
        mockAiClient
            .Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
            .ReturnsAsync((string prompt, object? schema) =>
            {
                // Verify KV context is in the prompt
                Assert.Contains("Kollektivvertrag Context", prompt);
                Assert.Contains(kvText, prompt);
                
                return new AiResponse(
                    Content: System.Text.Json.JsonSerializer.Serialize(new
                    {
                        zusammenfassung = "Test",
                        bewertung = "Test",
                        rechtsgrundlage = "Test",
                        risikoScore = "Neutral",
                        riskLevel = "Grün"
                    }),
                    InputTokens: 500,
                    OutputTokens: 200,
                    TotalTokens: 700
                );
            });

        var mockSchemaProvider = new Mock<ISchemaProvider>();
        mockSchemaProvider
            .Setup(x => x.GetSchema(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(BinaryData.FromString("{}"));
        mockSchemaProvider
            .Setup(x => x.GetSchemaForEndpoint(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new System.Text.Json.Nodes.JsonObject());

        var mockSchemaValidator = new Mock<ISchemaValidator>();
        mockSchemaValidator
            .Setup(x => x.Validate(It.IsAny<System.Text.Json.Nodes.JsonObject>(), It.IsAny<System.Text.Json.Nodes.JsonObject>()))
            .Returns(true);

        var mockContextProvider = CreateMockContextProvider("Reference context");

        var agent = CreateInitializedAgent(
            mockAiClient.Object,
            mockSchemaProvider.Object,
            mockSchemaValidator.Object,
            mockContextProvider.Object,
            kvName: "kollektivvertrag_handel",
            kvText: kvText
        );

        // Act
        var result = await InvokePrivateMethodAsync<PremiumSectionDto>(agent, "AnalyzeNonCompeteConfidentialityAsync");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Zusammenfassung);
        mockAiClient.Verify(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()), Times.Once);
    }

    [Fact]
    public async Task AnalyzeNonCompeteConfidentialityAsync_HandlesErrorGracefully()
    {
        // Arrange
        var mockAiClient = new Mock<IAiClient>();
        mockAiClient
            .Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
            .ThrowsAsync(new Exception("AI service unavailable"));

        var mockSchemaProvider = new Mock<ISchemaProvider>();
        mockSchemaProvider
            .Setup(x => x.GetSchema(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(BinaryData.FromString("{}"));
        mockSchemaProvider
            .Setup(x => x.GetSchemaForEndpoint(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new System.Text.Json.Nodes.JsonObject());

        var mockSchemaValidator = new Mock<ISchemaValidator>();
        var mockContextProvider = CreateMockContextProvider();

        var agent = CreateInitializedAgent(
            mockAiClient.Object,
            mockSchemaProvider.Object,
            mockSchemaValidator.Object,
            mockContextProvider.Object
        );

        // Act
        var result = await InvokePrivateMethodAsync<PremiumSectionDto>(agent, "AnalyzeNonCompeteConfidentialityAsync");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Fehler", result.Zusammenfassung);
        Assert.Contains("Wettbewerbs- und Vertraulichkeitsklauseln", result.Zusammenfassung);
        Assert.Equal("Nachteilhaft", result.RisikoScore);
        Assert.Equal("Gelb", result.RiskLevel);
    }

    #endregion

    #region AnalyzeRightsDutiesAsync Tests

    [Fact]
    public async Task AnalyzeRightsDutiesAsync_WithKvContext_CompletesSuccessfully()
    {
        // Arrange
        var kvText = "Kollektivvertrag Handel - Rechte und Pflichten";

        var mockAiClient = new Mock<IAiClient>();
        mockAiClient
            .Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
            .ReturnsAsync((string prompt, object? schema) =>
            {
                // Verify KV context is in the prompt
                Assert.Contains("Kollektivvertrag Context", prompt);
                Assert.Contains(kvText, prompt);
                
                return new AiResponse(
                    Content: System.Text.Json.JsonSerializer.Serialize(new
                    {
                        zusammenfassung = "Test",
                        bewertung = "Test",
                        rechtsgrundlage = "Test",
                        risikoScore = "Neutral",
                        riskLevel = "Grün"
                    }),
                    InputTokens: 500,
                    OutputTokens: 200,
                    TotalTokens: 700
                );
            });

        var mockSchemaProvider = new Mock<ISchemaProvider>();
        mockSchemaProvider
            .Setup(x => x.GetSchema(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(BinaryData.FromString("{}"));
        mockSchemaProvider
            .Setup(x => x.GetSchemaForEndpoint(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new System.Text.Json.Nodes.JsonObject());

        var mockSchemaValidator = new Mock<ISchemaValidator>();
        mockSchemaValidator
            .Setup(x => x.Validate(It.IsAny<System.Text.Json.Nodes.JsonObject>(), It.IsAny<System.Text.Json.Nodes.JsonObject>()))
            .Returns(true);

        var mockContextProvider = CreateMockContextProvider("Reference context");

        var agent = CreateInitializedAgent(
            mockAiClient.Object,
            mockSchemaProvider.Object,
            mockSchemaValidator.Object,
            mockContextProvider.Object,
            kvName: "kollektivvertrag_handel",
            kvText: kvText
        );

        // Act
        var result = await InvokePrivateMethodAsync<PremiumSectionDto>(agent, "AnalyzeRightsDutiesAsync");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Zusammenfassung);
        mockAiClient.Verify(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()), Times.Once);
    }

    [Fact]
    public async Task AnalyzeRightsDutiesAsync_HandlesErrorGracefully()
    {
        // Arrange
        var mockAiClient = new Mock<IAiClient>();
        mockAiClient
            .Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
            .ThrowsAsync(new Exception("AI service unavailable"));

        var mockSchemaProvider = new Mock<ISchemaProvider>();
        mockSchemaProvider
            .Setup(x => x.GetSchema(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(BinaryData.FromString("{}"));
        mockSchemaProvider
            .Setup(x => x.GetSchemaForEndpoint(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new System.Text.Json.Nodes.JsonObject());

        var mockSchemaValidator = new Mock<ISchemaValidator>();
        var mockContextProvider = CreateMockContextProvider();

        var agent = CreateInitializedAgent(
            mockAiClient.Object,
            mockSchemaProvider.Object,
            mockSchemaValidator.Object,
            mockContextProvider.Object
        );

        // Act
        var result = await InvokePrivateMethodAsync<PremiumSectionDto>(agent, "AnalyzeRightsDutiesAsync");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Fehler", result.Zusammenfassung);
        Assert.Contains("Rechte- und Pflichtenklauseln", result.Zusammenfassung);
        Assert.Equal("Nachteilhaft", result.RisikoScore);
        Assert.Equal("Gelb", result.RiskLevel);
    }

    #endregion

    #region AnalyzePremiumSummaryAsync Tests

    [Fact]
    public async Task AnalyzePremiumSummaryAsync_WithKvContext_CompletesSuccessfully()
    {
        // Arrange
        var kvText = "Kollektivvertrag Handel - Gesamtübersicht";

        var mockAiClient = new Mock<IAiClient>();
        mockAiClient
            .Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
            .ReturnsAsync((string prompt, object? schema) =>
            {
                // Verify KV context is in the prompt
                Assert.Contains("Kollektivvertrag Context", prompt);
                Assert.Contains(kvText, prompt);
                
                return new AiResponse(
                    Content: System.Text.Json.JsonSerializer.Serialize(new
                    {
                        gesamtfazit = "Gesamtfazit Test",
                        gesamtbewertung = "Gesamtbewertung Test",
                        handlungsempfehlungen = new[] { "Empfehlung 1", "Empfehlung 2" }
                    }),
                    InputTokens: 500,
                    OutputTokens: 200,
                    TotalTokens: 700
                );
            });

        var mockSchemaProvider = new Mock<ISchemaProvider>();
        mockSchemaProvider
            .Setup(x => x.GetSchema(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(BinaryData.FromString("{}"));
        mockSchemaProvider
            .Setup(x => x.GetSchemaForEndpoint(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new System.Text.Json.Nodes.JsonObject());

        var mockSchemaValidator = new Mock<ISchemaValidator>();
        mockSchemaValidator
            .Setup(x => x.Validate(It.IsAny<System.Text.Json.Nodes.JsonObject>(), It.IsAny<System.Text.Json.Nodes.JsonObject>()))
            .Returns(true);

        var mockContextProvider = CreateMockContextProvider("Reference context");

        var agent = CreateInitializedAgent(
            mockAiClient.Object,
            mockSchemaProvider.Object,
            mockSchemaValidator.Object,
            mockContextProvider.Object,
            kvName: "kollektivvertrag_handel",
            kvText: kvText
        );

        // Act
        var result = await InvokePrivateMethodAsync<(string Gesamtfazit, string Gesamtbewertung)>(agent, "AnalyzePremiumSummaryAsync");

        // Assert
        Assert.Equal("Gesamtfazit Test", result.Gesamtfazit);
        Assert.Equal("Gesamtbewertung Test", result.Gesamtbewertung);
        mockAiClient.Verify(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()), Times.Once);
    }

    [Fact]
    public async Task AnalyzePremiumSummaryAsync_HandlesErrorGracefully()
    {
        // Arrange
        var mockAiClient = new Mock<IAiClient>();
        mockAiClient
            .Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
            .ThrowsAsync(new Exception("AI service unavailable"));

        var mockSchemaProvider = new Mock<ISchemaProvider>();
        mockSchemaProvider
            .Setup(x => x.GetSchema(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(BinaryData.FromString("{}"));
        mockSchemaProvider
            .Setup(x => x.GetSchemaForEndpoint(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new System.Text.Json.Nodes.JsonObject());

        var mockSchemaValidator = new Mock<ISchemaValidator>();
        var mockContextProvider = CreateMockContextProvider();

        var agent = CreateInitializedAgent(
            mockAiClient.Object,
            mockSchemaProvider.Object,
            mockSchemaValidator.Object,
            mockContextProvider.Object
        );

        // Act
        var result = await InvokePrivateMethodAsync<(string Gesamtfazit, string Gesamtbewertung)>(agent, "AnalyzePremiumSummaryAsync");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Fehler", result.Gesamtfazit);
        Assert.Contains("Premium-Zusammenfassung", result.Gesamtfazit);
        Assert.Contains("konnte nicht erstellt werden", result.Gesamtbewertung);
    }

    #endregion

    #region Context Provider Tests

    [Fact]
    public async Task AllPremiumSteps_CallContextProvider()
    {
        // Arrange
        var mockContextProvider = new Mock<IContextProvider>();
        mockContextProvider
            .Setup(x => x.GetContextForCurrentStepAsync())
            .ReturnsAsync("Test context");

        var mockAiClient = new Mock<IAiClient>();
        mockAiClient
            .Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
            .ReturnsAsync(new AiResponse(
                Content: System.Text.Json.JsonSerializer.Serialize(new
                {
                    zusammenfassung = "Test",
                    bewertung = "Test",
                    rechtsgrundlage = "Test",
                    risikoScore = "Neutral",
                    riskLevel = "Grün"
                }),
                InputTokens: 500,
                OutputTokens: 200,
                TotalTokens: 700
            ));

        var mockSchemaProvider = new Mock<ISchemaProvider>();
        mockSchemaProvider
            .Setup(x => x.GetSchema(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(BinaryData.FromString("{}"));
        mockSchemaProvider
            .Setup(x => x.GetSchemaForEndpoint(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new System.Text.Json.Nodes.JsonObject());

        var mockSchemaValidator = new Mock<ISchemaValidator>();
        mockSchemaValidator
            .Setup(x => x.Validate(It.IsAny<System.Text.Json.Nodes.JsonObject>(), It.IsAny<System.Text.Json.Nodes.JsonObject>()))
            .Returns(true);

        var agent = CreateInitializedAgent(
            mockAiClient.Object,
            mockSchemaProvider.Object,
            mockSchemaValidator.Object,
            mockContextProvider.Object
        );

        // Act - Call all premium steps
        await InvokePrivateMethodAsync<PremiumSectionDto>(agent, "AnalyzeCompensationBenefitsClausesAsync");
        await InvokePrivateMethodAsync<PremiumSectionDto>(agent, "AnalyzeWorkingTimeVacationAsync");
        await InvokePrivateMethodAsync<PremiumSectionDto>(agent, "AnalyzeTerminationNoticeAsync");
        await InvokePrivateMethodAsync<PremiumSectionDto>(agent, "AnalyzeNonCompeteConfidentialityAsync");
        await InvokePrivateMethodAsync<PremiumSectionDto>(agent, "AnalyzeRightsDutiesAsync");

        // Assert - Context provider should be called for each step
        mockContextProvider.Verify(x => x.GetContextForCurrentStepAsync(), Times.Exactly(5));
    }

    #endregion
}
