using MoneySaver.Identity.Data;
using MoneySaver.System.Infrastructure;
using MoneySaver.System.Services;
using MoneySaver.Identity.Infrastructure;
using MoneySaver.Identity.Services.Identity;
using Serilog;
using MoneySaver.Identity.Models.Configuration;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry;
using OpenTelemetry.Exporter.Prometheus;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
              .ReadFrom.Configuration(builder.Configuration)
              .CreateLogger();

builder.Services.AddLogging(logging =>
{
    logging.AddSerilog(dispose: true);
});
// Add services to the container.
builder.Services.AddWebService<IdentityDbContext>(builder.Configuration);
builder.Services.Configure<UrlRoutesConfiguration>(builder.Configuration.GetSection(nameof(UrlRoutesConfiguration)));

builder.Services.AddHttpClient();
builder.Services.AddUserStorage();
builder.Services.AddTransient<IDataSeeder, IdentityDataSeeder>();
builder.Services.AddTransient<IIdentityService, IdentityService>()
                .AddTransient<ITokenGeneratorService, TokenGeneratorService>();

// builder.Services.AddHealthChecks();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Host.UseSerilog();
//builder.Logging.AddOpenTelemetry(logging => {
//    logging.IncludeScopes = true;
//    logging.IncludeFormattedMessage = true;
//});
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("Moneysaver.Identity"))
    .WithMetrics(metrics =>
    {
        metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        //.AddPrometheusExporter()
        .AddHttpClientInstrumentation()
        .AddMeter("Microsoft.AspNetCore.Hosting")
        .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
        // Metrics provided by System.Net libraries
        .AddMeter("System.Net.Http")
        .AddMeter("System.Net.NameResolution");
        //.AddPrometheusExporter();

        metrics.AddPrometheusExporter();
    })
    .WithTracing(tracing =>
    {
        tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation();

        tracing.AddOtlpExporter();
    });
    //.UseOtlpExporter();

var app = builder.Build();

app.MapPrometheusScrapingEndpoint();

app.UseWebService(app.Environment)
    .Initialize();



app.Run();
