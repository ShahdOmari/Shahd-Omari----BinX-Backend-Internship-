# Day 4 — ASP.NET Core Project Setup & Routing

**8 hours**

## Learning Objectives

- Scaffold a new ASP.NET Core Web API project using the dotnet CLI
- Understand the minimal hosting model in Program.cs
- Define routes using both Controllers and Minimal APIs

## What I Did

- Scaffolded a new Web API with `dotnet new webapi -o MyFirstApi` and
  confirmed it ran, then added Swagger UI manually (the newer .NET
  template only includes OpenAPI JSON, not the Swagger UI page, by
  default)
- Built a `TasksController` with a `GET /api/tasks` endpoint returning
  a hardcoded list, and `GET /api/tasks/{id}` returning a single item
  by route parameter, returning 404 when no match is found
- Added the same two endpoints again as Minimal APIs directly in
  `Program.cs` (`/api/tasks-minimal` and `/api/tasks-minimal/{id}`)
  for comparison
- Tested all 4 endpoints in Postman and saved them as a collection

## Key Code Example

```csharp
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(_tasks);

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        return task == null ? NotFound() : Ok(task);
    }
}
```

## What I Learned

Controllers and Minimal APIs end up producing identical results for
simple endpoints — the real difference is organizational. A
Controller groups related endpoints as methods on a class, which
stays readable as an API grows; a Minimal API is quicker to write for
one or two routes but would get messy fast if `Program.cs` had to
hold dozens of them. I also learned that newer .NET versions changed
the default Web API template — Swagger UI isn't included out of the
box anymore and has to be added explicitly.

## Project

[`MyFirstApi/`](MyFirstApi/) — first ASP.NET Core Web API with both
Controller-based and Minimal API endpoints, tested via Swagger and
Postman.