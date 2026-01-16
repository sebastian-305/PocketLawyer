namespace PocketLawyer.Options;

public class ContextOptions
{
    public string BasePath { get; set; } = "data/Context";
    public bool CreateReadmeFiles { get; set; } = true;
    public long MaxContextSizeBytes { get; set; } = 5 * 1024 * 1024; // 5MB
    
    /// <summary>
    /// Controls whether detected Kollektivvertrag content should be included in analysis prompts.
    /// Set to false if KV content is too large and causes token limit issues.
    /// </summary>
    public bool IncludeKvContentInAnalysis { get; set; } = true;
}
