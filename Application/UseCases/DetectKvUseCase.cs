using PocketLawyer.Application.Interfaces;
using PocketLawyer.Domain.Dtos.Shared;

namespace PocketLawyer.Application.UseCases;

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
