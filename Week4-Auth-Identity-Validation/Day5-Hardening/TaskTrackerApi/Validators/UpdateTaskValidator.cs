using FluentValidation;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Validators;

// Validates every incoming UpdateTaskRequest before it reaches the controller.
public class UpdateTaskValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskValidator()
    {
        // Rule 1: Title must be present and within a reasonable length.
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        // Rule 2: PriorityLevel must stay inside the same valid range used
        // on create — an update should not be able to set an invalid priority
        // even though the task already exists.
        RuleFor(x => x.PriorityLevel)
            .InclusiveBetween(1, 5)
            .WithMessage("PriorityLevel must be between 1 (lowest) and 5 (highest).");

        // IsCompleted is a bool — model binding itself already guarantees
        // it's a real boolean, so no extra rule is needed for it here.
    }
}
