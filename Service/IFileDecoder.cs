namespace PocketLawyer.Service
{
    public interface IFileDecoder
    {
        bool CanDecode(string ext);
        Task<string> DecodeAsync(Stream input);
    }
}
