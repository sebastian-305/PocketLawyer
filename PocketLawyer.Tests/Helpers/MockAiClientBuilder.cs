using Moq;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Application.Models;
using System.Text.Json;

namespace PocketLawyer.Tests.Helpers;

/// <summary>
/// Builder for creating mock IAiClient instances with consistent configuration.
/// Simplifies test setup and ensures all tests use SendPromptWithMetadata.
/// </summary>
public class MockAiClientBuilder
{
    private readonly Mock<IAiClient> _mock = new();
    private bool _readabilityConfigured = false;
    private bool _contractTypeConfigured = false;

    /// <summary>
    /// Configures the mock to return a specific readability check result.
    /// </summary>
    public MockAiClientBuilder WithReadabilityCheck(bool isReadable)
    {
        _mock.Setup(x => x.SendPromptWithMetadata(
                It.IsAny<string>(),
                It.Is<object?>(s => s != null && s.ToString()!.Contains("isReadable"))))
            .ReturnsAsync(new AiResponse(
                Content: JsonSerializer.Serialize(new { isReadable }),
                InputTokens: 100,
                OutputTokens: 10,
                TotalTokens: 110
            ));
        
        _readabilityConfigured = true;
        return this;
    }

    /// <summary>
    /// Configures the mock to return a specific contract type.
    /// </summary>
    public MockAiClientBuilder WithContractType(string contractType)
    {
        _mock.Setup(x => x.SendPromptWithMetadata(
                It.IsAny<string>(),
                It.Is<object?>(s => s != null && s.ToString()!.Contains("contractType"))))
            .ReturnsAsync(new AiResponse(
                Content: JsonSerializer.Serialize(new { contractType }),
                InputTokens: 100,
                OutputTokens: 10,
                TotalTokens: 110
            ));
        
        _contractTypeConfigured = true;
        return this;
    }

    /// <summary>
    /// Configures the mock to return a specific response for an analysis step.
    /// </summary>
    public MockAiClientBuilder WithAnalysisStep(string schemaKeyword, object response)
    {
        _mock.Setup(x => x.SendPromptWithMetadata(
                It.IsAny<string>(),
                It.Is<object?>(s => s != null && s.ToString()!.Contains(schemaKeyword))))
            .ReturnsAsync(new AiResponse(
                Content: JsonSerializer.Serialize(response),
                InputTokens: 500,
                OutputTokens: 200,
                TotalTokens: 700
            ));
        
        return this;
    }

    /// <summary>
    /// Configures the mock to throw an exception for a specific schema keyword.
    /// Useful for testing error handling.
    /// </summary>
    public MockAiClientBuilder WithFailingStep(string schemaKeyword, Exception exception)
    {
        _mock.Setup(x => x.SendPromptWithMetadata(
                It.IsAny<string>(),
                It.Is<object?>(s => s != null && s.ToString()!.Contains(schemaKeyword))))
            .ThrowsAsync(exception);
        
        return this;
    }

    /// <summary>
    /// Configures the mock with a callback for custom response logic.
    /// </summary>
    public MockAiClientBuilder WithCallback(Func<string, object?, AiResponse> callback)
    {
        _mock.Setup(x => x.SendPromptWithMetadata(
                It.IsAny<string>(),
                It.IsAny<object?>()))
            .ReturnsAsync((string prompt, object? schema) => callback(prompt, schema));
        
        return this;
    }

    /// <summary>
    /// Configures default successful responses for initialization (readability + contract type).
    /// </summary>
    public MockAiClientBuilder WithSuccessfulInitialization(string contractType = "Mietvertrag")
    {
        if (!_readabilityConfigured)
        {
            WithReadabilityCheck(true);
        }
        
        if (!_contractTypeConfigured)
        {
            WithContractType(contractType);
        }
        
        return this;
    }

    /// <summary>
    /// Builds and returns the configured mock IAiClient.
    /// </summary>
    public IAiClient Build()
    {
        return _mock.Object;
    }

    /// <summary>
    /// Returns the underlying Mock object for advanced verification scenarios.
    /// </summary>
    public Mock<IAiClient> GetMock()
    {
        return _mock;
    }
}
