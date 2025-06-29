namespace PocketLawyer.Service;

/// <summary>
/// Exception thrown when analysis of a contract fails.
/// </summary>
public class AnalysisException : Exception
{
    public string? AnalysisStep { get; }

    public AnalysisException(string message, string? analysisStep = null)
        : base(message)
    {
        AnalysisStep = analysisStep;
    }

    public AnalysisException(string message, string? analysisStep, Exception? innerException)
        : base(message, innerException)
    {
        AnalysisStep = analysisStep;
    }
}
