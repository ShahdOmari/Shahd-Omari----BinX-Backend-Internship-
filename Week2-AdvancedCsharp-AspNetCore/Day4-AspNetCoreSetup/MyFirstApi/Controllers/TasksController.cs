using Microsoft.AspNetCore.Mvc;

namespace MyFirstApi.Controllers;

// Using a simple in-memory list here instead of a database, since Day 4
// is about routing and controllers, not persistence yet (that's Week 3).
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    // Hardcoded for now — a real version would come from a database or
    // an injected repository, but the goal today is just the routing layer.
    private static readonly List<TaskItem> _tasks = new()
    {
        new TaskItem(1, "Design database schema", 3),
        new TaskItem(2, "Write unit tests", 2),
        new TaskItem(3, "Setup CI/CD pipeline", 5)
    };

    // GET api/tasks
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_tasks);
    }

    // GET api/tasks/{id}
    // The {id} in the route below is a route parameter — ASP.NET Core
    // automatically binds it to the "id" parameter of this method.
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);

        // Returning 404 here instead of 200 with an empty/null body, since
        // "the resource doesn't exist" is a different situation than
        // "the resource exists and happens to be empty" — REST expects
        // that distinction to show up in the status code, not just the body.
        if (task == null)
            return NotFound();

        return Ok(task);
    }
}