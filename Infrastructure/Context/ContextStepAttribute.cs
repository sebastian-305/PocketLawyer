namespace PocketLawyer.Infrastructure.Context;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ContextStepAttribute : Attribute
{
    public string StepName { get; }
    public string? Tier { get; set; }
    
    public ContextStepAttribute(string stepName)
    {
        StepName = stepName ?? throw new ArgumentNullException(nameof(stepName));
    }
}
