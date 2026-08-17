using CardiacMonitoring.Api.Data;
using CardiacMonitoring.Api.DTOs.Appointments;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitoring.Api.Validators;

public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentRequest>
{
    private readonly AppDbContext _context;

    public CreateAppointmentValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.PatientId)
            .MustAsync(PatientExistsAsync)
            .WithMessage("The specified PatientId does not exist.");

        // An appointment scheduled in the past isn't a real business
        // scenario for this endpoint (creating a new upcoming appointment).
        RuleFor(x => x.ScheduledAtUtc)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Appointment must be scheduled in the future.");

        RuleFor(x => x.DoctorName)
            .NotEmpty().WithMessage("Doctor name is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.");
    }

    private async Task<bool> PatientExistsAsync(int patientId, CancellationToken cancellationToken)
    {
        return await _context.Patients.AnyAsync(p => p.Id == patientId, cancellationToken);
    }
}
