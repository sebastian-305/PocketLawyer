using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PocketLawyer.Application.Agents;
using PocketLawyer.Application.Exceptions;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Domain;
using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Tests.Application.Agents;

public class AnalyzeEmploymentAgentTests
{
    private static AnalyzeEmploymentAgent CreateAgent(Mock<IAiClient> aiMock, Mock<IKollektivvertragService> kvServiceMock, Mock<IKvDetectionService> detectionMock)
    {
        return new AnalyzeEmploymentAgent(
            NullLogger<AnalyzeEmploymentAgent>.Instance,
            aiMock.Object,
            kvServiceMock.Object,
            detectionMock.Object);
    }

    [Fact]
    public async Task ProcessContractRequest_AddsKvInfoDuringValidation()
    {
        // Dieser Test prüft, dass bei übergebenem KV-Namen die Prompt-Erstellung um KV-Inhalte ergänzt und das Ergebnisobjekt mit dem KV-Namen angereichert wird.
        var aiMock = new Mock<IAiClient>();
        string? capturedPrompt = null;
        aiMock.Setup(a => a.SendPrompt(It.IsAny<string>(), It.IsAny<object?>()))
            .Callback<string, object?>((prompt, _) => capturedPrompt = prompt)
            .ReturnsAsync("{\"isReadable\":true,\"isContract\":true,\"result\":\"ok\"}");
        var kvServiceMock = new Mock<IKollektivvertragService>();
        kvServiceMock.Setup(s => s.GetKvFileContentsAsync("Metall"))
            .ReturnsAsync(new[] { "KV Text" });
        var detectionMock = new Mock<IKvDetectionService>();
        var agent = CreateAgent(aiMock, kvServiceMock, detectionMock);
        var content = new FileContentDto("vertrag.pdf", "text");
        var validation = new ContractValidationDto { Kollektivvertrag = "Metall" };

        ContractValidationDto result = await agent.ProcessContractRequest<ContractValidationDto>(content, AgentAction.Validate, validation);

        Assert.NotNull(capturedPrompt);
        Assert.Contains("KV Text", capturedPrompt);
        Assert.Equal("Metall", result.Kollektivvertrag);
        kvServiceMock.Verify(s => s.GetKvFileContentsAsync("Metall"), Times.Once);
    }

    [Fact]
    public async Task ProcessContractRequest_AppliesKvNameToAnalysisResult()
    {
        // Dieser Test prüft, dass bei der Analyse der KV-Inhalt an den Prompt angefügt und der zurückgelieferte Analyse-Datensatz mit dem KV-Namen versehen wird.
        var aiMock = new Mock<IAiClient>();
        string? capturedPrompt = null;
        aiMock.Setup(a => a.SendPrompt(It.IsAny<string>(), It.IsAny<object?>()))
            .Callback<string, object?>((prompt, _) => capturedPrompt = prompt)
            .ReturnsAsync("{\"keypoints\":{\"arbeitgeber\":\"\",\"taetigkeit\":\"\",\"beginn\":\"\",\"dauer\":\"\",\"gehalt_brutto\":\"\",\"zuschlaege\":\"\",\"arbeitsstunden\":\"\",\"ueberstunden\":\"\",\"probezeit\":\"\"},\"unzulaessige_klauseln\":[],\"nachteile_klauseln\":[],\"widerspruechliche_klauseln\":[],\"vorteilhafte_klauseln\":[],\"fazit\":\"ok\"}");
        var kvServiceMock = new Mock<IKollektivvertragService>();
        kvServiceMock.Setup(s => s.GetKvFileContentsAsync("Metall"))
            .ReturnsAsync(new[] { "KV Text" });
        var detectionMock = new Mock<IKvDetectionService>();
        var agent = CreateAgent(aiMock, kvServiceMock, detectionMock);
        var content = new FileContentDto("vertrag.pdf", "text");
        var validation = new ContractValidationDto { Kollektivvertrag = "Metall" };

        var result = await agent.ProcessContractRequest<EmploymentContractAnalysisDto>(content, AgentAction.Analyze, validation);

        Assert.NotNull(capturedPrompt);
        Assert.Contains("KV Text", capturedPrompt);
        Assert.Equal("Metall", result.Kollektivvertrag);
    }

    [Fact]
    public async Task ProcessContractRequest_ThrowsAnalysisExceptionOnInvalidJson()
    {
        // Dieser Test prüft, dass unparsebare KI-Antworten mit einer AnalysisException gemeldet werden, um Fehlinterpretationen zu verhindern.
        var aiMock = new Mock<IAiClient>();
        aiMock.Setup(a => a.SendPrompt(It.IsAny<string>(), It.IsAny<object?>()))
            .ReturnsAsync("not json");
        var kvServiceMock = new Mock<IKollektivvertragService>();
        var detectionMock = new Mock<IKvDetectionService>();
        var agent = CreateAgent(aiMock, kvServiceMock, detectionMock);
        var content = new FileContentDto("vertrag.pdf", "text");

        await Assert.ThrowsAsync<AnalysisException>(() => agent.ProcessContractRequest<ContractValidationDto>(content, AgentAction.Validate));
    }
}
