namespace PocketLawyer.Options;

public class ContextOptions
{
    public string BasePath { get; set; } = "data/Context";
    public bool CreateReadmeFiles { get; set; } = true;
    public long MaxContextSizeBytes { get; set; } = 5 * 1024 * 1024; // 5MB
}
