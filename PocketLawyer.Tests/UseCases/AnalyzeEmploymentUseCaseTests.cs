using Microsoft.Extensions.Logging;
using Moq;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Application.UseCases;
using PocketLawyer.Domain.Dtos;
using PocketLawyer.Web;

namespace PocketLawyer.Tests.UseCases;

public class AnalyzeEmploymentUseCaseTests
{
    [Fact]
    public async Task ProcessContractRequest_WithNonJsonResponse_ThrowsAnalysisException()
    {
        var logger = new Mock<ILogger<AnalyzeEmploymentUseCase>>();
        var aiClient = new Mock<IAiClient>();
        var kvService = new Mock<IKollektivvertragService>();
        var kvDetectionService = new Mock<IKvDetectionService>();

        aiClient
            .Setup(client => client.SendPrompt(It.IsAny<string>(), It.IsAny<object?>()))
            .ReturnsAsync("not-json");

        var useCase = new AnalyzeEmploymentUseCase(
            logger.Object,
            aiClient.Object,
            kvService.Object,
            kvDetectionService.Object);

        var fileContent = new FileContentDto("contract.txt", "content");
        Func<AnalysisType, string, string, string> promptBuilder = (_, _, _) => "prompt";
        var schema = new { };

        var exception = await Assert.ThrowsAsync<AnalysisException>(() =>
            useCase.ProcessContractRequest<ContractValidationDto>(
                fileContent,
                promptBuilder,
                schema,
                detectKv: false));

        Assert.Contains("not-json", exception.Message);
    }
}
