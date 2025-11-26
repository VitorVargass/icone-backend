using System.Net;
using System.Text.Json;
using icone_backend.Models;

namespace icone_backend.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception caught by global middleware");

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var error = new Error
                {
                    Code = ex.GetType().Name,
                    Message = "Ocorreu um erro inesperado no servidor.",
                    Details = ex.Message,         
                    TraceId = context.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                };

#if DEBUG
                error.Details = $"{ex.Message}\n{ex.StackTrace}";
#endif

                var json = JsonSerializer.Serialize(error);
                await context.Response.WriteAsync(json);
            }
        }
    }
}
