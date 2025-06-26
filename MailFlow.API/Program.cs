using MailFlow.API;
using MailFlow.API.Controllers;
using MailFlow.API.Extensions;
using Microsoft.EntityFrameworkCore;
using Repository;
using Service;
using Service.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//enabling reading secrets from user secrets
builder.Configuration.AddUserSecrets<Program>();
builder.Services.ConfigureRepositoryManager(builder.Configuration);
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.ConfigureCors();
builder.Services.ConfigureToolsRepository();
builder.Services.ConfigureToolsService();
builder.Services.ConfigureGmailApiClient();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddControllers()
    .AddApplicationPart(typeof(MailFlow.Presentation.AssemblyReference).Assembly);
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsProduction())
    app.UseHsts();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
