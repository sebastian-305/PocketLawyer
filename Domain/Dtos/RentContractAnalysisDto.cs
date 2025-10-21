using System.Text.Json.Serialization;

namespace PocketLawyer.Domain.Dtos;

public class RentContractAnalysisDto : IAnalysisDto
{
    [JsonPropertyName("keypoints")]
    public Summary Summary { get; set; } = new();

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




}
public class Summary
{
    [JsonPropertyName("vermieter")]
    public string Vermieter { get; set; } = string.Empty;

    [JsonPropertyName("mieter")]
    public string Mieter { get; set; } = string.Empty;

    [JsonPropertyName("beginn")]
    public string Beginn { get; set; } = string.Empty;

    [JsonPropertyName("dauer")]
    public string Dauer { get; set; } = string.Empty;

    [JsonPropertyName("mindestdauer")]
    public string Mindestdauer { get; set; } = string.Empty;

    [JsonPropertyName("kuendigungsfrist")]
    public string Kuendigungsfrist { get; set; } = string.Empty;

    [JsonPropertyName("miete_kalt")]
    public string MieteKalt { get; set; } = string.Empty;

    [JsonPropertyName("betriebskosten")]
    public string Betriebskosten { get; set; } = string.Empty;

    [JsonPropertyName("quadratmeter")]
    public string Quadratmeter { get; set; } = string.Empty;

    [JsonPropertyName("zimmer")]
    public int Zimmer { get; set; }

    [JsonPropertyName("im_mietgegenstand_enthalten")]
    public string ImMietgegenstandEnthalten { get; set; } = string.Empty;
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
