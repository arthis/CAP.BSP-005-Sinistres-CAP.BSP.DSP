using Serilog;
using Serilog.Events;

namespace CAP.BSP.DSP.Infrastructure.Observability;

/// <summary>
/// Serilog configuration for structured logging.
/// Configures log output, enrichment, and minimum levels.
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// Configures Serilog with console output and environment enrichment.
    /// </summary>
    /// <param name="minimumLevel">Minimum log level (default: Information).</param>
    /// <returns>Configured Serilog logger.</returns>
    public static ILogger Configure(LogEventLevel minimumLevel = LogEventLevel.Information)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Is(minimumLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", "CAP.BSP.DSP")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    /// <summary>
    /// Configures Serilog from environment variables.
    /// Reads log level from SERILOG_MIN_LEVEL environment variable.
    /// </summary>
    /// <returns>Configured Serilog logger.</returns>
    public static ILogger ConfigureFromEnvironment()
    {
        var logLevel = Environment.GetEnvironmentVariable("SERILOG_MIN_LEVEL");
        var minimumLevel = Enum.TryParse<LogEventLevel>(logLevel, out var level)
            ? level
            : LogEventLevel.Information;

        return Configure(minimumLevel);
    }
}
