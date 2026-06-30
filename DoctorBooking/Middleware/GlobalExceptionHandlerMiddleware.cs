using System.Net;
using System.Text.Json;
using Application.Common.Models;
using ValidationException = Application.Common.Exceptions.ValidationException;
using NotFoundException = Application.Common.Exceptions.NotFoundException;

namespace DoctorBooking.Middleware;

/// <summary>
/// Global exception handler middleware. Ensures every error returns
/// a consistent ApiResponse format — no unhandled exceptions leak to the client.
/// </summary>
public sealed class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail("Validation failed.", validationEx.Errors.ToList())
            ),

            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                ApiResponse<object>.Fail(notFoundEx.Message)
            ),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                ApiResponse<object>.Fail("Unauthorized access.")
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail("An unexpected error occurred.")
            )
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
