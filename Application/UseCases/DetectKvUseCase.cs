using PocketLawyer.Application.Interfaces;

// Anwendungsfall zum Auffinden eines passenden Kollektivvertrages.
using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Application.UseCases;

// Delegiert die Erkennung an den entsprechenden Service.
public class DetectKvUseCase
{
    private readonly IKvDetectionService _detection;

    public DetectKvUseCase(IKvDetectionService detection)
    {
        _detection = detection;
    }

    public Task<KvDetectionResult> DetectAsync(string content)
    {
        return _detection.DetectAsync(content);
    }
}
