using Contracts;
using Entities.ErrorModel;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MailFlow.API;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IServiceProvider _serviceProvider;

    public GlobalExceptionHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();   
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerManager>();


        
        httpContext.Response.ContentType = "application/json";

        var statusCode = exception switch
        {
            ArgumentNullException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            HttpRequestException => (int)HttpStatusCode.ServiceUnavailable,
            TaskCanceledException => (int)HttpStatusCode.GatewayTimeout,
            DbUpdateException => (int)HttpStatusCode.Conflict,
            _ => (int)HttpStatusCode.InternalServerError
        };
        
        httpContext.Response.StatusCode = (int)statusCode;
        
        logger.LogError($"Exception caught by global handler: {exception}");

        var problemDetails = new ProblemDetails
        {
            Status = httpContext.Response.StatusCode,
            Title = statusCode switch
            {
                (int)HttpStatusCode.BadRequest => "Bad Request",
                (int)HttpStatusCode.Unauthorized => "Unauthorized",
                (int)HttpStatusCode.NotFound => "Not Found",
                (int)HttpStatusCode.ServiceUnavailable => "Service Unavailable",
                (int)HttpStatusCode.GatewayTimeout => "Gateway Timeout",
                (int)HttpStatusCode.Conflict => "Conflict",
                _ => "Internal Server Error"
            },
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
