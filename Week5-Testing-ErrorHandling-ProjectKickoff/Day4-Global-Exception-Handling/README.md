# Day 4 — Centralized Error Handling & Global Exception Middleware

### Week 5 · BinX Backend Development Internship (.NET)

---

## 1. Learning Objectives

- Explain the problems with scattering try/catch throughout every endpoint.
- Implement centralized exception-handling middleware.
- Return standardized error responses using the ProblemDetails format.
- Apply structured logging so failures are diagnosable, not just visible.

## 2. What I Built

- `GlobalExceptionHandler` — implements ASP.NET Core's built-in `IExceptionHandler`
  interface (the modern replacement for a hand-rolled `app.UseExceptionHandler(errApp
  => ...)` lambda), registered via `AddExceptionHandler<T>()` and
  `app.UseExceptionHandler()` at the very top of the pipeline.
- Catches any exception left unhandled anywhere downstream (controllers, other
  middleware) and returns a standardized `ProblemDetails` (RFC 7807) response —
  `status`, `title`, `detail`, `instance` — instead of a raw framework error page or
  an inconsistent, hand-formatted error per endpoint.
- The client-facing `detail` is a deliberately generic, safe message. The real
  exception's message and full stack trace are never sent to the client — only
  logged server-side.
- Structured logging via `ILogger`: `{RequestMethod}`, `{RequestPath}`, and
  `{ExceptionType}` are logged as distinct, named fields (not flattened into one
  interpolated string), so they stay individually searchable once logs are
  aggregated somewhere queryable in production.
- `DiagnosticsController` — a small, permanent diagnostic endpoint
  (`GET /api/v1/Diagnostics/trigger-error`) that deliberately throws, purely to
  verify the handler end-to-end after this or any future pipeline change.

## 3. Verification

| Check | Result |
|---|---|
| Client response for an unhandled exception | `500` with a clean `ProblemDetails` body, no stack trace, no real exception message |
| Server console/log | Full exception message + stack trace + structured `RequestPath`/`ExceptionType` fields |
| Search for leftover local `try/catch` blocks in the codebase | Zero matches — the project was built from the start to let unexpected errors bubble up to this handler, rather than being retrofitted after scattering try/catch everywhere |
| Full test suite after adding the handler | 19/19 still passing — purely additive change, no existing behavior affected |

## 4. Note on This Folder

This folder contains a **read-only snapshot** of the files written this day, for
review purposes. The actual, buildable, currently-maintained versions live in the
real project at:
Cardiac-Monitoring-System/src/CardiacMonitoring.Api/Middleware/GlobalExceptionHandler.cs
Cardiac-Monitoring-System/src/CardiacMonitoring.Api/Controllers/DiagnosticsController.cs

Run `dotnet run` from `Cardiac-Monitoring-System/src/CardiacMonitoring.Api/` and hit
`GET /api/v1/Diagnostics/trigger-error` via Swagger to see it in action.

## 5. Key Takeaway

A single centralized handler, registered once at the top of the pipeline, replaces
what would otherwise be dozens of near-identical try/catch blocks scattered across
every controller action — each one a place the response shape could quietly drift,
or worse, accidentally leak an internal exception message to a caller. Choosing
`IExceptionHandler` over a raw middleware lambda also means the handler participates
correctly with `AddProblemDetails()`, so this same consistent shape automatically
extends to other built-in error paths (like model-validation failures) too, not just
unhandled exceptions.
