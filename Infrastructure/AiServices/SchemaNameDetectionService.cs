using PocketLawyer.Application.Interfaces;
using PocketLawyer.Infrastructure.AiServices.Nebius;
using System.Security.Cryptography;

namespace PocketLawyer.Infrastructure.AiServices;

static class SchemaNameDetectionService
{
    private static readonly byte[] PreHash = Sha256(NebiusService.PreAnalyzerSchemaDefinition);
    private static readonly byte[] AnalHash = Sha256(NebiusService.AnalyzerSchemaDefinition);

    public static string GetName(object? schema)
    {
        if (schema is not BinaryData bd) return schema?.GetType().Name ?? "null";
        var h = Sha256(bd);
        if (h.AsSpan().SequenceEqual(PreHash)) return "PreAnalyzerSchema";
        if (h.AsSpan().SequenceEqual(AnalHash)) return "AnalyzerSchema";
        return "UnknownBinaryData";
    }

    private static byte[] Sha256(BinaryData bd)
    {
        using var sha = SHA256.Create();
        return sha.ComputeHash(bd.ToArray()); // optional: bd.ToStream() to avoid extra array
    }
}
