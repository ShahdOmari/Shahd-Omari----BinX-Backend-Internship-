using CardiacMonitoring.Api.Data;
using CardiacMonitoring.Api.DTOs.VitalSigns;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitoring.Api.Validators;

public class CreateVitalSignValidator : AbstractValidator<CreateVitalSignRequest>
{
    private readonly AppDbContext _context;

    public CreateVitalSignValidator(AppDbContext context)
    {
        _context = context;

        // PatientId must reference a patient that actually exists — an
        // async database check, run explicitly by the controller since
        // ASP.NET's automatic validation pipeline can't invoke async rules.
        RuleFor(x => x.PatientId)
            .MustAsync(PatientExistsAsync)
            .WithMessage("The specified PatientId does not exist.");

        // Real physiological bounds, not arbitrary — values outside these
        // ranges are almost certainly data-entry errors, not real readings.
        RuleFor(x => x.HeartRateBpm)
            .InclusiveBetween(20, 250)
            .WithMessage("Heart rate must be between 20 and 250 bpm.");

        RuleFor(x => x.SystolicBp)
            .InclusiveBetween(50, 250)
            .WithMessage("Systolic blood pressure must be between 50 and 250 mmHg.");

        RuleFor(x => x.OxygenSaturationPercent)
            .InclusiveBetween(0, 100)
            .WithMessage("Oxygen saturation must be between 0 and 100 percent.");
    }

    private async Task<bool> PatientExistsAsync(int patientId, CancellationToken cancellationToken)
    {
        return await _context.Patients.AnyAsync(p => p.Id == patientId, cancellationToken);
    }
}
