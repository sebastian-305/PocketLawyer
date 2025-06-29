namespace PocketLawyer.Domain.Entities;

public class Contract
{
    public string FileName { get; set; } = string.Empty;
    public List<ContractClause> Clauses { get; set; } = new();
}
