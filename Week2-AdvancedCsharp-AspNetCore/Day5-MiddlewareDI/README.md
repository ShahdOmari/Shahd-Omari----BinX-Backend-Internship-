# Day 5 — Middleware Pipeline & Dependency Injection; Week 2 Synthesis

**8 hours**

## Learning Objectives

- Explain the middleware pipeline and how request execution order is
  determined
- Register services with the built-in dependency injection container
  using the correct lifetime
- Inject a service into a controller via constructor injection

## What I Did

- Wrote a custom middleware that logs each request's HTTP method and
  path to the console, registered before `MapControllers()`
- Deliberately tested the middleware in the wrong pipeline position
  (after the endpoints were mapped) and confirmed it never ran for
  matched routes, then corrected the ordering
- Created an `ITaskActivityLogger` interface and a
  `TaskActivityLogger` implementation, registered with `AddScoped`
- Injected `ITaskActivityLogger` into `TasksController` via
  constructor injection and used it inside the `GetById` endpoint

## Key Code Example

```csharp
app.Use(async (context, next) =>
{
    Console.WriteLine($"[Request] {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"[Response] {context.Response.StatusCode}");
});

app.MapControllers();

// In TasksController:
public TasksController(ITaskActivityLogger activityLogger)
{
    _activityLogger = activityLogger;
}
```

## What I Learned

Placing the middleware after `MapControllers()` proved directly (not
just in theory) that a matched request never reaches middleware
registered too late — the console log for the request simply never
appeared. I also learned that `MapControllers()` alone handles
endpoint execution implicitly at the end of the pipeline regardless
of where it's written in the file; using `UseRouting()` /
`UseEndpoints()` explicitly was what let me actually control and
observe the ordering effect for this exercise.

## Project

[`Day5Practice/`](Day5Practice/) — custom middleware, a pipeline
ordering experiment, and a DI-registered service injected into the
Day 4 controller.