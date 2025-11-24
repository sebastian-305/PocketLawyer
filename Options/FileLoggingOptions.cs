namespace PocketLawyer.Options;

public class FileLoggingOptions
{
    public bool Enabled { get; set; } = true;
    public string MinimumLevel { get; set; } = "Information";
    public string Path { get; set; } = "logs";
}
