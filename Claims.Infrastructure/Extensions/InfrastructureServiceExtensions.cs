using Claims.Application.Auditing;
using Claims.Application.Common.Interfaces;
using Claims.Infrastructure.Auditing;
using Claims.Infrastructure.Persistence;
using Claims.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Claims.Infrastructure.Extensions;

/// <summary>
/// Provides extension methods for registering infrastructure services.
/// </summary>
public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Adds infrastructure services (databases, auditing, background services) to the DI container.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Audit database (SQL Server)
        services.AddDbContext<AuditContext>(options =>
            options.UseSqlServer(
                    configuration.GetConnectionString("AuditDatabase"))
                .ConfigureWarnings(warnings =>
                    warnings.Ignore(
                        Microsoft.EntityFrameworkCore.Diagnostics
                            .RelationalEventId.PendingModelChangesWarning)));

        // Claims database (MongoDB)
        services.AddDbContext<ClaimsContext>(options =>
        {
            var connectionString =
                configuration.GetConnectionString("MongoDb");

            var databaseName =
                configuration["MongoDb:DatabaseName"];

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);

            options.UseMongoDB(
                database.Client,
                database.DatabaseNamespace.DatabaseName);
        });

        // Register ClaimsContext as IClaimsContext
        services.AddScoped<IClaimsContext>(sp => sp.GetRequiredService<ClaimsContext>());

        // Auditing services
        services.AddScoped<IAuditService, Auditer>();
        services.AddSingleton<IAuditQueue, AuditQueue>();
        services.AddHostedService<AuditBackgroundService>();

        return services;
    }
}
