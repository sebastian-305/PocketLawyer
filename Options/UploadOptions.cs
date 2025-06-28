namespace PocketLawyer;

public class UploadOptions
{
    public long MaxUploadSizeMB { get; set; } = 20;
    public string ResponsesPath { get; set; } = "Responses";
}
