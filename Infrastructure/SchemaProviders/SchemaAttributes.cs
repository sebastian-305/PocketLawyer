namespace PocketLawyer.Infrastructure.SchemaProviders;

/// <summary>
/// Attribute for defining display metadata on DTO properties.
/// Used to generate JSON schema with x-displayName, x-styling, and standard fields.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SchemaDisplayAttribute : Attribute
{
    /// <summary>
    /// Display name for the property (used in x-displayName).
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Description of the property (used in description field for AI).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// JSON Schema format (e.g., "date", "email", "uri").
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Icon name for frontend display (used in x-styling).
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// CSS class for frontend display (used in x-styling).
    /// </summary>
    public string? DisplayClass { get; set; }

    /// <summary>
    /// Importance level: "high", "medium", "low" (used in x-styling).
    /// </summary>
    public string? Importance { get; set; }

    /// <summary>
    /// Custom display format for frontend (e.g., "currency") (used in x-styling.format).
    /// This is different from the standard Format property which is for JSON Schema validation.
    /// </summary>
    public string? StylingFormat { get; set; }

    /// <summary>
    /// Maximum length for string properties.
    /// </summary>
    public int MaxLength { get; set; }

    /// <summary>
    /// Minimum length for string properties.
    /// </summary>
    public int MinLength { get; set; }

    /// <summary>
    /// Example values for the property.
    /// </summary>
    public string[]? Examples { get; set; }

    /// <summary>
    /// Enum values for string properties (used in enum field for AI validation).
    /// Allows specifying enum constraints without creating C# enum types.
    /// </summary>
    public string[]? Enum { get; set; }

    public SchemaDisplayAttribute(string displayName)
    {
        DisplayName = displayName;
    }
}

/// <summary>
/// Attribute for defining section title on DTO classes.
/// Used to generate x-sectionTitle in JSON schema.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class SchemaSectionAttribute : Attribute
{
    /// <summary>
    /// Section title for the schema (used in x-sectionTitle).
    /// </summary>
    public string SectionTitle { get; }

    public SchemaSectionAttribute(string sectionTitle)
    {
        SectionTitle = sectionTitle;
    }
}
