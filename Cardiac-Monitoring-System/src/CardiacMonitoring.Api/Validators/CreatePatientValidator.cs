using CardiacMonitoring.Api.DTOs.Patients;
using FluentValidation;

namespace CardiacMonitoring.Api.Validators;

public class CreatePatientValidator : AbstractValidator<CreatePatientRequest>
{
    public CreatePatientValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.");

        // A real business rule, not just a type check — a future date of
        // birth is technically a valid DateTime but meaningless here.
        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow)
            .WithMessage("Date of birth cannot be in the future.");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Gender is required.");
    }
}
