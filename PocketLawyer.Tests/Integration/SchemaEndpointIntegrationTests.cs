using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PocketLawyer.Application.Exceptions;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Infrastructure;
using PocketLawyer.Options;
using PocketLawyer.Web;
using System.Text.Json.Nodes;

namespace PocketLawyer.Tests.Integration;

/// <summary>
/// Integration tests for the schema retrieval endpoint.
/// Tests the complete flow from HTTP request to schema response.
/// </summary>
public class SchemaEndpointIntegrationTests
{
    private readonly Mock<ILogger<PocketController>> _logger;
    private readonly Mock<IWebHostEnvironment> _environment;
    private readonly Mock<WebSocketAnalysisHandler> _webSocketHandler;
    private readonly ISchemaProvider _schemaProvider;
    private readonly PathOptions _pathOptions;

    public SchemaEndpointIntegrationTests()
    {
        _logger = new Mock<ILogger<PocketController>>();
        _environment = new Mock<IWebHostEnvironment>();
        _webSocketHandler = new Mock<WebSocketAnalysisHandler>(
            Mock.Of<ILogger<WebSocketAnalysisHandler>>(),
            Mock.Of<IFileValidatorService>());
        _schemaProvider = new PocketLawyer.Infrastructure.SchemaProviders.SchemaProvider();
        _pathOptions = new PathOptions
        {
            ResponsesPath = "data/Responses"
        };
    }

    [Fact]
    public void GetSchema_RentalBasicGeneral_ReturnsValidSchema()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetSchema("Rental", "Basic", "General");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var schema = Assert.IsType<JsonObject>(okResult.Value);
        
