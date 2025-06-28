using System.Collections.Generic;

namespace PocketLawyer.Infrastructure.AiServices.Gemini.Models;

public class SimpleSchema
{
    public string type { get; set; } = string.Empty;
}

public class LegalReference
{
    public string type { get; set; } = "object";
    public LegalReferenceProperties properties { get; set; } = new();
    public string[] required { get; set; } = new[] { "rechtsgrundlage", "wortlaut" };
}

public class LegalReferenceProperties
{
    public SimpleSchema rechtsgrundlage { get; set; } = new() { type = "string" };
    public SimpleSchema wortlaut { get; set; } = new() { type = "string" };
}

public class ClauseInfo
{
    public string type { get; set; } = "object";
    public ClauseInfoProperties properties { get; set; } = new();
    public string[] required { get; set; } = new[] { "titel", "zitat", "problem", "rechtsgrundlage" };
}

public class ClauseInfoProperties
{
    public SimpleSchema titel { get; set; } = new() { type = "string" };
    public SimpleSchema zitat { get; set; } = new() { type = "string" };
    public SimpleSchema problem { get; set; } = new() { type = "string" };
    public ArraySchema<LegalReference> rechtsgrundlage { get; set; } = new();
}

public class AdvantageClauseInfo
{
    public string type { get; set; } = "object";
    public AdvantageClauseInfoProperties properties { get; set; } = new();
    public string[] required { get; set; } = new[] { "titel", "zitat", "vorteil" };
}

public class AdvantageClauseInfoProperties
{
    public SimpleSchema titel { get; set; } = new() { type = "string" };
    public SimpleSchema zitat { get; set; } = new() { type = "string" };
    public SimpleSchema vorteil { get; set; } = new() { type = "string" };
}

public class ConflictingClauseInfo
{
    public string type { get; set; } = "object";
    public ConflictingClauseInfoProperties properties { get; set; } = new();
    public string[] required { get; set; } = new[] { "titel", "zitat_klausel_a", "zitat_klausel_b", "problem", "rechtsgrundlage" };
}

public class ConflictingClauseInfoProperties
{
    public SimpleSchema titel { get; set; } = new() { type = "string" };
    public SimpleSchema zitat_klausel_a { get; set; } = new() { type = "string" };
    public SimpleSchema zitat_klausel_b { get; set; } = new() { type = "string" };
    public SimpleSchema problem { get; set; } = new() { type = "string" };
    public ArraySchema<LegalReference> rechtsgrundlage { get; set; } = new();
}

public class SummarySchema
{
    public string type { get; set; } = "object";
    public SummarySchemaProperties properties { get; set; } = new();
    public string[] required { get; set; } = new[]
    {
        "arbeitgeber", "taetigkeit", "beginn", "dauer", "gehalt_brutto", "zuschlaege",
        "arbeitsstunden", "ueberstunden", "probezeit"
    };
}

public class SummarySchemaProperties
{
    public SimpleSchema arbeitgeber { get; set; } = new() { type = "string" };
    public SimpleSchema taetigkeit { get; set; } = new() { type = "string" };
    public SimpleSchema beginn { get; set; } = new() { type = "string" };
    public SimpleSchema dauer { get; set; } = new() { type = "string" };
    public SimpleSchema gehalt_brutto { get; set; } = new() { type = "string" };
    public SimpleSchema zuschlaege { get; set; } = new() { type = "string" };
    public SimpleSchema arbeitsstunden { get; set; } = new() { type = "string" };
    public SimpleSchema ueberstunden { get; set; } = new() { type = "string" };
    public SimpleSchema probezeit { get; set; } = new() { type = "string" };
}

public class ArraySchema<T> where T : new()
{
    public string type { get; set; } = "array";
    public T items { get; set; } = new();
}

public class AnalyzerSchema
{
    public string type { get; set; } = "object";
    public AnalyzerSchemaProperties properties { get; set; } = new();
    public string[] required { get; set; } = new[]
    {
        "summary", "unzulaessige_klauseln", "nachteile_klauseln",
        "widerspruechliche_klauseln", "vorteilhafte_klauseln", "fazit"
    };
}

public class AnalyzerSchemaProperties
{
    public SummarySchema summary { get; set; } = new();
    public ArraySchema<ClauseInfo> unzulaessige_klauseln { get; set; } = new();
    public ArraySchema<ClauseInfo> nachteile_klauseln { get; set; } = new();
    public ArraySchema<ConflictingClauseInfo> widerspruechliche_klauseln { get; set; } = new();
    public ArraySchema<AdvantageClauseInfo> vorteilhafte_klauseln { get; set; } = new();
    public SimpleSchema fazit { get; set; } = new() { type = "string" };
}

public class PreAnalyzerSchema
{
    public string type { get; set; } = "object";
    public PreAnalyzerProperties properties { get; set; } = new();
    public string[] required { get; set; } = new[] { "isReadable", "result" };
}

public class PreAnalyzerProperties
{
    public SimpleSchema isReadable { get; set; } = new() { type = "boolean" };
    public SimpleSchema isContract { get; set; } = new() { type = "boolean" };
    public SimpleSchema result { get; set; } = new() { type = "string" };
}
