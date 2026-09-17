using Claims.Extensions;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder
    .AddApplicationLogging()
    .AddApplicationDatabases()
    .AddApplicationServices()
    .AddApplicationSwagger();

var app = builder.Build();

app
    .UseApplicationSwagger()
    .UseApplicationPipeline()
    .MigrateAuditDatabase();

app.Run();

public partial class Program;