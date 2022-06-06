using MoneySaver.Identity.Data;
using MoneySaver.System.Infrastructure;
using MoneySaver.System.Services;
using MoneySaver.Identity.Infrastructure;
using MoneySaver.Identity.Services.Identity;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddWebService<MoneySaver.Identity.Data.IdentityDbContext>(builder.Configuration);
builder.Services.AddUserStorage();
builder.Services.AddTransient<IDataSeeder, IdentityDataSeeder>();
builder.Services.AddTransient<IIdentityService, IdentityService>()
                .AddTransient<ITokenGeneratorService, TokenGeneratorService>();

builder.Services.AddHealthChecks();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.MapHealthChecks("/healthz");

//app.UseAuthorization();

//app.MapControllers();
app.UseWebService(app.Environment)
    .Initialize();

app.Run();
