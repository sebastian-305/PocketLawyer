namespace PocketLawyer;

public class ContractAnalysisDto
{
    public Summary summary { get; set; } = new();
    public ProblemKlausel[] unzulaessige_klauseln { get; set; } = [];
    public ProblemKlausel[] nachteile_klauseln { get; set; } = [];
    public WiderspruechlicheKlausel[] widerspruechliche_klauseln { get; set; } = [];
    public VorteilKlausel[] vorteilhafte_klauseln { get; set; } = [];
    public string fazit { get; set; } = string.Empty;
}

public class Summary
{
    public string arbeitgeber { get; set; } = string.Empty;
    public string taetigkeit { get; set; } = string.Empty;
    public string beginn { get; set; } = string.Empty;
    public string dauer { get; set; } = string.Empty;
    public string gehalt_brutto { get; set; } = string.Empty;
    public string zuschlaege { get; set; } = string.Empty;
    public string arbeitsstunden { get; set; } = string.Empty;
    public string ueberstunden { get; set; } = string.Empty;
    public string probezeit { get; set; } = string.Empty;
}

public class ProblemKlausel
{
    public string titel { get; set; } = string.Empty;
    public string zitat { get; set; } = string.Empty;
    public string problem { get; set; } = string.Empty;
    public string rechtsgrundlage { get; set; } = string.Empty;
}

public class WiderspruechlicheKlausel
{
    public string titel { get; set; } = string.Empty;
    public string zitat_klausel_a { get; set; } = string.Empty;
    public string zitat_klausel_b { get; set; } = string.Empty;
    public string problem { get; set; } = string.Empty;
    public string rechtsgrundlage { get; set; } = string.Empty;
}

public class VorteilKlausel
{
    public string titel { get; set; } = string.Empty;
    public string zitat { get; set; } = string.Empty;
    public string vorteil { get; set; } = string.Empty;
}
