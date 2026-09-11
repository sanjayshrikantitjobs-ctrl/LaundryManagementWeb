using System.Net;
using System.Text.Json;
using FluentValidation;
using LaundryMgmt.Domain.Exceptions;

namespace LaundryMgmt.API.Middleware;

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
            _logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);

            var (statusCode, payload) = ex switch
            {
                ValidationException validationEx => ((int)HttpStatusCode.BadRequest, (object)new
                {
                    title = "Validation failed",
                    errors = validationEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
                }),
                DomainException domainEx => ((int)HttpStatusCode.Conflict, new { title = domainEx.Message }),
                // Application handlers across the codebase (Customers in particular:
                // duplicate phone number, "no customer profile linked to this login",
                // etc.) throw this for expected business-rule failures — same intent as
                // DomainException, just predating it. Without this case every one of
                // those fell through to the generic 500 below instead of surfacing the
                // actual reason.
                InvalidOperationException invalidOpEx => ((int)HttpStatusCode.Conflict, new { title = invalidOpEx.Message }),
                KeyNotFoundException notFoundEx => ((int)HttpStatusCode.NotFound, new { title = notFoundEx.Message }),
                UnauthorizedAccessException => ((int)HttpStatusCode.Forbidden, new { title = "Access denied." }),
                _ => ((int)HttpStatusCode.InternalServerError, new { title = "An unexpected error occurred." })
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
