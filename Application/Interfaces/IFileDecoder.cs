namespace PocketLawyer.Application.Interfaces;

public interface IFileDecoder
{
    bool CanDecode(string ext);
    Task<string> DecodeAsync(Stream input);
}
