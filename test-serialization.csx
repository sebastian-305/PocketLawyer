using System;
using System.Text.Json;
using System.Text.Json.Serialization;

// Simulate the DTO structure
public interface IAnalysisDto
{
    string ContractType { get; }
    string Tier { get; }
}

public record RentalBasicAnalysisDto : IAnalysisDto
{
    [JsonPropertyName("allgemeineVertragsdaten")]
    public required string AllgemeineVertragsdaten { get; init; }
    
    [JsonPropertyName("mietkonditionen")]
    public required string Mietkonditionen { get; init; }
    
    [JsonIgnore]
    public string ContractType => "Mietvertrag";
    
    [JsonIgnore]
    public string Tier => "Basic";
}

public record AnalysisDto<TResult>(
    TResult Result,
    string Metadata
) where TResult : IAnalysisDto;

// Test serialization
var basicDto = new RentalBasicAnalysisDto
{
    AllgemeineVertragsdaten = "Test Data 1",
    Mietkonditionen = "Test Data 2"
};

var wrapper = new AnalysisDto<IAnalysisDto>(basicDto, "metadata");

// Test 1: Default serialization
Console.WriteLine("Test 1 - Default serialization:");
var json1 = JsonSerializer.Serialize(wrapper);
Console.WriteLine(json1);
Console.WriteLine();

// Test 2: With GetType()
Console.WriteLine("Test 2 - With GetType():");
var options = new JsonSerializerOptions
{
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false
};
var json2 = JsonSerializer.Serialize(wrapper, wrapper.GetType(), options);
Console.WriteLine(json2);
Console.WriteLine();

// Test 3: Serialize the Result directly
Console.WriteLine("Test 3 - Serialize Result directly:");
var json3 = JsonSerializer.Serialize(wrapper.Result, wrapper.Result.GetType(), options);
Console.WriteLine(json3);
