using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Application.UseCases;
using PocketLawyer.Domain.Dtos;
using PocketLawyer.Web;

namespace PocketLawyer.Tests;

public class AnalyzeRentUseCaseTests
{
    private const string SampleResponse = """
    {
      "keypoints": {
        "vermieter": "Max Mustermann",
        "mieter": "Erika Muster",
        "beginn": "01.01.2024",
        "dauer": "Unbefristet",
        "mindestdauer": "12 Monate",
        "kuendigungsfrist": "3 Monate",
        "miete_kalt": "900 EUR",
        "betriebskosten": "150 EUR",
        "quadratmeter": "75",
        "zimmer": 3,
        "im_mietgegenstand_enthalten": "Einbauküche"
      },
      "unzulaessige_klauseln": [
        {
          "titel": "Überhöhte Vertragsstrafe",
          "zitat": "Bei verspäteter Zahlung wird eine Strafe von 20% fällig.",
          "problem": "Die Strafe ist unangemessen hoch.",
          "rechtsgrundlage": [
            {
              "rechtsgrundlage": "ABGB §1336",
              "wortlaut": "Vertragsstrafen müssen angemessen sein."
            }
          ]
        }
      ],
      "nachteile_klauseln": [
        {
          "titel": "Kleinreparaturklausel",
          "zitat": "Mieter trägt Reparaturen bis 500 EUR.",
          "problem": "Ungewöhnlich hoher Höchstbetrag.",
          "rechtsgrundlage": [
            {
              "rechtsgrundlage": "MRG §3",
              "wortlaut": "Kleinreparaturen sind Vermietersache."
            }
          ]
        }
      ],
      "widerspruechliche_klauseln": [
        {
          "titel": "Nebenkosten widersprüchlich",
          "zitat_klausel_a": "Nebenkosten pauschal 150 EUR.",
          "zitat_klausel_b": "Nebenkosten nach Verbrauch abzurechnen.",
          "problem": "Unklarheit über Abrechnung.",
          "rechtsgrundlage": [
            {
              "rechtsgrundlage": "MRG §21",
              "wortlaut": "Abrechnung muss nachvollziehbar sein."
            }
          ]
        }
      ],
      "vorteilhafte_klauseln": [
        {
          "titel": "Staffelmietverzicht",
          "zitat": "Keine Staffelmiete in den ersten 5 Jahren.",
          "vorteil": "Planbare Mietkosten."
        }
      ],
      "fazit": "Insgesamt ausgewogen mit kleineren Beanstandungen.",
      "kollektivvertrag": "Muster-KV"
    }
    """;

    [Fact]
    public async Task ProcessContractRequest_ReturnsExpectedDtoAndRoundtrips()
    {
      var aiClient = new Mock<IAiClient>();
      aiClient.Setup(client => client.SendPrompt(It.IsAny<string>(), It.IsAny<object?>()))
        .ReturnsAsync(SampleResponse);

      var kvService = new Mock<IKollektivvertragService>();
      var kvDetection = new Mock<IKvDetectionService>();
      var environment = new Mock<IWebHostEnvironment>();
      environment.SetupGet(env => env.ContentRootPath).Returns(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

      var logger = new Mock<ILogger<AnalyzeRentUseCase>>();
      var useCase = new AnalyzeRentUseCase(
        logger.Object,
        aiClient.Object,
        kvService.Object,
        kvDetection.Object,
        environment.Object);

      var fileContent = new FileContentDto("vertrag.pdf", "Inhalt");
      var schema = new { type = "object" };
      RentContractAnalysisDto dto = await useCase.ProcessContractRequest<RentContractAnalysisDto>(
        fileContent,
        (_, _, _) => "prompt",
        schema);

      Assert.Equal("Max Mustermann", dto.Summary.Vermieter);
      var unzulaessigeDto = Assert.Single(dto.UnzulaessigeKlauseln);
      Assert.Equal("Insgesamt ausgewogen mit kleineren Beanstandungen.", dto.Fazit);
      Assert.Equal("Muster-KV", dto.Kollektivvertrag);
      Assert.Equal(3, dto.Summary.Zimmer);

      string roundtrip = JsonSerializer.Serialize(dto, dto.GetType());
      using JsonDocument originalDoc = JsonDocument.Parse(SampleResponse);
      using JsonDocument roundtripDoc = JsonDocument.Parse(roundtrip);

      JsonElement originalRoot = originalDoc.RootElement;
      JsonElement roundtripRoot = roundtripDoc.RootElement;

      Assert.True(roundtripRoot.TryGetProperty("keypoints", out JsonElement keypoints));
      Assert.Equal(
        originalRoot.GetProperty("keypoints").GetProperty("vermieter").GetString(),
        keypoints.GetProperty("vermieter").GetString());
      Assert.Equal(3, keypoints.GetProperty("zimmer").GetInt32());

      Assert.True(roundtripRoot.TryGetProperty("unzulaessige_klauseln", out JsonElement unzul));
      Assert.Equal(originalRoot.GetProperty("unzulaessige_klauseln").GetArrayLength(), unzul.GetArrayLength());
      Assert.Equal(
        originalRoot.GetProperty("unzulaessige_klauseln")[0].GetProperty("titel").GetString(),
        unzul[0].GetProperty("titel").GetString());
      Assert.Equal("Überhöhte Vertragsstrafe", unzulaessigeDto.Titel);

      Assert.True(roundtripRoot.TryGetProperty("nachteile_klauseln", out JsonElement nachteile));
      Assert.Equal(
        originalRoot.GetProperty("nachteile_klauseln")[0].GetProperty("problem").GetString(),
        nachteile[0].GetProperty("problem").GetString());

      Assert.True(roundtripRoot.TryGetProperty("widerspruechliche_klauseln", out JsonElement widerspruechliche));
      Assert.Equal(
        originalRoot.GetProperty("widerspruechliche_klauseln")[0].GetProperty("zitat_klausel_a").GetString(),
        widerspruechliche[0].GetProperty("zitat_klausel_a").GetString());

      Assert.True(roundtripRoot.TryGetProperty("vorteilhafte_klauseln", out JsonElement vorteile));
      Assert.Equal(
        originalRoot.GetProperty("vorteilhafte_klauseln")[0].GetProperty("vorteil").GetString(),
        vorteile[0].GetProperty("vorteil").GetString());

      Assert.True(roundtripRoot.TryGetProperty("fazit", out JsonElement fazit));
      Assert.Equal(originalRoot.GetProperty("fazit").GetString(), fazit.GetString());

      Assert.True(roundtripRoot.TryGetProperty("kollektivvertrag", out JsonElement kv));
      Assert.Equal("Muster-KV", kv.GetString());
    }
}
