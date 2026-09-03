# Day 4 — Custom Middleware & Pull Request

## Sprint Context
Sprint 2, Week 7, Day 4.

## What Was Built

### Custom Middleware: AuditLoggingMiddleware

**The cross-cutting concern:** In a cardiac monitoring system, every access to patient data must be traceable to a specific authenticated staff member — this is a real compliance requirement (HIPAA, ISO 27001) that applies uniformly across every endpoint without any per-controller changes.

**Why middleware and not an action filter:**
Action filters only run for requests that reach a controller action. Middleware runs for every request, including those rejected before reaching a controller (401 Unauthorized, 429 Too Many Requests, etc.) — which is precisely the kind of access attempt that matters most in an audit trail.

**What it logs (one line per request):**
AUDIT | POST /api/v1/Auth/login | status=200 | user=<userId> | staff=1 | role=Nurse | 45ms 

Fields logged per request:
- HTTP method and path
- Response status code
- `sub` claim (ApplicationUser ID) — or "anonymous" if unauthenticated
- `staffProfileId` claim — links the request to a specific StaffProfile record
- `role` claim — the role the caller held at the time of the request
- Elapsed milliseconds (response time)

**Key implementation detail:** The middleware reads JWT claims *after* calling `await _next(context)` — not before. This matters because `UseAuthentication()` runs in the pipeline before this middleware, so claims are only populated on the `HttpContext` after the authentication middleware has processed the token. Reading claims before `_next` would always return anonymous.

**Pipeline position:**
```csharp
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuditLoggingMiddleware>(); // here — claims resolved, status code set
app.MapControllers();
```

### Pull Request
Sprint 2 branch `week7/day2-auth` pushed to GitHub with a PR summarizing all Sprint 2 work (Days 1-4): Identity wiring, StaffProfile domain entity, JWT with domain claims, RBAC enforcement, ownership checks, and audit logging middleware.

## Test Suite: 26/26 passing
All tests continued to pass after adding the middleware — confirmed by running `dotnet test` after registration in the pipeline.

## Key Takeaway
Middleware is the correct tool for cross-cutting concerns that must apply before a controller is ever reached. Action filters are for concerns scoped to controller actions only. For an audit trail in a security-sensitive system, middleware is the only choice that guarantees complete coverage.
