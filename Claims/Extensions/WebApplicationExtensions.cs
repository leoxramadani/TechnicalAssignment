namespace Claims.Extensions;

/// <summary>
/// Provides extension methods for configuring the application pipeline in a web application.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the application pipeline with HTTPS redirection, authorization, and controller mapping.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication UseApplicationPipeline(
        this WebApplication app)
    {
        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}