using Contracts;
using LoggerService;
using Microsoft.EntityFrameworkCore;
using Repository;
using Serilog;
using Service;
using Service.Contracts;

namespace MailFlow.API.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureCors(this IServiceCollection services) =>
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
        });

    public static void ConfigureRepositoryManager(this IServiceCollection services, IConfiguration configuration) =>
        services.AddScoped<IRepositoryManager, RepositoryManager>();

    public static void ConfigureServiceManager(this IServiceCollection services) =>
        services.AddScoped<IServiceManager, ServiceManager>();

    public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration) =>
        services.AddDbContext<DataContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("sqlConnection")));

    public static void ConfigureToolsRepository(this IServiceCollection services) =>
        services.AddScoped<IToolsRepository, ToolsRepository>();
    public static void ConfigureToolsService(this IServiceCollection services) =>
        services.AddScoped<IToolsService, ToolsService>();

    public static void ConfigureGmailApiClient(this IServiceCollection services) =>
        services.AddHttpClient<IGmailApiClient, GmailApiClient>();

    public static void ConfigureCorrelationIdMiddleware(this IServiceCollection services) =>
        services.AddTransient<CorrelationIdMiddleware>();

    public static void ConfigureLoggerManager(this IServiceCollection services) => services.AddScoped<ILoggerManager, LoggerManager>();

}

