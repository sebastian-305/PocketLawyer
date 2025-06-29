namespace PocketLawyer.Domain.Entities;

public class AnalysisResult
{
    public string Summary { get; set; } = string.Empty;
    public List<ContractClause> ProblematicClauses { get; set; } = new();
}
