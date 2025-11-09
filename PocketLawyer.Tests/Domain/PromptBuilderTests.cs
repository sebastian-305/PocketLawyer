using PocketLawyer.Domain;
using PocketLawyer.Web;

namespace PocketLawyer.Tests.Domain;

public class PromptBuilderTests
{
    [Fact]
    public void AddKvInfo_AppendsInstructionsAndFiles()
    {
        // Dieser Test prüft, dass Zusatzinformationen zum Kollektivvertrag inklusive erklärendem Text und sämtlicher Dateiinhalte an das Prompt angehängt werden.
        var prompt = "Basis";
        var kvFiles = new[] { "KV1", "KV2" };

        string result = PromptBuilder.AddKvInfo(prompt, "Metall", kvFiles);

        Assert.Contains("Kollektivvertrag \"Metall\"", result);
        Assert.Contains("KV1", result);
        Assert.Contains("KV2", result);
    }

    [Fact]
    public void BuildEmploymentAnalysisPrompt_ContainsFilenameAndContent()
    {
        // Dieser Test prüft, dass der Beschäftigungs-Prompt den Dateinamen, den Vertragsinhalt und die Analyseleitlinien enthält.
        string result = PromptBuilder.BuildEmploymentAnalysisPrompt(AgentType.Employment, "vertrag.pdf", "Inhalt");

        Assert.Contains("vertrag.pdf", result);
        Assert.Contains("Vertragsinhalt", result);
        Assert.Contains("Arbeitsvertrag", result);
    }

    [Fact]
    public void BuildValidationPrompt_UsesTypeSpecificLanguage()
    {
        // Dieser Test prüft, dass der Validierungs-Prompt den gewählten Vertragstyp korrekt benennt und Dateimetadaten einbettet.
        string employmentPrompt = PromptBuilder.BuildValidationPrompt(AgentType.Employment, "job.pdf", "Text");
        string rentPrompt = PromptBuilder.BuildValidationPrompt(AgentType.Rent, "rent.pdf", "Text");

        Assert.Contains("Arbeitsvertrag", employmentPrompt);
        Assert.Contains("Mietvertrag", rentPrompt);
        Assert.Contains("job.pdf", employmentPrompt);
        Assert.Contains("rent.pdf", rentPrompt);
    }

    [Fact]
    public void BuildKvDetectionPrompt_ListsAllFolders()
    {
        // Dieser Test prüft, dass der KV-Erkennungs-Prompt alle Verzeichnisnamen auflistet und den Vertragsinhalt voranstellt.
        var prompt = PromptBuilder.BuildKvDetectionPrompt("Vertrag", new[] { "KV_A", "KV_B" });

        Assert.Contains("Vertrag", prompt);
        Assert.Contains("KV_A", prompt);
        Assert.Contains("KV_B", prompt);
    }
}
