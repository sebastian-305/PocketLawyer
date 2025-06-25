namespace PocketLawyer;

public class ContractValidationDto
{
    public bool isReadable { get; set; }
    public bool isContract { get; set; }
    public string result { get; set; } = string.Empty;
}
