#!/usr/bin/env dotnet-script
#r "nuget: System.Text.Json, 8.0.0"

using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Reflection;

public record TestDtoWithRequired(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("optional")] string? Optional
);

var type = typeof(TestDtoWithRequired);
foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
{
    Console.WriteLine($"Property: {prop.Name}");
    Console.WriteLine($"  Type: {prop.PropertyType}");
    Console.WriteLine($"  IsValueType: {prop.PropertyType.IsValueType}");
    Console.WriteLine($"  Nullable underlying: {Nullable.GetUnderlyingType(prop.PropertyType)}");
    
    var nullabilityContext = new NullabilityInfoContext();
    var nullabilityInfo = nullabilityContext.Create(prop);
    Console.WriteLine($"  ReadState: {nullabilityInfo.ReadState}");
    Console.WriteLine($"  WriteState: {nullabilityInfo.WriteState}");
    Console.WriteLine();
}
