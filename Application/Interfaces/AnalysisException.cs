namespace PocketLawyer.Application.Interfaces;

/// <summary>
/// Wird geworfen, wenn die Analyse eines Vertrags fehlschlägt.
/// </summary>
// Speichert optional den Schritt der Analyse, der fehlgeschlagen ist.
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
