var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
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


// ------- Minimal API versions of the Tasks endpoints, for comparison ----------
// Same in-memory data as TasksController, duplicated here on purpose just
// to compare the two styles side by side — in a real project I wouldn't
// duplicate data like this.

var minimalTasks = new List<TaskItem>
{
    new TaskItem(1, "Design database schema", 3),
    new TaskItem(2, "Write unit tests", 2),
    new TaskItem(3, "Setup CI/CD pipeline", 5)
};

// GET api/tasks-minimal
app.MapGet("/api/tasks-minimal", () => Results.Ok(minimalTasks));

// GET api/tasks-minimal/{id}
app.MapGet("/api/tasks-minimal/{id}", (int id) =>
{
    var task = minimalTasks.FirstOrDefault(t => t.Id == id);
    return task == null ? Results.NotFound() : Results.Ok(task);
});


app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Simple record just for this demo — shared between TasksController and
// the minimal API endpoints above.
public record TaskItem(int Id, string Title, int PriorityLevel);