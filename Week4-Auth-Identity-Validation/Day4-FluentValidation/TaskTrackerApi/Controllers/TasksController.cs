using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Data;
using TaskTrackerApi.Models;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;

namespace TaskTrackerApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IValidator<CreateTaskRequest> _createValidator;
    private readonly IValidator<UpdateTaskRequest> _updateValidator;

    // Validators are injected explicitly rather than relying on automatic
    // MVC pipeline validation, since FluentValidation's automatic mode
    // only supports synchronous rules — our CreateTaskValidator needs an
    // async database check (ProjectId existence), so it must be invoked
    // manually with ValidateAsync, keeping the async chain intact.
    public TasksController(
        AppDbContext context,
        IValidator<CreateTaskRequest> createValidator,
        IValidator<UpdateTaskRequest> updateValidator)
    {
        _context = context;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [Authorize(Policy = "CanManageProjects")]
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
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            // Returns a structured list of field-level error messages
            // instead of a single generic string.
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
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

        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        task.Title = request.Title;
        task.PriorityLevel = request.PriorityLevel;
        task.IsCompleted = request.IsCompleted;
        await _context.SaveChangesAsync();
        return Ok(task);
    }

    [Authorize(Roles = "Admin")]
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
