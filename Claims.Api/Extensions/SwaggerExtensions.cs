using Microsoft.OpenApi.Models;

namespace Claims.Api.Extensions;

public static class SwaggerExtensions
{
    public static WebApplicationBuilder AddApplicationSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Claims API", Version = "v1" });

            var xmlFile = "Claims.Api.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });

        return builder;
    }

    public static WebApplication UseApplicationSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Claims API v1");
        });

        return app;
    }
}
