var builder = WebApplication.CreateBuilder(args);

// ------ Services registration ---------

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registering the custom service as Scoped: I want one instance per HTTP
// request, not a brand new one on every single call (Transient), and not
// a single instance shared across every user for the app's entire
// lifetime Singleton : Singleton would be the wrong choice here since
// this service could end up holding per-request state later on.
builder.Services.AddScoped<ITaskActivityLogger, TaskActivityLogger>();

var app = builder.Build();

// ----- Middleware pipeline ------
// Order matters here — everything below runs in the exact sequence it's
// registered in, for every incoming request.

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Custom logging middleware, registered before UseHttpsRedirection and
// MapControllers on purpose, so it's the first thing that sees every
// request on its way in, and the last thing that touches it on the way
// back out. Moving this after MapControllers would mean it never runs
// at all, since MapControllers already sends a response and ends the
// pipeline before reaching this point.

app.UseHttpsRedirection();

// Custom logging middleware, registered before MapControllers on purpose,
// so every request is logged before it reaches any controller — this is
// the corrected order, after testing what happens when it's placed
// after the endpoints are mapped (see commit history / Notion notes for
// that comparison).
app.Use(async (context, next) =>
{
    Console.WriteLine($"[Request]  {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"[Response] {context.Response.StatusCode}");
});

app.MapControllers();


/* app.UseHttpsRedirection();

app.UseRouting();

app.UseEndpoints(endpoints => endpoints.MapControllers());

// Placed AFTER UseEndpoints — for any request that matches a real
// endpoint (like /api/tasks), UseEndpoints already handles it and stops
// the pipeline there, so this middleware never runs for matched routes.
app.Use(async (context, next) =>
{
    Console.WriteLine($"[Request]  {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"[Response] {context.Response.StatusCode}");
});
*/

// ----- Minimal API endpoints (kept from Day 4 for comparison) ----------

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

var minimalTasks = new List<TaskItem>
{
    new TaskItem(1, "Design database schema", 3),
    new TaskItem(2, "Write unit tests", 2),
    new TaskItem(3, "Setup CI/CD pipeline", 5)
};

app.MapGet("/api/tasks-minimal", () => Results.Ok(minimalTasks));

app.MapGet("/api/tasks-minimal/{id}", (int id) =>
{
    var task = minimalTasks.FirstOrDefault(t => t.Id == id);
    return task == null ? Results.NotFound() : Results.Ok(task);
});

app.Run();


// ----- Supporting types ------------------------------------

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public record TaskItem(int Id, string Title, int PriorityLevel);

// Interface so TasksController depends on an abstraction rather than a
// concrete class the same reasoning behind interfaces in Week 1's OOP
// lesson, now doing real structural work: this is what would let a fake
// implementation be swapped in during unit testing later on.
public interface ITaskActivityLogger
{
    void LogAccess(string taskTitle);
}

public class TaskActivityLogger : ITaskActivityLogger
{
    public void LogAccess(string taskTitle)
    {
        Console.WriteLine($"[Task Activity] '{taskTitle}' accessed at {DateTime.Now:HH:mm:ss}");
    }
}