using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using SmartTaskOptimizer.Application.Common.Exceptions;

namespace SmartTaskOptimizer.API.Middleware;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger) => _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try { await next(context); }
        catch (Exception ex) { await HandleAsync(context, ex); }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, title, detail) = ex switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed", "One or more validation errors occurred."),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", "Authentication is required or the supplied credentials are invalid."),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden", ex.Message),
            NotFoundException => (StatusCodes.Status404NotFound, "Not found", ex.Message),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict", ex.Message),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Concurrency conflict", "The resource was changed by another request. Refresh and try again."),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid operation", ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "Server error", "An unexpected error occurred.")
        };

        if (status >= 500) _logger.LogError(ex, "Unhandled request error for {Path}", context.Request.Path);
        else _logger.LogWarning(ex, "Request failed with {StatusCode} for {Path}", status, context.Request.Path);

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails { Status = status, Title = title, Detail = detail, Instance = context.Request.Path };
        if (ex is ValidationException validation)
        {
            problem.Extensions["errors"] = validation.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).Distinct().ToArray());
        }
        await context.Response.WriteAsJsonAsync(problem);
    }
}
