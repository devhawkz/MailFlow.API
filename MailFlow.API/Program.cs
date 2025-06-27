using MailFlow.API;
using MailFlow.API.Controllers;
using MailFlow.API.Extensions;
using Microsoft.EntityFrameworkCore;
using Repository;
using Serilog;
using Service;
using Service.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//enabling reading secrets from user secrets
builder.Configuration.AddUserSecrets<Program>();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();


builder.Services.ConfigureRepositoryManager(builder.Configuration);
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.ConfigureCors();
builder.Services.ConfigureToolsRepository();
builder.Services.ConfigureToolsService();
builder.Services.ConfigureGmailApiClient();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


builder.Services.ConfigureLoggerManager();

builder.Services.AddControllers()
    .AddApplicationPart(typeof(MailFlow.Presentation.AssemblyReference).Assembly);
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.


if (app.Environment.IsProduction())
    app.UseHsts();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.MapControllers();

app.Run();
