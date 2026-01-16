namespace PocketLawyer.Domain.Dtos;

public class KvDetectionResult
{
    public string KvName { get; set; } = string.Empty;
    public bool AllEqual { get; set; }
    public string[] DetectedKvs { get; set; } = Array.Empty<string>();
    public string[] Begruendungen { get; set; } = Array.Empty<string>();
    public string Message { get; set; } = string.Empty;
}
