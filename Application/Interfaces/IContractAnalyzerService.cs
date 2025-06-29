using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Application.Interfaces;

// Service zur Analyse von Vertragsinhalten mittels KI.

// Definiert die zentrale Analyse-Methode.
public interface IContractAnalyzerService
{
    Task<T> ProcessContractRequest<T>(
        FileContentDto content,
        Func<string, string, string> promptBuilder,
        object schema,
        bool useAltKey,
        bool detectKv = false,
        string? kvName = null);
}
