# Klausel Klara

**AI-Powered Contract Analysis for Austrian Employment and Rental Contracts**

Klausel Klara is a production-ready web application that analyzes Austrian employment and rental contracts using AI. Built with ASP.NET Core 8 and powered by the Nebius AI service, it provides comprehensive contract analysis with real-time results delivered via WebSocket connections.

## Project Overview

Klausel Klara helps users understand their contracts by:
- **Analyzing contract clauses** for legal compliance and fairness
- **Identifying risks** and potential issues in contract terms
- **Providing detailed explanations** in German for Austrian legal context
- **Supporting multiple file formats** (PDF, images via OCR, plain text)
- **Delivering real-time results** through a responsive web interface

The application supports two contract types:
- **Employment Contracts** (Arbeitsverträge) - Analysis of employment terms, salary, working conditions
- **Rental Contracts** (Mietverträge) - Analysis of rental terms, costs, tenant rights

## Architecture

### High-Level Design

Klausel Klara follows **Onion Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│           Web Layer (ASP.NET)           │  ← HTTP/WebSocket endpoints
├─────────────────────────────────────────┤
│      Infrastructure Layer               │  ← AI services, file processing
├─────────────────────────────────────────┤
│      Application Layer                  │  ← Business logic, agents
├─────────────────────────────────────────┤
│      Domain Layer                       │  ← DTOs, entities, core logic
└─────────────────────────────────────────┘
```

**Key Architectural Principles:**
- **Single Source of Truth**: JSON schemas define all data structures
- **Data-Driven Frontend**: UI renders dynamically based on schema metadata
- **Parallel Execution**: Analysis steps run concurrently for performance
- **Fault Tolerance**: Failed steps don't block other analyses
- **Dependency Injection**: Clean, testable component design

### Core Components

#### Contract Analysis Agents
Orchestrate the complete analysis workflow using a 3-phase lifecycle:
1. **Construction** - Lightweight creation with dependency injection
2. **Initialization** - Contract validation, type detection, text storage
3. **Analysis** - Parallel execution of Basic and Premium analysis steps

- `RentalContractAgent` - Analyzes rental contracts
- `EmploymentContractAgent` - Analyzes employment contracts

#### Schema Provider
Provides JSON schemas as the single source of truth for all data structures:
- Defines field types and validation rules
- Contains `x-displayName` metadata for German UI labels
- Contains `x-sectionTitle` metadata for section headings
- Enables fully data-driven frontend rendering

#### AI Integration
- **Nebius AI Service** - Primary AI provider for contract analysis
- **Token Tracking** - Monitors and logs AI token consumption
- **Schema Validation** - Validates AI responses against JSON schemas

#### File Processing
- **PDF Extraction** - UglyToad.PdfPig for text extraction from PDFs
- **OCR Processing** - Tesseract OCR for image-based contracts
- **File Validation** - Size and type validation (max 20MB)

### Folder Structure

```
Domain/                      # Core business logic and data structures
  Dtos/
    Employment/              # Employment contract DTOs
    Rental/                  # Rental contract DTOs
    Shared/                  # Shared DTOs and interfaces

Application/                 # Use cases and business workflows
  Agents/                    # Contract analysis agents
  Interfaces/                # Service interfaces
  Models/                    # Application models
  Exceptions/                # Custom exceptions

Infrastructure/              # External service implementations
  SchemaProviders/           # JSON schema providers
  AiServices/                # AI service integrations
  FileDecoding/              # File processing services

Web/                         # ASP.NET Core web host
  Controllers/               # HTTP controllers
  WebSockets/                # WebSocket handlers

wwwroot/                     # Static web assets
  employment.html            # Employment contract page
  rental.html                # Rental contract page
  index.html                 # Landing page
  js/                        # JavaScript modules
  css/                       # Stylesheets
  Media/                     # Images and assets

PocketLawyer.Tests/          # Comprehensive test suite
  Domain/                    # Domain layer tests
  Application/               # Application layer tests
  Infrastructure/            # Infrastructure tests
  Integration/               # End-to-end tests

data/                        # Application data
  KVS/                       # Collective agreements (Kollektivverträge)
  Context/                   # Pre-processed context data
  tessdata/                  # Tesseract OCR language data
  Samples/                   # Sample contracts for testing

docs/                        # Documentation
  API_DOCUMENTATION.md       # Complete API reference
  CONFIGURATION.md           # Configuration guide
  DesignPatterns_de.md       # Design patterns documentation
  diagrams/                  # Architecture diagrams
  legacy/                    # Archived documentation
```

## Setup Instructions

### Prerequisites

- **.NET 8 SDK** or later
- **Nebius AI API Key** (for contract analysis)
- **Git** (for cloning the repository)

### Installation Steps

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd PocketLawyer
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure API credentials**
   
   Set up your Nebius API key using user secrets (recommended):
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "Nebius:Key" "your-api-key-here"
   ```
   
   Alternatively, set environment variables or edit `appsettings.json` (not recommended for production).

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the web interface**
   
   Open your browser and navigate to: `http://localhost:5112`

