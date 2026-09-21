using Claims.Api.Extensions;
using Claims.Infrastructure.Extensions;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.AddApplicationLogging()
       .AddApplicationServices()
       .AddApplicationSwagger();

builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseApplicationSwagger();
}

app.UseHttpsRedirection();

app.UseApplicationPipeline();

app.MigrateAuditDatabase();

app.Run();

