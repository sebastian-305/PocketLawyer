using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PocketLawyer.Application.Agents;
using PocketLawyer.Application.Exceptions;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Domain;
using PocketLawyer.Domain.Dtos;
using PocketLawyer.Infrastructure;

namespace PocketLawyer.Tests.Application.Agents;

public class AnalyzeRentAgentTests
{
    private static AnalyzeRentAgent CreateAgent(Mock<IAiClient> aiMock, IWebHostEnvironment environment)
    {
        return new AnalyzeRentAgent(
            NullLogger<AnalyzeRentAgent>.Instance,
            aiMock.Object,
            environment);
    }

    [Fact]
    public async Task ProcessContractRequest_DeserializesValidationResponse()
    {
        // Dieser Test prüft, dass der Mietagent die Validierungsantwort der KI korrekt in ein ContractValidationDto überführt.
        var aiMock = new Mock<IAiClient>();
        aiMock.Setup(a => a.SendPrompt(It.IsAny<string>(), It.IsAny<object?>()))
            .ReturnsAsync("{\"isReadable\":true,\"isContract\":false,\"result\":\"check\"}");
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.SetupGet(e => e.ContentRootPath).Returns(Path.GetTempPath());
        var agent = CreateAgent(aiMock, envMock.Object);
        var content = new FileContentDto("rent.pdf", "text");

        var result = await agent.ProcessContractRequest<ContractValidationDto>(content, AgentAction.Validate);

        Assert.True(result.IsReadable);
        Assert.False(result.IsContract);
        Assert.Equal("check", result.Result);
    }

    [Fact]
    public async Task ProcessContractRequest_AppendsContextFilesForAnalysis()
    {
        // Dieser Test prüft, dass während der Analyse die Gesetzestexte aus dem Context-Verzeichnis gelesen und in den Prompt eingebettet werden.
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        string contextPath = Path.Combine(root, DefaultPaths.RentContext);
        Directory.CreateDirectory(contextPath);
        string lawFile = Path.Combine(contextPath, "law.txt");
        await File.WriteAllTextAsync(lawFile, "Paragraph 1");
        string? capturedPrompt = null;
        var aiMock = new Mock<IAiClient>();
        aiMock.Setup(a => a.SendPrompt(It.IsAny<string>(), It.IsAny<object?>()))
            .Callback<string, object?>((prompt, _) => capturedPrompt = prompt)
            .ReturnsAsync("{\"keypoints\":{\"vermieter\":\"\",\"mieter\":\"\"},\"unzulaessige_klauseln\":[],\"nachteile_klauseln\":[],\"widerspruechliche_klauseln\":[],\"vorteilhafte_klauseln\":[],\"fazit\":\"ok\"}");
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.SetupGet(e => e.ContentRootPath).Returns(root);
        var agent = CreateAgent(aiMock, envMock.Object);
        var content = new FileContentDto("rent.pdf", "text");

        var result = await agent.ProcessContractRequest<RentContractAnalysisDto>(content, AgentAction.Analyze);

        Assert.NotNull(capturedPrompt);
        Assert.Contains("Paragraph 1", capturedPrompt);
        Assert.Equal("ok", result.Fazit);
    }

    [Fact]
    public async Task ProcessContractRequest_ThrowsOnAiErrorResponse()
    {
        // Dieser Test prüft, dass eine KI-Antwort "Error" als Ausnahme gewertet wird, damit Aufrufer den Analysefehler behandeln können.
        var aiMock = new Mock<IAiClient>();
        aiMock.Setup(a => a.SendPrompt(It.IsAny<string>(), It.IsAny<object?>()))
            .ReturnsAsync("Error");
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.SetupGet(e => e.ContentRootPath).Returns(Path.GetTempPath());
        var agent = CreateAgent(aiMock, envMock.Object);
        var content = new FileContentDto("rent.pdf", "text");

        await Assert.ThrowsAsync<Exception>(() => agent.ProcessContractRequest<RentContractAnalysisDto>(content, AgentAction.Analyze));
    }
}
