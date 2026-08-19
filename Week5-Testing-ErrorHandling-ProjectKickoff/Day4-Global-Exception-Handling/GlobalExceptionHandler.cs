using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CardiacMonitoring.Api.Middleware;

// Implements ASP.NET Core's built-in IExceptionHandler interface (the
// modern replacement for hand-rolled exception-handling middleware) —
// registered once via AddExceptionHandler<T>() and wired into the
// pipeline with app.UseExceptionHandler(), it catches any exception left
// unhandled by every controller in the API, so individual endpoints never
// need their own try/catch for the unexpected case.
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Structured logging: {RequestPath} and {ExceptionType} stay as
        // distinct, queryable fields in the log entry rather than being
        // flattened into one opaque string — this matters once logs are
        // aggregated somewhere searchable, not just scrolled through
        // locally in a console window.
        _logger.LogError(
            exception,
            "Unhandled exception on {RequestMethod} {RequestPath}. Exception type: {ExceptionType}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            exception.GetType().Name);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            // Deliberately generic — the real exception message and stack
            // trace are logged server-side above, never sent to the
            // client. Leaking them would hand an attacker implementation
            // details for free.
            Detail = "The server encountered an error while processing your request. Please try again later.",
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // Returning true tells the framework this handler fully handled
        // the exception — no further exception handlers or the default
        // developer exception page should run.
        return true;
    }
}
