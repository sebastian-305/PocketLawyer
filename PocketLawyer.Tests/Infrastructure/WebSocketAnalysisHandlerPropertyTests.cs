using FsCheck;
using FsCheck.Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using PocketLawyer.Application.Agents;
using PocketLawyer.Application.Exceptions;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Domain.Dtos;
using PocketLawyer.Infrastructure;
using PocketLawyer.Tests.TestDoubles;
using System.Text;
using System.Text.Json;
using Xunit;

namespace PocketLawyer.Tests.Infrastructure;

/// <summary>
/// Property-based tests for WebSocketAnalysisHandler.
/// Feature: agent-refactoring
/// </summary>
public class WebSocketAnalysisHandlerPropertyTests
{
    /// <summary>
    /// **Feature: agent-refactoring, Property 1: WebSocket receives contract with type**
    /// For any valid WebSocket connection and file upload, when a contract is sent with a contract type,
    /// the system should successfully receive both the file data and the contract type.
    /// **Validates: Requirements 1.1, 1.2**
    /// 
    /// Note: This property test focuses on file reception with Rental contracts only,
    /// since Employment agent is a skeleton. The full integration test is covered by
    /// HandleAsync_WithValidFile_ReceivesFileAndContractType.
    /// </summary>
    [Property(MaxTest = 50, Skip = "Needs migration to SendPromptWithMetadata - see TEST_FAILURES_ANALYSIS.md")]
    public Property WebSocketReceivesContractWithType()
    {
        // Generate valid file data
        var fileDataGen = Gen.Choose(10, 1000)
            .Select(length => Encoding.UTF8.GetBytes(new string('A', length)));

        // Generate valid file names
        var fileNameGen = Gen.Elements("contract.pdf", "document.txt", "agreement.pdf", "test.docx");

        // Generate valid contract types - only Rental since Employment is a skeleton
        var contractTypeGen = Gen.Constant("Rental");

        return Prop.ForAll(
            fileDataGen.ToArbitrary(),
            fileNameGen.ToArbitrary(),
            contractTypeGen.ToArbitrary(),
            (fileData, fileName, contractType) =>
            {
                // Arrange
                var mockLogger = new Mock<ILogger<WebSocketAnalysisHandler>>();
                var mockFileValidator = new Mock<IFileValidatorService>();

                // Setup file validator to return valid content
                mockFileValidator
                    .Setup(x => x.ValidateAndDecodeAsync(It.IsAny<IFormFile>()))
                    .ReturnsAsync(new FileContentDto("test.pdf", "Valid contract text"));

                // Create real agent with mocked dependencies
                var mockAiClientForAgent = new Mock<IAiClient>();
                var mockSchemaProviderForAgent = new Mock<ISchemaProvider>();
                var mockSchemaValidatorForAgent = new Mock<ISchemaValidator>();
                
                // Setup AI client for initialization
                mockAiClientForAgent
                    .Setup(x => x.SendPrompt(
                        It.Is<string>(p => p.Contains("lesbar", StringComparison.OrdinalIgnoreCase)),
                        It.IsAny<object>()))
                    .ReturnsAsync(JsonSerializer.Serialize(new { isReadable = true }));
                
                // Setup AI client for contract type detection - return appropriate type
                var expectedContractTypeName = contractType == "Rental" ? "Mietvertrag" : "Arbeitsvertrag";
                mockAiClientForAgent
                    .Setup(x => x.SendPrompt(
                        It.Is<string>(p => p.Contains("Erkenne die Art", StringComparison.OrdinalIgnoreCase)),
                        It.IsAny<object>()))
                    .ReturnsAsync(JsonSerializer.Serialize(new { contractType = expectedContractTypeName }));
                
                // Setup AI client for analysis steps
                mockAiClientForAgent
                    .Setup(x => x.SendPrompt(
                        It.Is<string>(p => !p.Contains("lesbar") && !p.Contains("Erkenne")),
                        It.IsAny<object>()))
                    .ReturnsAsync((string prompt, object? schema) =>
                    {
                        // Return minimal valid responses
                        if (prompt.Contains("allgemein", StringComparison.OrdinalIgnoreCase))
                        {
                            return JsonSerializer.Serialize(new
                            {
                                vertragstyp = "Test",
                                mieter = "Test",
                                vermieter = "Test",
                                mietobjektAdresse = "Test",
                                vertragsbeginn = "2024-01-01",
                                vertragsart = "unbefristet"
                            });
                        }
                        return JsonSerializer.Serialize(new
                        {
                            kaltmiete = 1000.0,
                            nebenkosten = 200.0,
                            warmmiete = 1200.0,
                            kaution = 3000.0,
                            zahlungsweise = "monatlich",
                            nebenkostenregelung = "Pauschale",
                            kuendigungsfrist = "3 Monate",
                            tierhaltung = "erlaubt",
                            untervermietung = "mit Zustimmung",
                            besonderheiten = "keine",
                            kurzfazit = "Test",
                            gesamtfazit = "Test",
                            gesamtbewertung = "Gut",
                            riskLevel = "Gruen"
                        });
                    });
                
                // Setup schema provider and validator
                mockSchemaProviderForAgent
                    .Setup(x => x.GetSchema(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(BinaryData.FromString("{}"));
                
                mockSchemaValidatorForAgent
                    .Setup(x => x.Validate(It.IsAny<System.Text.Json.Nodes.JsonObject>(), It.IsAny<System.Text.Json.Nodes.JsonObject>()))
                    .Returns(true);
                
                // Create Rental agent (Employment is skeleton, so we only test Rental)
                var mockContextProviderForAgent = new Mock<IContextProvider>();
                mockContextProviderForAgent.Setup(x => x.GetContextForCurrentStepAsync()).ReturnsAsync(string.Empty);
                var rentalAgent = new RentalContractAgent(
                    mockAiClientForAgent.Object,
                    mockSchemaProviderForAgent.Object,
                    mockSchemaValidatorForAgent.Object,
                    mockContextProviderForAgent.Object,
                    new Mock<ILogger<RentalContractAgent>>().Object
                );

                var handler = new WebSocketAnalysisHandler(
                    mockLogger.Object,
                    mockFileValidator.Object);

                // Create test WebSocket
                var testSocket = new TestWebSocket();

                // Create WebSocket file DTO (ContractType is optional now)
                var fileDto = new WebSocketFileDto
                {
                    FileName = fileName,
                    Data = Convert.ToBase64String(fileData)
                };

                // Enqueue the file data
                testSocket.EnqueueReceive(JsonSerializer.Serialize(fileDto));

                // Create mock HttpContext
                var mockContext = new Mock<HttpContext>();
                var mockWebSocketManager = new Mock<WebSocketManager>();
                mockWebSocketManager
                    .Setup(x => x.AcceptWebSocketAsync())
                    .ReturnsAsync(testSocket);
                mockContext.Setup(x => x.WebSockets).Returns(mockWebSocketManager.Object);

                // Act
                try
                {
                    handler.HandleAsync(mockContext.Object, rentalAgent).GetAwaiter().GetResult();

                    // Assert - Verify that the file validator was called with the correct data
                    mockFileValidator.Verify(
                        x => x.ValidateAndDecodeAsync(It.Is<IFormFile>(f => f.FileName == fileName)),
                        Times.Once);

                    // Verify that the AI client was called (initialization + analysis)
                    mockAiClientForAgent.Verify(
                        x => x.SendPrompt(It.IsAny<string>(), It.IsAny<object>()),
                        Times.AtLeastOnce);

                    // Verify WebSocket was closed normally
                    var wasClosedNormally = testSocket.CloseStatus == System.Net.WebSockets.WebSocketCloseStatus.NormalClosure;
                    
                    // If not closed normally, check for error messages
                    if (!wasClosedNormally)
                    {
                        var sentMessages = testSocket.SentMessages;
                        var messages = sentMessages.Select(m => Encoding.UTF8.GetString(m.Data)).ToList();
                        var errorMessage = messages.FirstOrDefault(m => m.Contains("error"));
                        System.Diagnostics.Debug.WriteLine($"Test failed. Close status: {testSocket.CloseStatus}. Error: {errorMessage}");
                    }
                    
                    return wasClosedNormally;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Test exception: {ex.Message}");
                    return false;
                }
            });
    }

    /// <summary>
    /// Unit test to verify basic WebSocket file reception works.
    /// </summary>
    [Fact(Skip = "Needs migration to SendPromptWithMetadata - see TEST_FAILURES_ANALYSIS.md")]
    public async Task HandleAsync_WithValidFile_ReceivesFileAndContractType()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<WebSocketAnalysisHandler>>();
        var mockFileValidator = new Mock<IFileValidatorService>();

        // Setup file validator
        mockFileValidator
            .Setup(x => x.ValidateAndDecodeAsync(It.IsAny<IFormFile>()))
            .ReturnsAsync(new FileContentDto("test.pdf", "Valid contract text"));

        // Create a real agent with mocked dependencies
        var mockAiClient = new Mock<IAiClient>();
        var mockSchemaProvider = new Mock<ISchemaProvider>();
        var mockSchemaValidator = new Mock<ISchemaValidator>();
        var mockAgentLogger = new Mock<ILogger<RentalContractAgent>>();
        
        // Setup AI client for initialization
        mockAiClient
            .Setup(x => x.SendPrompt(
                It.Is<string>(p => p.Contains("lesbar", StringComparison.OrdinalIgnoreCase)),
                It.IsAny<object>()))
            .ReturnsAsync(JsonSerializer.Serialize(new { isReadable = true }));
        
        mockAiClient
            .Setup(x => x.SendPrompt(
                It.Is<string>(p => p.Contains("Erkenne die Art", StringComparison.OrdinalIgnoreCase)),
                It.IsAny<object>()))
            .ReturnsAsync(JsonSerializer.Serialize(new { contractType = "Mietvertrag" }));
        
        // Setup AI client for analysis steps
        mockAiClient
            .Setup(x => x.SendPrompt(
                It.Is<string>(p => !p.Contains("lesbar") && !p.Contains("Erkenne")),
                It.IsAny<object>()))
            .ReturnsAsync((string prompt, object? schema) =>
            {
                // Return appropriate responses based on prompt
                if (prompt.Contains("allgemein", StringComparison.OrdinalIgnoreCase))
                {
                    return JsonSerializer.Serialize(new
                    {
                        vertragstyp = "Test",
                        mieter = "Test Mieter",
                        vermieter = "Test Vermieter",
                        mietobjektAdresse = "Test Adresse",
                        vertragsbeginn = "2024-01-01",
                        vertragsart = "unbefristet"
                    });
                }
                else if (prompt.Contains("Rent", StringComparison.OrdinalIgnoreCase))
                {
                    return JsonSerializer.Serialize(new
                    {
                        kaltmiete = 1000.0,
                        nebenkosten = 200.0,
                        warmmiete = 1200.0,
                        kaution = 3000.0,
                        zahlungsweise = "monatlich",
                        nebenkostenregelung = "Pauschale"
                    });
                }
                else if (prompt.Contains("Conditions", StringComparison.OrdinalIgnoreCase))
                {
                    return JsonSerializer.Serialize(new
                    {
                        kuendigungsfrist = "3 Monate",
                        tierhaltung = "erlaubt",
                        untervermietung = "mit Zustimmung",
                        besonderheiten = "keine"
                    });
                }
                else if (prompt.Contains("Zusammenfassung", StringComparison.OrdinalIgnoreCase))
                {
                    return JsonSerializer.Serialize(new
                    {
                        kurzfazit = "Test Fazit",
                        gesamtfazit = "Test Gesamtfazit",
                        gesamtbewertung = "Gut",
                        riskLevel = "Gruen"
                    });
                }
                return "{}";
            });
        
        // Setup schema provider and validator
        mockSchemaProvider
            .Setup(x => x.GetSchema(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(BinaryData.FromString("{}"));
        
        mockSchemaValidator
            .Setup(x => x.Validate(It.IsAny<System.Text.Json.Nodes.JsonObject>(), It.IsAny<System.Text.Json.Nodes.JsonObject>()))
            .Returns(true);
        
        var mockContextProvider2 = new Mock<IContextProvider>();
        mockContextProvider2.Setup(x => x.GetContextForCurrentStepAsync()).ReturnsAsync(string.Empty);
        
        var realAgent = new RentalContractAgent(
            mockAiClient.Object,
            mockSchemaProvider.Object,
            mockSchemaValidator.Object,
            mockContextProvider2.Object,
            mockAgentLogger.Object
        );

        var handler = new WebSocketAnalysisHandler(
            mockLogger.Object,
            mockFileValidator.Object);

        // Create test WebSocket
        var testSocket = new TestWebSocket();

        // Create WebSocket file DTO (ContractType is optional now)
        var fileDto = new WebSocketFileDto
        {
            FileName = "test.pdf",
            Data = Convert.ToBase64String(Encoding.UTF8.GetBytes("Test file content"))
        };

        // Enqueue the file data
        testSocket.EnqueueReceive(JsonSerializer.Serialize(fileDto));

        // Create mock HttpContext
        var mockContext = new Mock<HttpContext>();
        var mockWebSocketManager = new Mock<WebSocketManager>();
        mockWebSocketManager
            .Setup(x => x.AcceptWebSocketAsync())
            .ReturnsAsync(testSocket);
        mockContext.Setup(x => x.WebSockets).Returns(mockWebSocketManager.Object);

        // Act
        await handler.HandleAsync(mockContext.Object, realAgent);

        // Assert
        // Check what happened
        var sentMessages = testSocket.SentMessages;
        Assert.NotEmpty(sentMessages); // Should have sent at least one message
        
        // Check if there was an error message
        var messages = sentMessages.Select(m => Encoding.UTF8.GetString(m.Data)).ToList();
        var errorMessage = messages.FirstOrDefault(m => m.Contains("error"));
        
        // If there's an error, fail with the error message for debugging
        if (errorMessage != null)
        {
            Assert.Fail($"Handler sent error: {errorMessage}. Close status: {testSocket.CloseStatus}");
        }

        // Verify file validator was called
        mockFileValidator.Verify(
            x => x.ValidateAndDecodeAsync(It.Is<IFormFile>(f => f.FileName == "test.pdf")),
            Times.Once);
        
        // Verify AI client was called for initialization and analysis
        mockAiClient.Verify(
            x => x.SendPrompt(It.IsAny<string>(), It.IsAny<object>()),
            Times.AtLeastOnce);
        
        // Verify WebSocket was closed normally
        Assert.Equal(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, testSocket.CloseStatus);
        
        // Verify we got confirmation and result messages
        Assert.True(messages.Any(m => m.Contains("confirmation")), "Should have sent confirmation message");
        Assert.True(messages.Any(m => m.Contains("result")), "Should have sent result messages");
    }
}
