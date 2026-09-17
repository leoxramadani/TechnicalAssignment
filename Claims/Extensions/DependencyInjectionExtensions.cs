using Claims.Helpers.PremiumComputation;
using Claims.Services.AuditingServices;
using Claims.Services.ClaimsServices;
using Claims.Services.CoversServices;
using FluentValidation;

namespace Claims.Extensions;

/// <summary>
/// Provides extension methods for configuring dependency injection in the application.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds application services to the dependency injection container.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddApplicationServices(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IAuditService, Auditer>();
        builder.Services.AddScoped<IPremiumComputationService, PremiumComputationService>();
        builder.Services.AddScoped<IClaimsService, ClaimsService>();
        builder.Services.AddScoped<ICoversService, CoversService>();

        builder.Services.AddSingleton<IAuditQueue, AuditQueue>();
        builder.Services.AddHostedService<AuditBackgroundService>();

        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        return builder;
    }
}