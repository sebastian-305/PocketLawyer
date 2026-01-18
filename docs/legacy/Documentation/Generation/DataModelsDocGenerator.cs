using PocketLawyer.Documentation.Extraction;
using System.Text;

namespace PocketLawyer.Documentation.Generation;

/// <summary>
/// Generates DATA_MODELS.md documentation from extracted DTO information.
/// Includes DTO hierarchy, properties, inheritance, and enum values.
/// Uses German language strings for all documentation.
/// </summary>
/// <remarks>
/// Validates Requirements: 3.1, 3.2, 3.3, 3.4, 3.5
/// </remarks>
public class DataModelsDocGenerator
{
    private readonly MarkdownGenerator _markdown;
    private readonly DocumentationLanguage _lang;

    public DataModelsDocGenerator()
    {
        _markdown = new MarkdownGenerator();
        _lang = DocumentationLanguage.German;
    }

    /// <summary>
    /// Generates complete DATA_MODELS.md documentation from extracted DTOs.
    /// Requirement 3.1: WHEN documenting DTOs, THE System SHALL read all files in Domain/Dtos/ directory
    /// </summary>
    /// <param name="dtos">List of extracted DTOs from ImprovedDtoExtractor</param>
    /// <param name="projectRoot">Root directory of the project for file references</param>
    /// <returns>Complete DATA_MODELS.md content</returns>
    public string GenerateDataModelsDocumentation(IEnumerable<DtoInfo> dtos, string projectRoot = "")
    {
        _markdown.Clear();

        var dtoList = dtos.ToList();

        // Header
        _markdown.H1(_lang.DataModelsTitle);
        _markdown.Paragraph("Dieses Dokument beschreibt alle Data Transfer Objects (DTOs) im PocketLawyer-System. " +
                          "Alle Informationen werden aus dem tatsächlichen Quellcode im Domain/Dtos/ Verzeichnis extrahiert.");

        // Table of Contents
        GenerateTableOfContents();

        // DTOs (Classes and Records)
        GenerateDtosSection(dtoList.Where(d => !d.IsEnum).ToList(), projectRoot);

        // Enumerations
        GenerateEnumerationsSection(dtoList.Where(d => d.IsEnum).ToList(), projectRoot);

        return _markdown.Content;
    }

    private void GenerateTableOfContents()
    {
        _markdown.H2("Inhaltsverzeichnis");
        _markdown.UnorderedList(new[]
        {
            MarkdownGenerator.AnchorLink("DTOs", "dtos"),
            MarkdownGenerator.AnchorLink(_lang.DataModelsEnums, AnchorGenerator.Generate(_lang.DataModelsEnums))
        });
    }

    /// <summary>
    /// Generates the DTOs section with all classes and records.
    /// Requirement 3.5: Each DTO is documented exactly once
    /// </summary>
    private void GenerateDtosSection(List<DtoInfo> dtos, string projectRoot)
    {
        _markdown.H2("DTOs");
        _markdown.Paragraph("Data Transfer Objects für die Vertragsanalyse.");

        if (!dtos.Any())
        {
            _markdown.Paragraph(MarkdownGenerator.Italic("Keine DTOs gefunden."));
            return;
        }

        // Sort DTOs alphabetically by name
        var sortedDtos = dtos.OrderBy(d => d.Name).ToList();

        foreach (var dto in sortedDtos)
        {
            GenerateDtoDocumentation(dto, projectRoot);
        }
    }

    /// <summary>
    /// Generates documentation for a single DTO.
    /// Requirement 3.2: Extract actual property names, types, and attributes from source code
    /// Requirement 3.3: Analyze class declarations to determine actual inheritance relationships
    /// </summary>
    private void GenerateDtoDocumentation(DtoInfo dto, string projectRoot)
    {
        // DTO header with anchor
        var anchor = AnchorGenerator.Generate(dto.Name);
        _markdown.H3($"{dto.Name}");

        // File reference
        var relativePath = string.IsNullOrEmpty(projectRoot)
            ? dto.FilePath
            : Path.GetRelativePath(projectRoot, dto.FilePath).Replace("\\", "/");
        _markdown.Paragraph($"**{_lang.Source}:** {MarkdownGenerator.FileReference(relativePath)}");

        // Namespace
        _markdown.Paragraph($"**{_lang.Namespace}:** {MarkdownGenerator.InlineCode(dto.Namespace)}");

        // Type (Class or Record)
        var typeLabel = dto.IsRecord ? _lang.Record : _lang.Class;
        _markdown.Paragraph($"**{_lang.Type}:** {typeLabel}");

        // Description
        if (!string.IsNullOrEmpty(dto.Description))
        {
            _markdown.Paragraph($"**{_lang.Description}:** {dto.Description}");
        }

        // Properties
        if (dto.Properties.Any())
        {
            GeneratePropertiesSection(dto.Properties);
        }

        _markdown.BlankLine();
    }

