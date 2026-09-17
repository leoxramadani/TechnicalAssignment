using Claims.Common.Logging;

namespace Claims.Extensions;

/// <summary>
/// Provides extension methods for configuring logging in the application.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Adds application logging to the service collection.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddApplicationLogging(
        this WebApplicationBuilder builder)
    {
        var logFilePath =
            builder.Configuration["Logging:FilePath"]
            ?? Path.Combine(
                builder.Environment.ContentRootPath,
                "claims-log.txt");

        builder.Logging.AddFile(logFilePath);

        return builder;
    }
}