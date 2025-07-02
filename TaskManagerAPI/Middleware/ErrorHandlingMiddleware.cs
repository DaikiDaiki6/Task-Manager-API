using System;
using System.Text.Json;

namespace TaskManagerAPI.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
            if (context.Response.StatusCode == 405) await HandleMethodNotAllowedAsync(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleMethodNotAllowedAsync(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = 405;

        var allowedMethods = context.Response.Headers["Allow"].ToString();
        var response = new
        {
            error = "Method Not Allowed",
            message = $"The {context.Request.Method} method is not allowed for this endpoint",
            allowedMethods = string.IsNullOrEmpty(allowedMethods) ?
                new[] { "GET", "POST", "PUT", "DELETE" } :
                allowedMethods.Split(',').Select(m => m.Trim()).ToArray(),
            path = context.Request.Path.Value,
            statusCode = 405
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = 500;

        var response = new
        {
            error = "Internal Server Error",
            message = "An unexpected error occurred while processing your request",
            statusCode = 500
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
