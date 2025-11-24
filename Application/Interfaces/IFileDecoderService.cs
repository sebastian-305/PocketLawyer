namespace PocketLawyer.Application.Interfaces;

/// <summary>
/// Service for decoding various file formats (PDF, text, images) to text.
/// </summary>
public interface IFileDecoderService
{
    /// <summary>
    /// Gets the array of allowed file extensions.
    /// </summary>
    string[] AllowedExtensions { get; }
    
    /// <summary>
    /// Gets a comma-separated string of allowed file extensions for display.
    /// </summary>
    string AllowedExtensionsString { get; }
    
    /// <summary>
    /// Decodes file content to text based on file extension.
    /// </summary>
    /// <param name="filename">The name of the file (used to determine file type)</param>
    /// <param name="content">The file content stream</param>
    /// <returns>The decoded text content</returns>
    /// <exception cref="FileValidationException">Thrown when file type is not supported or decoding fails</exception>
    Task<string> GetFileContentAsync(string filename, Stream content);
}
