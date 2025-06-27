
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace MailFlow.API
{
    public class CorrelationIdMiddleware(RequestDelegate next) : IMiddleware
    {
        private const string CorrelationIdHeader = "X-Correlation-ID";
     
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            string correlationId = GetCorrelationId(context);
            
            using(LogContext.PushProperty(CorrelationIdHeader, correlationId))
            {
                await next(context);
            }
        }

        private static string GetCorrelationId(HttpContext context)
        {
            context.Request.Headers.TryGetValue(CorrelationIdHeader, out StringValues correlationId);
            return correlationId.FirstOrDefault() ?? context.TraceIdentifier;
        }
    }
}