### Configuration

Key configuration settings in `appsettings.json`:

```json
{
  "Nebius": {
    "Key": "your-api-key",
    "ModelId": "model-identifier",
    "BaseUrl": "https://api.nebius.ai"
  },
  "Upload": {
    "MaxSizeMB": 20,
    "AllowedExtensions": [".pdf", ".txt", ".jpg", ".png"]
  },
  "Logging": {
    "File": {
      "Enabled": true,
      "MinimumLevel": "Information",
      "Path": "logs"
    }
  }
}
```

**Security Note**: Never commit API keys to version control. Use user secrets or environment variables.

## Usage Guide

### Analyzing a Contract

1. **Navigate to the application** at `http://localhost:5112`

2. **Select contract type**
   - Click "Employment Contract" for Arbeitsverträge
   - Click "Rental Contract" for Mietverträge

3. **Upload your contract**
   - Supported formats: PDF, JPG, PNG, TXT
   - Maximum file size: 20MB
   - Click "Upload" or drag-and-drop

4. **View analysis results**
   - **Basic Analysis** appears first (5-10 seconds)
     - General contract data
     - Key conditions and terms
     - Initial risk assessment
   
   - **Premium Analysis** follows (10-20 seconds)
     - Detailed clause-by-clause analysis
     - Legal compliance checks
     - Comprehensive risk evaluation
     - Recommendations and warnings

### Analysis Workflow

The application follows a structured analysis process:

1. **File Upload & Validation**
   - File size and type validation
   - Content extraction (PDF text, OCR for images)

2. **Contract Initialization**
   - Readability verification
   - Contract type detection
   - Text storage for analysis

3. **Basic Analysis** (Parallel Execution)
   - General contract information
   - Key conditions and terms
   - Contract summary
   - Initial risk assessment

4. **Premium Analysis** (Parallel Execution)
   - Detailed clause analysis
   - Legal compliance verification
   - Rights and obligations review
   - Data protection assessment
   - Comprehensive risk evaluation

5. **Results Delivery**
   - Real-time updates via WebSocket
   - Schema-driven UI rendering
   - Expandable sections for detailed information

### WebSocket API

For programmatic access, connect to WebSocket endpoints:

- **Employment Contracts**: `ws://localhost:5112/employment-analysis`
- **Rental Contracts**: `ws://localhost:5112/rental-analysis`

See [API_DOCUMENTATION.md](docs/API_DOCUMENTATION.md) for detailed API specifications.

## Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test categories
dotnet test --filter "FullyQualifiedName~Domain"        # Domain tests
dotnet test --filter "FullyQualifiedName~PropertyTests" # Property-based tests
dotnet test --filter "FullyQualifiedName~Integration"   # Integration tests
```

### Test Coverage

The project includes comprehensive testing:
- **Unit Tests** - Test individual components and methods
- **Property-Based Tests** - Verify universal properties using FsCheck
- **Integration Tests** - Test end-to-end workflows

## Documentation

- **[API Documentation](docs/API_DOCUMENTATION.md)** - Complete API reference with examples
- **[Configuration Guide](docs/CONFIGURATION.md)** - Detailed configuration options
- **[Design Patterns](docs/DesignPatterns_de.md)** - Architectural patterns used (German)
- **[Architecture Diagrams](docs/diagrams/)** - Visual architecture documentation

## Performance

- **Agent Construction**: < 1ms (dependency injection only)
- **Initialization**: ~1-2 seconds (AI-based validation and type detection)
- **Basic Analysis**: ~5-10 seconds (4 parallel steps)
- **Premium Analysis**: ~10-20 seconds (7 parallel steps)
- **Token Tracking**: Minimal overhead (< 1ms per analysis)

## Security Considerations

- **Input Validation**: File size and type restrictions enforced
- **Data Privacy**: Contract texts are not persisted or logged
- **API Security**: Use HTTPS/WSS in production environments
- **Rate Limiting**: Recommended for production deployments
- **Secret Management**: Use user secrets or environment variables for API keys

## Technology Stack

- **Backend**: ASP.NET Core 8
- **AI Service**: Nebius AI API
- **PDF Processing**: UglyToad.PdfPig
- **OCR**: Tesseract OCR
- **Schema Validation**: NJsonSchema
- **Logging**: Serilog with file rotation
- **Testing**: xUnit, FsCheck (property-based testing)
- **Frontend**: Vanilla JavaScript, Bootstrap 5

## Contributing

This project follows clean architecture principles and comprehensive testing practices. When contributing:

1. Follow the existing code structure and patterns
2. Write both unit tests and property-based tests for new features
3. Update documentation for API or configuration changes
4. Ensure all tests pass before submitting changes

## License

This project is part of an academic submission. All rights reserved.

## Contact

For questions or feedback about this project, please refer to the project documentation or contact the development team through the repository.

---

**Built with ❤️ for Austrian contract analysis**
