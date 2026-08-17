using CardiacMonitoring.Api.Data;
using CardiacMonitoring.Api.DTOs.Medications;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitoring.Api.Validators;

public class CreateMedicationValidator : AbstractValidator<CreateMedicationRequest>
{
    private readonly AppDbContext _context;

    public CreateMedicationValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.PatientId)
            .MustAsync(PatientExistsAsync)
            .WithMessage("The specified PatientId does not exist.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Medication name is required.");

        RuleFor(x => x.DosageMg)
            .GreaterThan(0).WithMessage("Dosage must be greater than 0mg.");

        RuleFor(x => x.Frequency)
            .NotEmpty().WithMessage("Frequency is required.");
    }

    private async Task<bool> PatientExistsAsync(int patientId, CancellationToken cancellationToken)
    {
        return await _context.Patients.AnyAsync(p => p.Id == patientId, cancellationToken);
    }
}
