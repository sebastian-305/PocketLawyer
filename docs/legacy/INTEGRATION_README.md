# Integration Tests

This directory contains integration tests for the PocketLawyer agent refactoring system.

## Test Coverage

### AgentIntegrationTests.cs
Comprehensive integration tests for the complete agent workflow:

#### Rental Contract Tests
- **Complete Analysis Flow**: Tests the full workflow from initialization through basic and premium analysis
- **Invalid File Handling**: Verifies that unreadable contracts fail initialization gracefully
- **Wrong Contract Type**: Ensures contracts of the wrong type are rejected during initialization
- **Uninitialized Agent**: Confirms that analysis methods throw exceptions when called before initialization
- **Parallel Step Execution**: Validates that all analysis steps execute in parallel using Task.WhenAll
- **Failed Step Handling**: Tests that when one step fails, others continue and error messages are returned
- **Premium Independence**: Verifies that premium analysis can run independently of basic analysis

#### Employment Contract Tests
- **Initialization**: Tests successful initialization of employment contract agents
- **Stub Implementation**: Verifies that analysis methods throw NotImplementedException (skeleton implementation)

#### Error Scenario Tests
- **AI Service Failures**: Tests graceful handling of AI service exceptions
- **Empty Contract Text**: Validates rejection of empty or whitespace-only contracts

### SchemaEndpointIntegrationTests.cs
Integration tests for the schema retrieval HTTP endpoint:

#### Schema Retrieval Tests
- **All Rental Schemas**: Tests retrieval of all basic and premium rental contract schemas
- **Invalid Schema Keys**: Verifies 404 response for non-existent schemas
- **Schema Structure**: Validates that all schemas contain required fields (type, properties, required)
- **Styling Metadata**: Confirms presence of x-styling metadata in schemas
- **Premium Consistency**: Ensures all premium sections have consistent structure (zusammenfassung, bewertung, rechtsgrundlage, risikoScore, riskLevel)

#### Logging Tests
- **Request Logging**: Verifies that schema requests are logged at Info level
- **Error Logging**: Confirms that invalid schema requests are logged as warnings

## WebSocket Communication

WebSocket communication testing is covered indirectly through the agent integration tests. The WebSocket handler's behavior is validated through its interaction with agents:

- **Message Flow**: Agents send confirmation, basic results, and premium results
- **Error Handling**: Agents handle initialization failures, validation errors, and analysis exceptions
- **Connection Lifecycle**: Proper closure with appropriate status codes

Direct WebSocket message testing would require complex HttpContext mocking that is not practical in unit tests. The current approach validates the core functionality through agent behavior.

## Running the Tests

Run all integration tests:
```bash
dotnet test --filter "FullyQualifiedName~Integration"
```

Run specific test class:
```bash
dotnet test --filter "FullyQualifiedName~AgentIntegrationTests"
dotnet test --filter "FullyQualifiedName~SchemaEndpointIntegrationTests"
```

## Test Requirements Mapping

These integration tests validate the following requirements from the spec:

- **Requirement 1**: File upload and validation
- **Requirement 2**: Agent lifecycle (construction, initialization, analysis)
- **Requirement 3**: Agent initialization and contract validation
- **Requirement 4**: Basic analysis workflow
- **Requirement 5**: Premium analysis workflow
- **Requirement 6**: Schema provider and validator
- **Requirement 7**: Multiple contract type support
- **Requirement 8**: Analysis DTO structure
- **Requirement 10**: Parallel step execution
- **Requirement 11**: WebSocket communication (indirect)
- **Requirement 12**: Prompt management (indirect)

## Notes

- All tests use mocked AI clients to avoid external dependencies
- Tests validate both success and failure scenarios
- Error messages are checked for user-friendly content
- Parallel execution is verified through timing analysis
- Schema validation ensures Single-Source-of-Truth principle
