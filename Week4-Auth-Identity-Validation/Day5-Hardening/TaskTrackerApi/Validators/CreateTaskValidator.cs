using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Data;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Validators;

// Validates every incoming CreateTaskRequest before it reaches the controller.
// Registered automatically in Program.cs, so an invalid request never reaches
// TasksController at all — it gets rejected during model binding.
public class CreateTaskValidator : AbstractValidator<CreateTaskRequest>
{
    private readonly AppDbContext _context;

    public CreateTaskValidator(AppDbContext context)
    {
        _context = context;

        // Rule 1: Title must be present and within a reasonable length.
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        // Rule 2: PriorityLevel must fall inside a meaningful range — not
        // just "not null", since 0 or 999 are valid ints but meaningless
        // priority values in this domain.
        RuleFor(x => x.PriorityLevel)
            .InclusiveBetween(1, 5)
            .WithMessage("PriorityLevel must be between 1 (lowest) and 5 (highest).");

        // Rule 3: ProjectId must reference a project that actually exists.
        // EF Core's foreign key would eventually reject this anyway, but
        // checking here returns a clear 400 with a specific message instead
        // of a raw database error leaking through.
        RuleFor(x => x.ProjectId)
            .MustAsync(ProjectExistsAsync)
            .WithMessage("The specified ProjectId does not exist.");
    }

    private async Task<bool> ProjectExistsAsync(int projectId, CancellationToken cancellationToken)
    {
        return await _context.Projects.AnyAsync(p => p.Id == projectId, cancellationToken);
    }
}
