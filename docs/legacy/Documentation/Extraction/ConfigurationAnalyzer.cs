using System.Text.Json;

namespace PocketLawyer.Documentation.Extraction;

/// <summary>
/// Analyzes configuration files (appsettings.json) and option classes to extract
/// configuration structure, keys, values, and mappings between JSON keys and option properties.
/// </summary>
/// <remarks>
/// Validates Requirements: 4.1, 4.2, 4.3, 4.4
/// </remarks>
public class ConfigurationAnalyzer
{
    private readonly SourceFileAnalyzer _sourceFileAnalyzer;

    public ConfigurationAnalyzer()
    {
        _sourceFileAnalyzer = new SourceFileAnalyzer();
    }

    /// <summary>
    /// Analyzes a JSON configuration file and extracts its structure.
    /// </summary>
    /// <param name="filePath">Path to the appsettings.json file</param>
    /// <returns>Configuration information extracted from the file</returns>
    public ConfigurationFileInfo AnalyzeConfigurationFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Configuration file not found: {filePath}");
        }

        var jsonContent = File.ReadAllText(filePath);
        return AnalyzeConfigurationJson(jsonContent, filePath);
    }

    /// <summary>
    /// Analyzes JSON configuration content and extracts its structure.
    /// </summary>
    /// <param name="jsonContent">The JSON content to analyze</param>
    /// <param name="filePath">Optional file path for reference</param>
    /// <returns>Configuration information extracted from the JSON</returns>
    public ConfigurationFileInfo AnalyzeConfigurationJson(string jsonContent, string? filePath = null)
    {
        var configInfo = new ConfigurationFileInfo
        {
            Path = filePath ?? string.Empty
        };

        try
        {
            // Strip JSON comments (// style) before parsing
            var cleanedJson = StripJsonComments(jsonContent);
            
            using var document = JsonDocument.Parse(cleanedJson);
            var root = document.RootElement;

            configInfo.Sections = ExtractConfigurationSections(root, string.Empty);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse JSON configuration: {ex.Message}", ex);
        }

        return configInfo;
    }

    /// <summary>
    /// Strips single-line comments (//) from JSON content.
    /// This is needed because appsettings.json files often contain comments which are not valid JSON.
    /// </summary>
    /// <param name="jsonContent">JSON content with comments</param>
    /// <returns>JSON content without comments</returns>
    private string StripJsonComments(string jsonContent)
    {
        var lines = jsonContent.Split('\n');
        var cleanedLines = new List<string>();

        foreach (var line in lines)
        {
            // Find the position of // comment
            var commentIndex = line.IndexOf("//");
            
            if (commentIndex >= 0)
            {
                // Check if // is inside a string by counting quotes before it
                var beforeComment = line.Substring(0, commentIndex);
                var quoteCount = beforeComment.Count(c => c == '"' && (beforeComment.IndexOf(c) == 0 || beforeComment[beforeComment.IndexOf(c) - 1] != '\\'));
                
                // If even number of quotes, the // is outside a string (a comment)
                if (quoteCount % 2 == 0)
                {
                    cleanedLines.Add(line.Substring(0, commentIndex).TrimEnd());
                }
                else
                {
                    // Odd number of quotes means // is inside a string, keep the line
                    cleanedLines.Add(line);
                }
            }
            else
            {
                cleanedLines.Add(line);
            }
        }

        return string.Join('\n', cleanedLines);
    }

    /// <summary>
    /// Analyzes an option class file and extracts its properties.
    /// </summary>
    /// <param name="filePath">Path to the option class C# file</param>
    /// <returns>Option class information</returns>
    public OptionClassInfo AnalyzeOptionClass(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Option class file not found: {filePath}");
        }

        var sourceInfo = _sourceFileAnalyzer.AnalyzeFile(filePath);
        
        // Get the first class (option classes typically have one class per file)
        var classInfo = sourceInfo.Classes.FirstOrDefault();
        
        if (classInfo == null)
        {
            throw new InvalidOperationException($"No class found in option file: {filePath}");
        }

        return new OptionClassInfo
        {
            Name = classInfo.Name,
            Namespace = classInfo.Namespace,
            Path = filePath,
            Properties = classInfo.Properties.Select(p => new OptionPropertyInfo
            {
                Name = p.Name,
                Type = p.Type,
                DefaultValue = ExtractDefaultValue(p),
                Attributes = p.Attributes
            }).ToList()
        };
    }

    /// <summary>
    /// Maps configuration keys from JSON to option class properties.
    /// </summary>
    /// <param name="configSection">Configuration section name (e.g., "Paths", "Upload")</param>
    /// <param name="configInfo">Configuration file information</param>
    /// <param name="optionClassInfo">Option class information</param>
    /// <returns>Mapping between configuration keys and option properties</returns>
    public ConfigurationMapping MapConfigurationToOptions(
        string configSection,
        ConfigurationFileInfo configInfo,
        OptionClassInfo optionClassInfo)
    {
        var mapping = new ConfigurationMapping
        {
            SectionName = configSection,
            OptionClassName = optionClassInfo.Name
        };

        // Find the configuration section
        var section = FindSection(configInfo.Sections, configSection);
        if (section == null)
        {
            return mapping; // Return empty mapping if section not found
        }

        // Map each configuration key to option properties
        foreach (var key in section.Keys)
        {
            var matchingProperty = optionClassInfo.Properties
                .FirstOrDefault(p => p.Name.Equals(key.Key, StringComparison.OrdinalIgnoreCase));

            if (matchingProperty != null)
            {
                mapping.PropertyMappings.Add(new PropertyMapping
                {
                    ConfigurationKey = $"{configSection}:{key.Key}",
                    PropertyName = matchingProperty.Name,
                    PropertyType = matchingProperty.Type,
                    ConfigurationValue = key.Value,
                    HasMatch = true
                });
            }
            else
            {
                // Configuration key exists but no matching property
                mapping.PropertyMappings.Add(new PropertyMapping
                {
                    ConfigurationKey = $"{configSection}:{key.Key}",
                    PropertyName = null,
                    PropertyType = null,
                    ConfigurationValue = key.Value,
                    HasMatch = false
                });
            }
        }

        // Check for properties that don't have configuration values
        foreach (var property in optionClassInfo.Properties)
        {
            var hasConfigKey = section.Keys.Any(k => 
                k.Key.Equals(property.Name, StringComparison.OrdinalIgnoreCase));

            if (!hasConfigKey)
            {
                mapping.PropertyMappings.Add(new PropertyMapping
                {
                    ConfigurationKey = $"{configSection}:{property.Name}",
                    PropertyName = property.Name,
                    PropertyType = property.Type,
                    ConfigurationValue = property.DefaultValue,
                    HasMatch = false,
                    UsesDefaultValue = true
                });
            }
        }

        return mapping;
    }

    private List<ConfigurationSection> ExtractConfigurationSections(JsonElement element, string parentPath)
    {
        var sections = new List<ConfigurationSection>();

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var currentPath = string.IsNullOrEmpty(parentPath) 
                    ? property.Name 
                    : $"{parentPath}:{property.Name}";

                if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    // This is a section with nested properties
                    var section = new ConfigurationSection
                    {
                        Name = property.Name,
                        Path = currentPath
                    };

                    // Extract immediate keys
                    foreach (var subProperty in property.Value.EnumerateObject())
                    {
                        if (subProperty.Value.ValueKind != JsonValueKind.Object)
                        {
                            section.Keys.Add(new ConfigurationKey
                            {
                                Key = subProperty.Name,
                                Value = GetJsonValue(subProperty.Value),
                                Type = GetJsonType(subProperty.Value)
                            });
                        }
                    }

                    sections.Add(section);

                    // Recursively extract nested sections
                    sections.AddRange(ExtractConfigurationSections(property.Value, currentPath));
                }
                else
                {
                    // This is a top-level key-value pair
                    // Create or find a root section
                    var rootSection = sections.FirstOrDefault(s => s.Name == "Root");
                    if (rootSection == null)
                    {
                        rootSection = new ConfigurationSection
                        {
                            Name = "Root",
                            Path = string.Empty
                        };
                        sections.Add(rootSection);
                    }

                    rootSection.Keys.Add(new ConfigurationKey
                    {
                        Key = property.Name,
                        Value = GetJsonValue(property.Value),
                        Type = GetJsonType(property.Value)
                    });
                }
            }
        }

        return sections;
    }

    private ConfigurationSection? FindSection(List<ConfigurationSection> sections, string sectionName)
    {
        return sections.FirstOrDefault(s => 
            s.Name.Equals(sectionName, StringComparison.OrdinalIgnoreCase));
    }

    private string? GetJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => null,
            JsonValueKind.Array => element.GetRawText(),
            JsonValueKind.Object => element.GetRawText(),
            _ => element.GetRawText()
        };
    }

    private string GetJsonType(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => "string",
            JsonValueKind.Number => element.TryGetInt64(out _) ? "long" : "double",
            JsonValueKind.True or JsonValueKind.False => "bool",
            JsonValueKind.Null => "null",
            JsonValueKind.Array => "array",
            JsonValueKind.Object => "object",
            _ => "unknown"
        };
    }

    private string? ExtractDefaultValue(PropertyInfo property)
    {
        // This is a simplified extraction - in real code, we'd need to parse the initializer
        // For now, we'll return null as we can't easily extract default values from Roslyn
        // without more complex analysis
        return null;
    }
}

