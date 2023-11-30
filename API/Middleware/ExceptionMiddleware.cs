using System.Net;
using System.Text.Json;
using API.Errors;
using API.Extensions;

namespace API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            if (!httpContext.Response.HasStarted)
            {
                httpContext.Response.ContentType = "application/json; charset=utf-8";
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            var response = _environment.IsDevelopment()
                ? new ApiException((int)HttpStatusCode.InternalServerError, e.Message, e.StackTrace)
                : new ApiException((int)HttpStatusCode.InternalServerError, e.Message, "Internal server error");

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            var jsonResponse = JsonSerializer.Serialize(response, options);

            await httpContext.Response.WriteAsync(jsonResponse);
        }
    }
}