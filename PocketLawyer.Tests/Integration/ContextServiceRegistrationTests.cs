using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Infrastructure.Context;
using PocketLawyer.Options;

namespace PocketLawyer.Tests.Integration;

public class ContextServiceRegistrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ContextServiceRegistrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void ContextOptions_IsRegistered()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var options = scope.ServiceProvider.GetService<Microsoft.Extensions.Options.IOptions<ContextOptions>>();

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.Value);
        Assert.Equal("data/Context", options.Value.BasePath);
    }

    [Fact]
    public void IContextFileReader_IsRegisteredAsSingleton()
    {
        // Arrange & Act
        using var scope1 = _factory.Services.CreateScope();
        using var scope2 = _factory.Services.CreateScope();
        
        var reader1 = scope1.ServiceProvider.GetService<IContextFileReader>();
        var reader2 = scope2.ServiceProvider.GetService<IContextFileReader>();

        // Assert
        Assert.NotNull(reader1);
        Assert.NotNull(reader2);
        Assert.Same(reader1, reader2); // Singleton should be the same instance
    }

    [Fact]
    public void IContextProvider_IsRegisteredAsScoped()
    {
        // Arrange & Act
        using var scope1 = _factory.Services.CreateScope();
        using var scope2 = _factory.Services.CreateScope();
        
        var provider1a = scope1.ServiceProvider.GetService<IContextProvider>();
        var provider1b = scope1.ServiceProvider.GetService<IContextProvider>();
        var provider2 = scope2.ServiceProvider.GetService<IContextProvider>();

        // Assert
        Assert.NotNull(provider1a);
        Assert.NotNull(provider1b);
        Assert.NotNull(provider2);
        Assert.Same(provider1a, provider1b); // Same scope should return same instance
        Assert.NotSame(provider1a, provider2); // Different scopes should return different instances
    }

    [Fact(Skip = "Not reliable across platforms (Linux/Windows) – requires refactor with IDirectoryService")]
    public void ContextDirectoryInitializer_IsRegisteredAsHostedService()
    {
        // Arrange & Act
        var hostedServices = _factory.Services.GetServices<Microsoft.Extensions.Hosting.IHostedService>();

        // Assert
        Assert.Contains(hostedServices, service => service is ContextDirectoryInitializer);
    }

    [Fact]
    public void AllContextServices_CanBeResolved()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Assert - All services should resolve without throwing
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ContextOptions>>();
        var fileReader = serviceProvider.GetRequiredService<IContextFileReader>();
        var provider = serviceProvider.GetRequiredService<IContextProvider>();

        Assert.NotNull(options);
        Assert.NotNull(fileReader);
        Assert.NotNull(provider);
    }
}