        Assert.NotNull(schema);
        Assert.Equal("object", schema["type"]?.ToString());
        Assert.NotNull(schema["properties"]);
        Assert.NotNull(schema["required"]);
    }

    [Fact]
    public void GetSchema_RentalBasicRent_ReturnsValidSchema()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetSchema("Rental", "Basic", "Rent");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var schema = Assert.IsType<JsonObject>(okResult.Value);
        
        Assert.NotNull(schema);
        Assert.Equal("object", schema["type"]?.ToString());
        Assert.NotNull(schema["properties"]);
    }

    [Fact]
    public void GetSchema_RentalBasicConditions_ReturnsValidSchema()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetSchema("Rental", "Basic", "Conditions");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var schema = Assert.IsType<JsonObject>(okResult.Value);
        
        Assert.NotNull(schema);
        Assert.Equal("object", schema["type"]?.ToString());
    }

    [Fact]
    public void GetSchema_RentalBasicSummary_ReturnsValidSchema()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetSchema("Rental", "Basic", "Summary");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var schema = Assert.IsType<JsonObject>(okResult.Value);
        
        Assert.NotNull(schema);
        Assert.Equal("object", schema["type"]?.ToString());
    }

    [Fact]
    public void GetSchema_RentalPremiumCosts_ReturnsValidSchema()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetSchema("Rental", "Premium", "Costs");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var schema = Assert.IsType<JsonObject>(okResult.Value);
        
        Assert.NotNull(schema);
        Assert.Equal("object", schema["type"]?.ToString());
    }

    [Fact]
    public void GetSchema_RentalPremiumTerms_ReturnsValidSchema()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetSchema("Rental", "Premium", "Terms");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var schema = Assert.IsType<JsonObject>(okResult.Value);
        
        Assert.NotNull(schema);
    }

    [Fact]
    public void GetSchema_RentalPremiumUse_ReturnsValidSchema()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetSchema("Rental", "Premium", "Use");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var schema = Assert.IsType<JsonObject>(okResult.Value);
        
        Assert.NotNull(schema);
    }

    [Fact]
    public void GetSchema_RentalPremiumMaintenance_ReturnsValidSchema()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetSchema("Rental", "Premium", "Maintenance");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var schema = Assert.IsType<JsonObject>(okResult.Value);
        
        Assert.NotNull(schema);
    }

    [Fact]
    public void GetSchema_RentalPremiumDuties_ReturnsValidSchema()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetSchema("Rental", "Premium", "Duties");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var schema = Assert.IsType<JsonObject>(okResult.Value);
        
        Assert.NotNull(schema);
    }

    [Fact]
    public void GetSchema_RentalPremiumPrivacy_ReturnsValidSchema()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetSchema("Rental", "Premium", "Privacy");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var schema = Assert.IsType<JsonObject>(okResult.Value);
        
        Assert.NotNull(schema);
    }

    [Fact]
    public void GetSchema_RentalPremiumSummary_ReturnsValidSchema()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetSchema("Rental", "Premium", "Summary");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var schema = Assert.IsType<JsonObject>(okResult.Value);
        
        Assert.NotNull(schema);
    }

    [Fact]
    public void GetSchema_InvalidSchemaKey_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetSchema("Invalid", "Invalid", "Invalid");

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public void GetSchema_AllRentalSchemas_ContainStylingMetadata()
    {
        // Arrange
        var controller = CreateController();
        var schemaKeys = new[]
        {
            ("Rental", "Basic", "General"),
            ("Rental", "Basic", "Rent"),
            ("Rental", "Basic", "Conditions"),
            ("Rental", "Basic", "Summary")
        };

        foreach (var (contractType, tier, step) in schemaKeys)
        {
            // Act
            var result = controller.GetSchema(contractType, tier, step);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var schema = Assert.IsType<JsonObject>(okResult.Value);
            
            var properties = schema["properties"] as JsonObject;
            Assert.NotNull(properties);

            // Check that at least one property has x-styling metadata
            bool hasStyling = false;
            foreach (var prop in properties)
            {
                var propValue = prop.Value as JsonObject;
                if (propValue != null && propValue.ContainsKey("x-styling"))
                {
                    hasStyling = true;
                    break;
                }
            }
            
            Assert.True(hasStyling, $"Schema {contractType}/{tier}/{step} should contain x-styling metadata");
        }
    }

    [Fact]
    public void GetSchema_AllRentalSchemas_ContainRequiredFields()
    {
        // Arrange
        var controller = CreateController();
        var schemaKeys = new[]
        {
            ("Rental", "Basic", "General"),
            ("Rental", "Basic", "Rent"),
            ("Rental", "Basic", "Conditions"),
            ("Rental", "Basic", "Summary")
        };

        foreach (var (contractType, tier, step) in schemaKeys)
        {
            // Act
            var result = controller.GetSchema(contractType, tier, step);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var schema = Assert.IsType<JsonObject>(okResult.Value);
            
            Assert.NotNull(schema["type"]);
            Assert.NotNull(schema["properties"]);
            Assert.NotNull(schema["required"]);
            
            var required = schema["required"] as JsonArray;
            Assert.NotNull(required);
            Assert.NotEmpty(required);
        }
    }

    [Fact]
    public void GetSchema_PremiumSchemas_HaveConsistentStructure()
    {
        // Arrange
        var controller = CreateController();
        var premiumSteps = new[] { "Costs", "Terms", "Use", "Maintenance", "Duties", "Privacy" };

        foreach (var step in premiumSteps)
        {
            // Act
            var result = controller.GetSchema("Rental", "Premium", step);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var schema = Assert.IsType<JsonObject>(okResult.Value);
            
            var properties = schema["properties"] as JsonObject;
            Assert.NotNull(properties);

            // Premium sections should have these fields
            Assert.True(properties.ContainsKey("zusammenfassung"), $"{step} should have zusammenfassung");
            Assert.True(properties.ContainsKey("bewertung"), $"{step} should have bewertung");
            Assert.True(properties.ContainsKey("rechtsgrundlage"), $"{step} should have rechtsgrundlage");
            Assert.True(properties.ContainsKey("risikoScore"), $"{step} should have risikoScore");
            Assert.True(properties.ContainsKey("riskLevel"), $"{step} should have riskLevel");
        }
    }

    [Fact]
    public void GetSchema_LogsRequest()
    {
        // Arrange
        var controller = CreateController();

        // Act
        controller.GetSchema("Rental", "Basic", "General");

        // Assert
        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieving schema")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetSchema_InvalidSchema_LogsWarning()
    {
        // Arrange
        var controller = CreateController();

        // Act
        controller.GetSchema("Invalid", "Invalid", "Invalid");

        // Assert
        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Schema not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private PocketController CreateController()
    {
        var options = Microsoft.Extensions.Options.Options.Create(_pathOptions);
        return new PocketController(
            _logger.Object,
            options,
            _environment.Object,
            _webSocketHandler.Object,
            _schemaProvider);
    }
}
