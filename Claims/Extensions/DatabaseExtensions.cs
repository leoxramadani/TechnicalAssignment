using Claims.Persistance;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Claims.Extensions;

/// <summary>
/// Provides extension methods for configuring and managing application databases.
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Adds the application databases to the service collection.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddApplicationDatabases(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AuditContext>(options =>
            options.UseSqlServer(
                    builder.Configuration.GetConnectionString("AuditDatabase"))
                .ConfigureWarnings(warnings =>
                    warnings.Ignore(
                        Microsoft.EntityFrameworkCore.Diagnostics
                            .RelationalEventId.PendingModelChangesWarning)));

        builder.Services.AddDbContext<ClaimsContext>(options =>
        {
            var connectionString =
                builder.Configuration.GetConnectionString("MongoDb");

            var databaseName =
                builder.Configuration["MongoDb:DatabaseName"];

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);

            options.UseMongoDB(
                database.Client,
                database.DatabaseNamespace.DatabaseName);
        });

        return builder;
    }
    /// <summary>
    /// Migrates the audit database to the latest version.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication MigrateAuditDatabase(
        this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<AuditContext>();

        context.Database.Migrate();

        return app;
    }
}