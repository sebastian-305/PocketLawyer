using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Application.Interfaces;

// Schnittstelle für die KI-basierte KV-Erkennung.

// Erkennt anhand des Vertrags den zutreffenden KV.
public interface IKvDetectionService
{
    Task<KvDetectionResult> DetectAsync(string contractContent);
}
