using System.Text.Json;
using Auth.Application.Common.Exceptions;
using FluentValidation;

namespace Auth.API.Common.Middleware;

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
                statusCode = StatusCodes.Status400BadRequest;
                message = "Validation failed";

                errors = validationEx.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    error = e.ErrorMessage
                });
                break;

            case UnauthorizedAccessException:
                statusCode = StatusCodes.Status401Unauthorized;
                message = "Unauthorized";
                break;

            case HttpRequestException:
                statusCode = StatusCodes.Status503ServiceUnavailable;
                message = "External service unavailable";
                break;

            case TaskCanceledException:
                statusCode = StatusCodes.Status504GatewayTimeout;
                message = "Request timed out";
                break;

            default:
                statusCode = StatusCodes.Status500InternalServerError;
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