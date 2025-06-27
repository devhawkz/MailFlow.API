using Contracts;
using Entities.ErrorModel;
using Microsoft.AspNetCore.Diagnostics;
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


        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/json";

        var contextFeature = httpContext.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature is not null)
        {
            logger.LogError($"Something went wrong: {exception.Message}");
            
            await httpContext.Response.WriteAsync(new ErrorDetails()
            {
                StatusCode = httpContext.Response.StatusCode,
                Message = "Internal Server Error"
            }.ToString());
        }
        return true;
    }
}
