#!/usr/bin/env dotnet-script
#r "bin/Debug/net8.0/PocketLawyer.dll"

using PocketLawyer.Infrastructure.SchemaProviders.Rental.Basic;
using System.Text.Json;

var provider = new RentalBasicSummarySchemaProvider();
var schema = provider.GetSchema();

Console.WriteLine("Generated Schema:");
Console.WriteLine(JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true }));
