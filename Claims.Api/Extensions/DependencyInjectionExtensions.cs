using Claims.Api.Filters;
using Claims.Application.Claims;
using Claims.Application.Covers;
using Claims.Domain.Services;
using FluentValidation;

namespace Claims.Api.Extensions;

public static class DependencyInjectionExtensions
{
    public static WebApplicationBuilder AddApplicationServices(
        this WebApplicationBuilder builder)
    {
        // Application layer registrations
        builder.Services.AddScoped<IPremiumComputationService, PremiumComputationService>();
        builder.Services.AddScoped<IClaimsService, ClaimsService>();
        builder.Services.AddScoped<ICoversService, CoversService>();
        builder.Services.AddScoped<ValidationFilter>();

        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        return builder;
    }
}
