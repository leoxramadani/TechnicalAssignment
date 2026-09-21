using Microsoft.AspNetCore.Builder;

namespace Claims.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApplicationPipeline(this WebApplication app)
    {
        app.UseRouting();
        app.UseAuthorization();
        app.MapControllers();
        return app;
    }
}
