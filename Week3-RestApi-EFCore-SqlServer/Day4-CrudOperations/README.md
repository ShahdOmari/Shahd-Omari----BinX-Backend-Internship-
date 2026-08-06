# Day 4 — Implementing CRUD Operations with EF Core

**8 hours**

## Learning Objectives

- Implement Create and Read operations using async EF Core queries
- Implement Update and Delete operations with correct change tracking
- Handle not-found and validation error cases with appropriate HTTP
  responses

## What I Did

- Implemented a `Create` endpoint (`POST /api/v1/tasks`) that
  validates input, adds the entity via `_context.Tasks.Add()`, saves
  with `await _context.SaveChangesAsync()`, and returns `201 Created`
  with a `Location` header via `CreatedAtAction`
- Implemented `GetAll` and `GetById` endpoints using
  `.AsNoTracking()` for read-only queries, returning `404 Not Found`
  when an ID doesn't match any row
- Implemented an `Update` endpoint that fetches the tracked entity
  first, validates input, modifies its properties, and saves —
  returning `404` for a missing resource and `400` for invalid input
- Implemented a `Delete` endpoint returning `204 No Content` on
  success and `404 Not Found` if the resource doesn't exist (verified
  by deleting the same ID twice)
- Seeded a default `User` and `Project` on startup so `Task` creation
  has valid foreign keys to reference during manual testing
- Manually tested all 5 endpoints via Swagger, including the happy
  path and every documented error case (invalid input on Create/Update,
  missing ID on Get/Update/Delete, deleting an already-deleted
  resource)

## Key Code Example

```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateTaskRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Title) ||
        request.PriorityLevel < 1 || request.PriorityLevel > 5)
    {
        return BadRequest("Title is required and priority must be between 1 and 5.");
    }

    var task = new TaskItem { Title = request.Title, PriorityLevel = request.PriorityLevel,
        ProjectId = request.ProjectId, AssignedToUserId = request.AssignedToUserId };

    _context.Tasks.Add(task);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
}
```

## What I Learned

The distinction between `.AsNoTracking()` on read endpoints and
regular tracked queries on `Update` made the change tracker's purpose
concrete: `Update` needs EF Core watching the entity to know which
properties actually changed and generate a targeted `UPDATE`, while
`GetAll`/`GetById` never modify anything, so tracking them would be
wasted overhead. Testing `DELETE` on the same ID twice was also a
good concrete check — the first call correctly returns `204`, and the
second returns `404` since the resource genuinely no longer exists,
confirming the endpoint checks for existence rather than assuming
success.

## Project

[`TaskTrackerApi/`](TaskTrackerApi/) — full CRUD API for the `Tasks`
resource, backed by EF Core and SQL Server, tested end-to-end via
Swagger.