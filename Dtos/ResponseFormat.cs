namespace PocketLawyer;

public class ResponseFormat
{
    public string Name { get; set; } = string.Empty;
    public string Hauptstadt { get; set; } = string.Empty;
    public int Population { get; set; }
    public int Flaeche { get; set; }
    public string[] Bundeslaender { get; set; } = [];
    public string[] Sehenswuerdigkeiten { get; set; } = [];
}
