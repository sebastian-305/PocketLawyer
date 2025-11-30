namespace PocketLawyer.Application.Interfaces;

public interface IContextFileReader
{
    Task<string> ReadDirectoryAsync(string directoryPath);
}
