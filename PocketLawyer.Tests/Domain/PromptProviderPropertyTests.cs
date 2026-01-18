using FsCheck;
using FsCheck.Xunit;
using PocketLawyer.Application.Exceptions;
using PocketLawyer.Domain;

namespace PocketLawyer.Tests.Domain;

/// <summary>
/// Property-based tests for PromptProvider.
/// Feature: agent-refactoring
/// </summary>
public class PromptProviderPropertyTests
{
    /// <summary>
    /// **Feature: agent-refactoring, Property 39: Central prompt retrieval**
    /// For any valid (contractType, tier, step) combination, GetPrompt() should return a non-empty string.
    /// **Validates: Requirements 12.1, 12.5**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property CentralPromptRetrieval_ReturnsNonEmptyString()
    {
        // Define valid prompt combinations
        var validCombinations = new[]
        {
            // Rental Basic
            ("Rental", "Basic", "General"),
            ("Rental", "Basic", "Rent"),
            ("Rental", "Basic", "Conditions"),
            ("Rental", "Basic", "Summary"),
            
            // Rental Premium
            ("Rental", "Premium", "Costs"),
            ("Rental", "Premium", "Terms"),
            ("Rental", "Premium", "Use"),
            ("Rental", "Premium", "Maintenance"),
            ("Rental", "Premium", "Duties"),
            ("Rental", "Premium", "Privacy"),
            ("Rental", "Premium", "Summary"),
            
            // Employment Basic (placeholder)
            ("Employment", "Basic", "General"),
            ("Employment", "Basic", "Compensation"),
            ("Employment", "Basic", "WorkingConditions"),
            ("Employment", "Basic", "Summary")
        };

        return Prop.ForAll(
            Gen.Elements(validCombinations).ToArbitrary(),
            combination =>
            {
                var (contractType, tier, step) = combination;
                
                // Act
                var prompt = PromptProvider.GetPrompt(contractType, tier, step);
                
                // Assert
                return !string.IsNullOrWhiteSpace(prompt);
            });
    }

    /// <summary>
    /// **Feature: agent-refactoring, Property 39: Central prompt retrieval**
    /// For any valid prompt, it should contain the system prompt.
    /// **Validates: Requirements 12.1, 12.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property PromptContainsSystemPrompt()
    {
        // Define valid prompt combinations
        var validCombinations = new[]
        {
            ("Rental", "Basic", "General"),
            ("Rental", "Basic", "Rent"),
            ("Rental", "Premium", "Costs"),
            ("Employment", "Basic", "General")
        };

        return Prop.ForAll(
            Gen.Elements(validCombinations).ToArbitrary(),
            combination =>
            {
                var (contractType, tier, step) = combination;
                
                // Act
                var prompt = PromptProvider.GetPrompt(contractType, tier, step);
                
                // Assert - System prompt should be included
                return prompt.Contains("Experte für Vertragsanalyse") &&
                       prompt.Contains("JSON-Format");
            });
    }

    /// <summary>
    /// For any invalid prompt key, GetPrompt() should throw PromptNotFoundException.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property InvalidPromptKey_ThrowsPromptNotFoundException()
    {
        // Generate random invalid combinations
        var invalidContractTypes = Gen.Elements("Invalid", "Unknown", "Test", "");
        var invalidTiers = Gen.Elements("Invalid", "Unknown", "Test", "");
        var invalidSteps = Gen.Elements("Invalid", "Unknown", "Test", "");

        return Prop.ForAll(
            invalidContractTypes.ToArbitrary(),
            invalidTiers.ToArbitrary(),
            invalidSteps.ToArbitrary(),
            (contractType, tier, step) =>
            {
                // Skip if this happens to be a valid combination
                var key = $"{contractType}{tier}{step}";
                if (IsValidPromptKey(key))
                    return true;

                // Act & Assert
                try
                {
                    PromptProvider.GetPrompt(contractType, tier, step);
                    return false; // Should have thrown
                }
                catch (PromptNotFoundException)
                {
                    return true;
                }
                catch
                {
                    return false; // Wrong exception type
                }
            });
    }

    /// <summary>
    /// For any valid prompt, it should be properly formatted with both system and step-specific content.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property PromptIsProperlyFormatted()
    {
        var validCombinations = new[]
        {
            ("Rental", "Basic", "General"),
            ("Rental", "Premium", "Costs"),
            ("Employment", "Basic", "General")
        };

        return Prop.ForAll(
            Gen.Elements(validCombinations).ToArbitrary(),
            combination =>
            {
                var (contractType, tier, step) = combination;
                
                // Act
                var prompt = PromptProvider.GetPrompt(contractType, tier, step);
                
                // Assert - Prompt should have multiple lines and reasonable length
                var lines = prompt.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                return lines.Length > 1 && prompt.Length > 50;
            });
    }

    private static bool IsValidPromptKey(string key)
    {
        var validKeys = new[]
        {
            // Rental Basic
            "RentalBasicGeneral",
            "RentalBasicRent",
            "RentalBasicConditions",
            "RentalBasicSummary",
            
            // Rental Premium
            "RentalPremiumCosts",
            "RentalPremiumTerms",
            "RentalPremiumUse",
            "RentalPremiumMaintenance",
            "RentalPremiumDuties",
            "RentalPremiumPrivacy",
            "RentalPremiumSummary",
            
            // Employment Basic
            "EmploymentBasicGeneral",
            "EmploymentBasicConditions"
        };

        return validKeys.Contains(key);
    }
}
