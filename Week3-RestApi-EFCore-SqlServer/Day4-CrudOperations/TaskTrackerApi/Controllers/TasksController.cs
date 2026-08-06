using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Data;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _context;

    public TasksController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tasks = await _context.Tasks.AsNoTracking().ToListAsync();
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _context.Tasks.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            return NotFound();

        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) ||
            request.PriorityLevel < 1 || request.PriorityLevel > 5)
        {
            return BadRequest("Title is required and priority must be between 1 and 5.");
        }

        var task = new TaskItem
        {
            Title = request.Title,
            PriorityLevel = request.PriorityLevel,
            ProjectId = request.ProjectId,
            AssignedToUserId = request.AssignedToUserId
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateTaskRequest request)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(request.Title) ||
            request.PriorityLevel < 1 || request.PriorityLevel > 5)
        {
            return BadRequest("Title is required and priority must be between 1 and 5.");
        }

        task.Title = request.Title;
        task.PriorityLevel = request.PriorityLevel;
        task.IsCompleted = request.IsCompleted;

        await _context.SaveChangesAsync();

        return Ok(task);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            return NotFound();

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}