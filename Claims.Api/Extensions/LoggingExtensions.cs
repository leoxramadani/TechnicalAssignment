using Claims.Infrastructure.Common.Logging;

namespace Claims.Api.Extensions;

public static class LoggingExtensions
{
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