/// <summary>
/// Information about a configuration file.
/// </summary>
public class ConfigurationFileInfo
{
    public string Path { get; set; } = string.Empty;
    public List<ConfigurationSection> Sections { get; set; } = new();
}

/// <summary>
/// Information about a configuration section.
/// </summary>
public class ConfigurationSection
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public List<ConfigurationKey> Keys { get; set; } = new();
}

/// <summary>
/// Information about a configuration key-value pair.
/// </summary>
public class ConfigurationKey
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Information about an option class.
/// </summary>
public class OptionClassInfo
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public List<OptionPropertyInfo> Properties { get; set; } = new();
}

/// <summary>
/// Information about an option class property.
/// </summary>
public class OptionPropertyInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? DefaultValue { get; set; }
    public List<AttributeInfo> Attributes { get; set; } = new();
}

/// <summary>
/// Mapping between configuration keys and option class properties.
/// </summary>
public class ConfigurationMapping
{
    public string SectionName { get; set; } = string.Empty;
    public string OptionClassName { get; set; } = string.Empty;
    public List<PropertyMapping> PropertyMappings { get; set; } = new();
}

/// <summary>
/// Mapping between a configuration key and an option property.
/// </summary>
public class PropertyMapping
{
    public string ConfigurationKey { get; set; } = string.Empty;
    public string? PropertyName { get; set; }
    public string? PropertyType { get; set; }
    public string? ConfigurationValue { get; set; }
    public bool HasMatch { get; set; }
    public bool UsesDefaultValue { get; set; }
}
