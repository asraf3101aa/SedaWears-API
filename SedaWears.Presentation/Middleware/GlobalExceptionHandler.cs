using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SedaWears.Application.Common.Exceptions;

namespace SedaWears.Presentation.Middleware;

public sealed class GlobalExceptionHandler(
    IHostEnvironment env,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ValidationException or NotFoundException or BadRequestException or UnauthorizedAccessException or ForbiddenException or ConflictException)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Handled domain exception: {ExceptionType} - {Message}", exception.GetType().Name, exception.Message);
            }
        }
        else
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(exception, "Unhandled exception occurred while processing request to {Path}", httpContext.Request.Path);
            }
        }

        var problemDetails = exception switch
        {
            ValidationException ex => new ValidationProblemDetails(ex.Errors)
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Validation failed",
                Detail = "One or more validation errors occurred.",
                Instance = httpContext.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            },

            NotFoundException ex => new ProblemDetails
            {
                Status = (int)HttpStatusCode.NotFound,
                Title = "Resource not found",
                Detail = ex.Message,
                Instance = httpContext.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            },

            BadRequestException ex => new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Bad request",
                Detail = ex.Message,
                Instance = httpContext.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            },

            UnauthorizedAccessException ex => new ProblemDetails
            {
                Status = (int)HttpStatusCode.Unauthorized,
                Title = "Unauthorized",
                Detail = ex.Message ?? "You are not authenticated to access this resource.",
                Instance = httpContext.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
            },

            ForbiddenException ex => new ProblemDetails
            {
                Status = (int)HttpStatusCode.Forbidden,
                Title = "Forbidden",
                Detail = ex.Message,
                Instance = httpContext.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
            },

            ConflictException ex => new ProblemDetails
            {
                Status = (int)HttpStatusCode.Conflict,
                Title = "Conflict",
                Detail = ex.Message,
                Instance = httpContext.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8"
            },

            _ => new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "Internal server error",
                Detail = env.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred.",
                Instance = httpContext.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            }
        };

        if (env.IsDevelopment() && exception is not ValidationException)
        {
            problemDetails.Extensions.Add("stackTrace", exception.StackTrace);
        }

        httpContext.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, problemDetails.GetType(), cancellationToken);

        return true;
    }
}