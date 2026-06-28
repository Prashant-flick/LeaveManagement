using System.Text.Json;
using Employee.Application.Common.Exceptions;
using FluentValidation;

namespace Employee.API.Common.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled exception occurred");

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        int statusCode;
        string message;
        object? errors = null;

        switch (exception)
        {
            case BaseException baseEx:
                statusCode = baseEx.StatusCode;
                message = baseEx.Message;
                break;

            case ValidationException validationEx:
                statusCode = 400;
                message = "Validation failed";

                errors = validationEx.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    error = e.ErrorMessage
                });
                break;

            case UnauthorizedAccessException:
                statusCode = 401;
                message = "Unauthorized";
                break;

            case HttpRequestException:
                statusCode = 503;
                message = "External service unavailable";
                break;

            case TaskCanceledException:
                statusCode = 504;
                message = "Request timed out";
                break;

            default:
                statusCode = 500;
                message = "An unexpected error occurred";
                break;
        }

        var response = new
        {
            success = false,
            message,
            statusCode,
            errors,
            traceId = context.TraceIdentifier
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response)
        );
    }
}