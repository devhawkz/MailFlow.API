using Contracts;
using Entities.ErrorModel;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace MailFlow.API
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            httpContext.Response.ContentType = "application/json";

            var contextFeature = httpContext.Features.Get<IExceptionHandlerFeature>();
            if (contextFeature is not null)
            {
                //_logger.LogError($"Something went wrong: {exception.Message}");
                
                await httpContext.Response.WriteAsync(new ErrorDetails()
                {
                    StatusCode = httpContext.Response.StatusCode,
                    Message = "Internal Server Error"
                }.ToString());
            }
            return true;
        }
    }
}
