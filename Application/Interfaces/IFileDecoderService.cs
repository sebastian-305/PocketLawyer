namespace PocketLawyer.Application.Interfaces;

public interface IFileDecoderService
{
    string[] AllowedExtensions { get; }
    string AllowedExtensionsString { get; }
    Task<string> GetFileContentAsync(string filename, Stream content);
}
