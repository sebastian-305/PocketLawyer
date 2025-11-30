using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PocketLawyer.Options;
using Serilog;
using Serilog.Events;

namespace PocketLawyer.Tests.Integration;

/// <summary>
/// Integration tests for filesystem logging functionality.
/// Verifies that log files are created correctly with proper naming and filtering.
/// </summary>
public class FilesystemLoggingIntegrationTests : IDisposable
{
    private readonly string _testLogPath;

    public FilesystemLoggingIntegrationTests()
    {
        // Create a unique test log directory for each test run
        _testLogPath = Path.Combine(Path.GetTempPath(), $"pocketlawyer-test-logs-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testLogPath);
    }

    public void Dispose()
    {
        // Clean up test log directory
        if (Directory.Exists(_testLogPath))
        {
            try
            {
                Directory.Delete(_testLogPath, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public void FilesystemLogging_Enabled_CreatesLogFileWithCorrectName()
    {
        // Arrange
        var config = CreateConfiguration(enabled: true, minimumLevel: "Information");
        var logger = CreateLogger(config);

        // Act
        logger.LogInformation("Test log message");
        Log.CloseAndFlush();
        
        // Give the file system time to flush and close handles
        System.Threading.Thread.Sleep(200);

        // Assert
        var logFiles = Directory.GetFiles(_testLogPath, "log-*.txt");
        Assert.Single(logFiles);

        var fileName = Path.GetFileName(logFiles[0]);
        var today = DateTime.Now.ToString("yyyyMMdd");
        Assert.Contains(today, fileName);
    }

    [Fact]
    public void FilesystemLogging_Disabled_DoesNotCreateLogFile()
    {
        // Arrange
        var config = CreateConfiguration(enabled: false, minimumLevel: "Information");
        var logger = CreateLogger(config);

        // Act
        logger.LogInformation("Test log message");
        Log.CloseAndFlush();

        // Assert
        var logFiles = Directory.GetFiles(_testLogPath, "log-*.txt");
        Assert.Empty(logFiles);
    }

    [Fact]
    public void FilesystemLogging_MinimumLevelInformation_FiltersDebugLogs()
    {
        // Arrange
        var config = CreateConfiguration(enabled: true, minimumLevel: "Information");
        var logger = CreateLogger(config);

        // Act
        logger.LogDebug("Debug message - should not appear");
        logger.LogInformation("Information message - should appear");
        logger.LogWarning("Warning message - should appear");
        Log.CloseAndFlush();
        
        // Give the file system time to flush
        System.Threading.Thread.Sleep(100);

        // Assert
        var logFiles = Directory.GetFiles(_testLogPath, "log-*.txt");
        Assert.Single(logFiles);

        var logContent = File.ReadAllText(logFiles[0]);
        Assert.DoesNotContain("Debug message", logContent);
        Assert.Contains("Information message", logContent);
        Assert.Contains("Warning message", logContent);
    }

    [Fact]
    public void FilesystemLogging_MinimumLevelWarning_FiltersInfoAndDebugLogs()
    {
        // Arrange
        var config = CreateConfiguration(enabled: true, minimumLevel: "Warning");
        var logger = CreateLogger(config);

        // Act
        logger.LogDebug("Debug message - should not appear");
        logger.LogInformation("Information message - should not appear");
        logger.LogWarning("Warning message - should appear");
        logger.LogError("Error message - should appear");
        Log.CloseAndFlush();

        // Assert
        var logFiles = Directory.GetFiles(_testLogPath, "log-*.txt");
        Assert.Single(logFiles);

        var logContent = File.ReadAllText(logFiles[0]);
        Assert.DoesNotContain("Debug message", logContent);
        Assert.DoesNotContain("Information message", logContent);
        Assert.Contains("Warning message", logContent);
        Assert.Contains("Error message", logContent);
    }

    [Fact]
    public void FilesystemLogging_LogFormat_ContainsTimestampLevelAndMessage()
    {
        // Arrange
        var config = CreateConfiguration(enabled: true, minimumLevel: "Information");
        var logger = CreateLogger(config);

        // Act
        logger.LogInformation("Test message with format");
        Log.CloseAndFlush();
        
        // Give the file system time to flush and close handles
        System.Threading.Thread.Sleep(200);

        // Assert
        var logFiles = Directory.GetFiles(_testLogPath, "log-*.txt");
        Assert.Single(logFiles);

        var logContent = File.ReadAllText(logFiles[0]);
        
        // Verify format: {Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}", logContent); // Timestamp
        Assert.Contains("[INF]", logContent); // Level
        Assert.Contains("Test message with format", logContent); // Message
    }

    [Fact]
    public void FilesystemLogging_MultipleLogLevels_AllWrittenCorrectly()
    {
        // Arrange
        var config = CreateConfiguration(enabled: true, minimumLevel: "Debug");
        var logger = CreateLogger(config);

        // Act
        logger.LogDebug("Debug level");
        logger.LogInformation("Information level");
        logger.LogWarning("Warning level");
        logger.LogError("Error level");
        logger.LogCritical("Critical level");
        Log.CloseAndFlush();
        
        // Give the file system time to flush
        System.Threading.Thread.Sleep(100);

        // Assert
        var logFiles = Directory.GetFiles(_testLogPath, "log-*.txt");
        Assert.Single(logFiles);

        var logContent = File.ReadAllText(logFiles[0]);
        Assert.Contains("[DBG]", logContent);
        Assert.Contains("[INF]", logContent);
        Assert.Contains("[WRN]", logContent);
        Assert.Contains("[ERR]", logContent);
        Assert.Contains("[FTL]", logContent);
    }

    [Fact]
    public void FilesystemLogging_ExceptionLogging_IncludesStackTrace()
    {
        // Arrange
        var config = CreateConfiguration(enabled: true, minimumLevel: "Information");
        var logger = CreateLogger(config);

        // Act
        var exception = new InvalidOperationException("Test exception");
        logger.LogError(exception, "An error occurred");
        Log.CloseAndFlush();

        // Assert
        var logFiles = Directory.GetFiles(_testLogPath, "log-*.txt");
        Assert.Single(logFiles);

        var logContent = File.ReadAllText(logFiles[0]);
        Assert.Contains("An error occurred", logContent);
        Assert.Contains("System.InvalidOperationException", logContent);
        Assert.Contains("Test exception", logContent);
    }

    [Fact]
    public void FilesystemLogging_StructuredLogging_PreservesProperties()
    {
        // Arrange
        var config = CreateConfiguration(enabled: true, minimumLevel: "Information");
        var logger = CreateLogger(config);

        // Act
        logger.LogInformation("User {UserId} performed action {Action}", 123, "Login");
        Log.CloseAndFlush();
        
        // Give the file system time to flush and close handles
        System.Threading.Thread.Sleep(200);

        // Assert
        var logFiles = Directory.GetFiles(_testLogPath, "log-*.txt");
        Assert.Single(logFiles);

        var logContent = File.ReadAllText(logFiles[0]);
        Assert.Contains("User 123 performed action Login", logContent);
    }

    #region Helper Methods

    private IConfiguration CreateConfiguration(bool enabled, string minimumLevel)
    {
        var configData = new Dictionary<string, string>
        {
            ["Logging:File:Enabled"] = enabled.ToString(),
            ["Logging:File:MinimumLevel"] = minimumLevel,
            ["Logging:File:Path"] = _testLogPath
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
    }

    private ILogger<FilesystemLoggingIntegrationTests> CreateLogger(IConfiguration configuration)
    {
        var fileLoggingOptions = configuration.GetSection("Logging:File").Get<FileLoggingOptions>() 
            ?? new FileLoggingOptions();

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext();

        // Add file logging if enabled
        if (fileLoggingOptions.Enabled)
        {
            var minimumLevel = Enum.Parse<LogEventLevel>(fileLoggingOptions.MinimumLevel, ignoreCase: true);
            
            loggerConfig.WriteTo.File(
                path: Path.Combine(fileLoggingOptions.Path, "log-.txt"),
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: minimumLevel,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            );
        }

        Log.Logger = loggerConfig.CreateLogger();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
            // Set minimum level to match Serilog configuration
            builder.SetMinimumLevel(LogLevel.Trace);
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        return serviceProvider.GetRequiredService<ILogger<FilesystemLoggingIntegrationTests>>();
    }

    #endregion
}
