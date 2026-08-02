using Microsoft.AspNetCore.Mvc;

namespace MyFirstApi.Controllers;

// In memory list here instead of a database, since the focus this week
// is routing, middleware, and DI, persistence with a real database
// starts in Week 3.
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private static readonly List<TaskItem> _tasks = new()
    {
        new TaskItem(1, "Design database schema", 3),
        new TaskItem(2, "Write unit tests", 2),
        new TaskItem(3, "Setup CI/CD pipeline", 5)
    };

    // Receiving ITaskActivityLogger through the constructor instead of
    // creating it manually inside the class ("new TaskActivityLogger()").
    // The DI container resolves and supplies the concrete implementation
    // automatically, based on how it was registered in Program.cs.
    private readonly ITaskActivityLogger _activityLogger;

    public TasksController(ITaskActivityLogger activityLogger)
    {
        _activityLogger = activityLogger;
    }

    // GET api/tasks
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_tasks);
    }

    // GET api/tasks/{id}
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);

        // Returning 404 here instead of 200 with an empty body, since
        // "the resource doesn't exist" is a different situation than
        // "the resource exists and happens to be empty" REST expects
        // that distinction to show up in the status code.
        if (task == null)
            return NotFound();

        // Using the injected service to log access to this specific
        // task, separate from the request-level logging the middleware
        // already handles in Program.cs.
        _activityLogger.LogAccess(task.Title);

        return Ok(task);
    }
}