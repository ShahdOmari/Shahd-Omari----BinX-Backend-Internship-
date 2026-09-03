using System.Diagnostics;
using System.Security.Claims;

namespace CardiacMonitoring.Api.Middleware;

// Every HTTP request that touches this API is logged with who made it,
// what they asked for, what we returned, and how long it took. In a
// real cardiac monitoring system this satisfies the audit trail required
// by healthcare compliance frameworks (HIPAA, ISO 27001) — any access to
// sensitive patient data must be traceable to a specific authenticated
// staff member. Implemented as middleware (not an action filter) because
// it must also cover requests that are rejected before they reach a
// controller, such as unauthenticated calls returning 401 or rate-limited
// calls returning 429.
public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        // Resolved after _next(context) so the JWT authentication
        // middleware has already run and populated User.Claims — reading
        // claims before that point would always return anonymous.
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? context.User.FindFirstValue("sub")
                     ?? "anonymous";

        var staffProfileId = context.User.FindFirstValue("staffProfileId") ?? "-";
        var role = context.User.FindFirstValue(ClaimTypes.Role) ?? "-";

        _logger.LogInformation(
            "AUDIT | {Method} {Path} | status={Status} | user={UserId} | staff={StaffProfileId} | role={Role} | {ElapsedMs}ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            userId,
            staffProfileId,
            role,
            stopwatch.ElapsedMilliseconds);
    }
}
