namespace PocketLawyer.Service
{
    public interface IFileDecoderService
    {
        public string[] AllowedExtensions { get;  }
        public string AllowedExtensionsString { get; }
        public Task<string> GetFileContentAsync(string filename, Stream content);
    }
}
