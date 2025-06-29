# Design Patterns Overview

This document identifies design patterns and principles used in the PocketLawyer project. Each section lists where the pattern appears in the code and explains its usefulness.

## Software Design Principles

### Dependency Injection
* **Implementation:** Dependency injection is configured in `Program.cs` using `builder.Services` to register services and keyed implementations. Example registrations:
  ```csharp
  builder.Services.AddKeyedScoped<IAiService, GeminiService>("gemini");
  builder.Services.AddKeyedScoped<IAiService, ClaudeService>("claude");
  builder.Services.AddScoped<IFileDecoderService, FileDecoderService>();
  ```
  These registrations allow controllers and services to depend on abstractions (`IAiService`, `IFileDecoderService`, etc.) rather than concrete classes.
* **Benefit:** Promotes loose coupling and adheres to the Dependency Inversion Principle by allowing implementations to be swapped or mocked during testing.

### SOLID & DRY
* **Implementation:** The project defines interfaces (`IAiService`, `IFileDecoder`, `IKvDetectionService`, etc.) with focused responsibilities, and service classes implement them. This separation follows the Single Responsibility and Interface Segregation principles. Common steps such as validation and subsequent processing now occur in `WebSocketAnalysisService` and other dedicated services rather than in controller helpers.
* **Benefit:** Clear interfaces prevent tight coupling and avoid repeating decoding or validation logic, satisfying the DRY principle.

### GRASP Principles
* **Information Expert & Controller:** `WebSocketAnalysisService` coordinates decoding, KV detection and AI calls. It delegates specialized tasks to other services, aligning with the Controller and Information Expert roles.
* **Low Coupling:** The services depend on abstractions. `PocketController` only relies on interfaces for its operations.

### YAGNI
* **Observation:** The code tends to implement only the features currently required (e.g., limited file decoder types). There are no unused generalizations, showing adherence to the "You Aren't Gonna Need It" mindset.

## Domain Driven Design

### Anemic Domain Model
* **Evidence:** DTO classes in `Dtos/` contain data without behavior, e.g. `ContractAnalysisDto` and `ContractValidationDto`. Domain logic resides mostly in services rather than the DTOs themselves.
* **Impact:** This simplifies serialization and transport but concentrates business logic in service classes.

### Value Objects
* **Evidence:** `FileContentDto` is an immutable record storing a file name and its content.
* **Benefit:** Treating small data structures as value objects improves clarity when passing data between layers.

### Smart UI
* **Observation:** The JavaScript front‑end in `wwwroot` performs rendering and interacts with the server through WebSockets. The server delegates heavy analysis to the backend services, so the UI remains "smart" but thin.

## Structural Patterns

### Facade
* **Implementation:** `ContractAnalyzerService` acts as a facade that hides the complexity of file decoding, Kollektivvertrag detection, and AI service calls. Consumers only call `ProcessContractRequest` without dealing with the underlying steps.
* **Benefit:** Simplifies controller logic and offers a single entry point for contract analysis. This keeps the controller lean and encourages reuse.
* **Alternative:** The controller could orchestrate each step itself, but that would make the code harder to test and maintain.

### Adapter
* **Implementation:** The file decoding strategy classes (`TextFileDecoder`, `PdfFileDecoder`, `ImageFileDecoder`) adapt different libraries (plain text reader, PdfPig, Tesseract) to the common `IFileDecoder` interface.
* **Benefit:** Allows `FileDecoderService` to treat disparate decoding mechanisms uniformly and extend them easily.
* **Alternative:** The service could invoke each library directly. This would duplicate code and create tight coupling to specific libraries.


## Behavioral Patterns

### Strategy
* **Implementation:** `FileDecoderService` chooses a decoding strategy at runtime from registered `IFileDecoder` implementations based on file extension.
  ```csharp
  var decoder = _decoders.FirstOrDefault(d => d.CanDecode(extension));
  if (decoder == null)
      throw new NotSupportedException("Unknown file extension: " + extension);
  return await decoder.DecodeAsync(content);
  ```
* **Benefit:** Makes it easy to extend support for new file types by providing another `IFileDecoder` implementation without modifying existing code. Each decoder is isolated from the others.
* **Alternative:** A large `switch` statement could handle each extension directly in the service, but this would grow complex and require changes for every new file type.

## Summary
PocketLawyer relies heavily on dependency injection and interface-based design, applying SOLID and GRASP principles. The domain model is mostly anemic, with simple DTOs representing data. Structural patterns such as Facade and Adapter simplify subsystem interactions, while the Strategy pattern is used for file decoding.
