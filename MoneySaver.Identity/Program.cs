using MoneySaver.Identity.Data;
using MoneySaver.System.Infrastructure;
using MoneySaver.System.Services;
using MoneySaver.Identity.Infrastructure;
using MoneySaver.Identity.Services.Identity;
using HealthChecks.UI.Client;
using Serilog;
using MoneySaver.Identity.Models.Configuration;

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

var app = builder.Build();

app.UseWebService(app.Environment)
    .Initialize();

app.Run();
