namespace Claims.Extensions;

/// <summary>
/// Provides extension methods for configuring Swagger in the application.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Adds Swagger services to the application.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddApplicationSwagger(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        return builder;
    }

    /// <summary>
    /// Configures the application to use Swagger and Swagger UI in the development environment.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication UseApplicationSwagger(
        this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }
}