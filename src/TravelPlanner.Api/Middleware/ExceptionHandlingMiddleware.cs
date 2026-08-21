using System.Net;
using System.Text.Json;

namespace TravelPlanner.Api.Middleware;
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred processing the request including {path}.", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (status, title) = exception switch
        {
            KeyNotFoundException => (HttpStatusCode.NotFound, "Not Found"),
            ArgumentException => (HttpStatusCode.BadRequest, "Invalid Request"),
            InvalidOperationException => (HttpStatusCode.Conflict, "Operation Not Valid"),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error")
        };
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        var problemResponse = new
        {
            type = $"https://httpstatuses.com/{(int)status}",
            title,
            status = (int)status,
            detail = exception.Message,
            instance = context.Request.Path.Value
        };

        var jsonResponse = JsonSerializer.Serialize(problemResponse);
        return context.Response.WriteAsync(jsonResponse);
    }
}