    /// <summary>
    /// Generates the properties table for a DTO.
    /// Requirement 3.1: Document all properties with name, type, description, and required status
    /// </summary>
    private void GeneratePropertiesSection(List<DtoPropertyInfo> properties)
    {
        _markdown.H4(_lang.Properties);

        var propertyRows = properties.Select(p =>
        {
            var requiredText = p.IsRequired ? _lang.Required : _lang.Optional;
            var inheritedMark = p.IsInherited ? " (geerbt)" : "";
            var description = string.IsNullOrEmpty(p.Description) ? "-" : p.Description;

            return new[] 
            { 
                p.Name + inheritedMark, 
                p.Type, 
                description,
                requiredText 
            };
        });

        _markdown.Table(
            new[] { _lang.Name, _lang.Type, _lang.Description, _lang.Status },
            propertyRows
        );
    }

    /// <summary>
    /// Generates the Enumerations section.
    /// Requirement 7.2: Document all enum values with their numeric values
    /// </summary>
    private void GenerateEnumerationsSection(List<DtoInfo> enums, string projectRoot)
    {
        _markdown.H2(_lang.DataModelsEnums);
        _markdown.Paragraph("Enumerationstypen, die in DTOs verwendet werden.");

        if (!enums.Any())
        {
            _markdown.Paragraph(MarkdownGenerator.Italic("Keine Enumerationen gefunden."));
            return;
        }

        // Sort enums alphabetically by name
        var sortedEnums = enums.OrderBy(e => e.Name).ToList();

        foreach (var enumInfo in sortedEnums)
        {
            GenerateEnumDocumentation(enumInfo, projectRoot);
        }
    }

    /// <summary>
    /// Generates documentation for a single enumeration.
    /// Requirement 7.1: Extract actual enum values from source files
    /// </summary>
    private void GenerateEnumDocumentation(DtoInfo enumInfo, string projectRoot)
    {
        // Enum header with anchor
        var anchor = AnchorGenerator.Generate(enumInfo.Name);
        _markdown.H3($"{enumInfo.Name}");

        // File reference
        var relativePath = string.IsNullOrEmpty(projectRoot)
            ? enumInfo.FilePath
            : Path.GetRelativePath(projectRoot, enumInfo.FilePath).Replace("\\", "/");
        _markdown.Paragraph($"**{_lang.Source}:** {MarkdownGenerator.FileReference(relativePath)}");

        // Namespace
        _markdown.Paragraph($"**{_lang.Namespace}:** {MarkdownGenerator.InlineCode(enumInfo.Namespace)}");

        // Type
        _markdown.Paragraph($"**{_lang.Type}:** {_lang.Enum}");

        // Description
        if (!string.IsNullOrEmpty(enumInfo.Description))
        {
            _markdown.Paragraph($"**{_lang.Description}:** {enumInfo.Description}");
        }

        // Enum values
        if (enumInfo.EnumValues.Any())
        {
            GenerateEnumValuesSection(enumInfo.EnumValues);
        }

        _markdown.BlankLine();
    }

    /// <summary>
    /// Generates the enum values table.
    /// </summary>
    private void GenerateEnumValuesSection(List<EnumValue> enumValues)
    {
        _markdown.H4(_lang.Values);

        var valueRows = enumValues.Select(v =>
        {
            var description = string.IsNullOrEmpty(v.Description) ? "-" : v.Description;
            return new[] { v.Name, v.Value.ToString(), description };
        });

        _markdown.Table(
            new[] { _lang.Name, _lang.Value, _lang.Description },
            valueRows
        );
    }

    /// <summary>
    /// Writes the generated documentation to a file.
    /// </summary>
    public void WriteToFile(string filePath)
    {
        _markdown.WriteToFile(filePath);
    }
}
