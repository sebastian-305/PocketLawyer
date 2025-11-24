using System.Text.Json.Serialization;

namespace PocketLawyer.Domain.Dtos;

public class EmploymentContractAnalysisDto : IAnalysisDto
{
    public string ContractType => "Arbeitsvertrag";
    public string Tier => "Legacy";

    [JsonPropertyName("keypoints")]
    public SummaryClass Keypoints { get; set; } = new();

    [JsonPropertyName("unzulaessige_klauseln")]
    public ProblemKlausel[] UnzulaessigeKlauseln { get; set; } = [];

    [JsonPropertyName("nachteile_klauseln")]
    public ProblemKlausel[] NachteileKlauseln { get; set; } = [];

    [JsonPropertyName("widerspruechliche_klauseln")]
    public WiderspruechlicheKlausel[] WiderspruechlicheKlauseln { get; set; } = [];

    [JsonPropertyName("vorteilhafte_klauseln")]
    public VorteilKlausel[] VorteilhafteKlauseln { get; set; } = [];

    [JsonPropertyName("fazit")]
    public string Fazit { get; set; } = string.Empty;

    [JsonPropertyName("kollektivvertrag")]
    public string? Kollektivvertrag { get; set; }



    public class SummaryClass
    {
        [JsonPropertyName("arbeitgeber")]
        public string Arbeitgeber { get; set; } = string.Empty;

        [JsonPropertyName("taetigkeit")]
        public string Taetigkeit { get; set; } = string.Empty;

        [JsonPropertyName("beginn")]
        public string Beginn { get; set; } = string.Empty;

        [JsonPropertyName("dauer")]
        public string Dauer { get; set; } = string.Empty;

        [JsonPropertyName("gehalt_brutto")]
        public string GehaltBrutto { get; set; } = string.Empty;

        [JsonPropertyName("zuschlaege")]
        public string Zuschlaege { get; set; } = string.Empty;

        [JsonPropertyName("arbeitsstunden")]
        public string Arbeitsstunden { get; set; } = string.Empty;

        [JsonPropertyName("ueberstunden")]
        public string Ueberstunden { get; set; } = string.Empty;

        [JsonPropertyName("probezeit")]
        public string Probezeit { get; set; } = string.Empty;
    }

    public class RechtsgrundlageEintrag
    {
        [JsonPropertyName("rechtsgrundlage")]
        public string Rechtsgrundlage { get; set; } = string.Empty;

        [JsonPropertyName("wortlaut")]
        public string Wortlaut { get; set; } = string.Empty;
    }

    public class ProblemKlausel
    {
        [JsonPropertyName("titel")]
        public string Titel { get; set; } = string.Empty;

        [JsonPropertyName("zitat")]
        public string Zitat { get; set; } = string.Empty;

        [JsonPropertyName("problem")]
        public string Problem { get; set; } = string.Empty;

        [JsonPropertyName("rechtsgrundlage")]
        public RechtsgrundlageEintrag[] Rechtsgrundlage { get; set; } = [];
    }

    public class WiderspruechlicheKlausel
    {
        [JsonPropertyName("titel")]
        public string Titel { get; set; } = string.Empty;

        [JsonPropertyName("zitat_klausel_a")]
        public string ZitatKlauselA { get; set; } = string.Empty;

        [JsonPropertyName("zitat_klausel_b")]
        public string ZitatKlauselB { get; set; } = string.Empty;

        [JsonPropertyName("problem")]
        public string Problem { get; set; } = string.Empty;

        [JsonPropertyName("rechtsgrundlage")]
        public RechtsgrundlageEintrag[] Rechtsgrundlage { get; set; } = [];
    }

    public class VorteilKlausel
    {
        [JsonPropertyName("titel")]
        public string Titel { get; set; } = string.Empty;

        [JsonPropertyName("zitat")]
        public string Zitat { get; set; } = string.Empty;

        [JsonPropertyName("vorteil")]
        public string Vorteil { get; set; } = string.Empty;
    }
}

