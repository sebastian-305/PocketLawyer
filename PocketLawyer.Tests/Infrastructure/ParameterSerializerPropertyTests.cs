using FsCheck;
using FsCheck.Xunit;
using PocketLawyer.Infrastructure.Logging;

namespace PocketLawyer.Tests.Infrastructure;

/// <summary>
/// Property-based tests for ParameterSerializer.
/// Feature: method-tracing
/// </summary>
public class ParameterSerializerPropertyTests
{
    private readonly ParameterSerializer _serializer = new();

    /// <summary>
    /// **Feature: method-tracing, Property 1: Sensitive parameter redaction**
    /// For any parameter name containing "password", "secret", "token", or "key" (case-insensitive), 
    /// the serialized value should be "[REDACTED]"
    /// **Validates: Requirements 7.1, 7.2, 7.3, 7.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property SensitiveParameterRedaction_RedactsValue()
    {
        // Generate parameter names containing sensitive keywords
        var sensitiveKeywords = new[] { "password", "secret", "token", "key", "apikey", "api_key" };
        var prefixes = new[] { "", "user", "my", "the" };
        var suffixes = new[] { "", "Value", "123", "Data" };
        
        var sensitiveNameGen = from keyword in Gen.Elements(sensitiveKeywords)
                               from prefix in Gen.Elements(prefixes)
                               from suffix in Gen.Elements(suffixes)
                               from casing in Gen.Elements(0, 1, 2) // 0=lower, 1=upper, 2=mixed
                               select ApplyCasing(prefix + keyword + suffix, casing);
        
        var valueGen = Gen.OneOf(
            Gen.Constant<object?>("sensitive-value"),
            Gen.Constant<object?>(12345),
            Gen.Constant<object?>(true),
            Gen.Constant<object?>(null)
        );

        return Prop.ForAll(
            sensitiveNameGen.ToArbitrary(),
            valueGen.ToArbitrary(),
            (paramName, value) =>
            {
                // Act
                var result = _serializer.Serialize(paramName, value);
                
                // Assert
                return result == "[REDACTED]";
            });
    }

    /// <summary>
    /// **Feature: method-tracing, Property 2: String truncation consistency**
    /// For any string longer than 200 characters, the serialized value should contain 
    /// exactly the first 200 characters followed by "..." and the total length
    /// **Validates: Requirements 2.1**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property StringTruncation_TruncatesLongStrings()
    {
        // Generate strings longer than 200 characters
        var longStringGen = from length in Gen.Choose(201, 1000)
                           from str in Gen.ArrayOf(length, Gen.Elements("abcdefghijklmnopqrstuvwxyz0123456789 ".ToCharArray()))
                           select new string(str);

        return Prop.ForAll(
            longStringGen.ToArbitrary(),
            longString =>
            {
                // Act
                var result = _serializer.Serialize("testParam", longString);
                
                // Assert
                var expectedPrefix = $"\"{longString.Substring(0, 200)}...\"";
                var expectedSuffix = $"(truncated, total: {longString.Length} chars)";
                
                return result.StartsWith(expectedPrefix) && 
                       result.Contains(expectedSuffix);
            });
    }

    /// <summary>
    /// **Feature: method-tracing, Property 3: Byte array serialization format**
    /// For any byte array, the serialized value should be in the format "byte[{length}]" 
    /// where length is the array length
    /// **Validates: Requirements 2.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ByteArraySerialization_ShowsLength()
    {
        // Generate byte arrays of various sizes
        var byteArrayGen = from length in Gen.Choose(0, 1000)
                          from bytes in Gen.ArrayOf(length, Arb.Generate<byte>())
                          select bytes;

        return Prop.ForAll(
            byteArrayGen.ToArbitrary(),
            byteArray =>
            {
                // Act
                var result = _serializer.Serialize("testParam", byteArray);
                
                // Assert
                var expected = $"byte[{byteArray.Length}]";
                return result == expected;
            });
    }

    /// <summary>
    /// **Feature: method-tracing, Property 4: Null handling**
    /// For any null parameter value, the serialized value should be the string "null"
    /// **Validates: Requirements 2.5**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property NullHandling_ReturnsNullString()
    {
        // Generate various parameter names
        var paramNameGen = Gen.Elements("param", "value", "data", "input", "test");

        return Prop.ForAll(
            paramNameGen.ToArbitrary(),
            paramName =>
            {
                // Act
                var result = _serializer.Serialize(paramName, null);
                
                // Assert
                return result == "null";
            });
    }

    /// <summary>
    /// Property test for primitive serialization.
    /// Tests int, long, double, decimal, bool, DateTime, DateTimeOffset
    /// **Validates: Requirements 2.6**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property PrimitiveSerialization_ReturnsToString()
    {
        var primitiveGen = Gen.OneOf(
            Arb.Generate<int>().Select(x => (object)x),
            Arb.Generate<long>().Select(x => (object)x),
            Arb.Generate<double>().Select(x => (object)x),
            Arb.Generate<decimal>().Select(x => (object)x),
            Arb.Generate<bool>().Select(x => (object)x),
            Arb.Generate<DateTime>().Select(x => (object)x),
            Arb.Generate<DateTimeOffset>().Select(x => (object)x)
        );

        return Prop.ForAll(
            primitiveGen.ToArbitrary(),
            primitive =>
            {
                // Act
                var result = _serializer.Serialize("testParam", primitive);
                
                // Assert - should match ToString() output
                var expected = primitive.ToString();
                return result == expected;
            });
    }

    private static string ApplyCasing(string input, int casingType)
    {
        return casingType switch
        {
            0 => input.ToLower(),
            1 => input.ToUpper(),
            _ => input // mixed case as-is
        };
    }
}
