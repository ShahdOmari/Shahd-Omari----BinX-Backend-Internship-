using CardiacMonitoring.Api.Data;
using CardiacMonitoring.Api.DTOs.VitalSigns;
using CardiacMonitoring.Api.Entities;
using CardiacMonitoring.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitoring.Api.Services;

public class VitalSignService : IVitalSignService
{
    private readonly IRepository<VitalSign> _vitalSignRepository;
    private readonly IRepository<Appointment> _appointmentRepository;
    private readonly IRiskEvaluator _riskEvaluator;
    private readonly AppDbContext _context;

    public VitalSignService(
        IRepository<VitalSign> vitalSignRepository,
        IRepository<Appointment> appointmentRepository,
        IRiskEvaluator riskEvaluator,
        AppDbContext context)
    {
        _vitalSignRepository = vitalSignRepository;
        _appointmentRepository = appointmentRepository;
        _riskEvaluator = riskEvaluator;
        _context = context;
    }

    public async Task<VitalSignResponse> RecordReadingAsync(CreateVitalSignRequest request, int? recordedByStaffProfileId)
    {
        var vitalSign = new VitalSign
        {
            PatientId = request.PatientId,
            HeartRateBpm = request.HeartRateBpm,
            SystolicBp = request.SystolicBp,
            DiastolicBp = request.DiastolicBp,
            OxygenSaturationPercent = request.OxygenSaturationPercent,
            RecordedAtUtc = DateTime.UtcNow,
            RecordedByStaffProfileId = recordedByStaffProfileId
        };

        vitalSign.RiskLevel = _riskEvaluator.Evaluate(vitalSign);

        // Sprint 1, Day 4: real business logic beyond simple field mapping —
        // a Critical reading automatically schedules an urgent follow-up
        // appointment, unless the patient already has one coming up in the
        // next 24 hours (avoids stacking duplicate urgent appointments if
        // several critical readings come in close together).
        //
        // Recording the vital sign and creating the follow-up appointment
        // must succeed or fail together — a reading saved without its
        // required follow-up (or an orphaned appointment with no matching
        // reading) would both be inconsistent, half-finished data. Wrapped
        // in a single database transaction to guarantee that.
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _vitalSignRepository.AddAsync(vitalSign);
            await _vitalSignRepository.SaveChangesAsync();

            if (vitalSign.RiskLevel == RiskLevel.Critical)
            {
                var next24Hours = DateTime.UtcNow.AddHours(24);

                var hasUpcomingAppointment = await _context.Appointments
                    .AnyAsync(a => a.PatientId == request.PatientId
                                   && a.ScheduledAtUtc <= next24Hours
                                   && a.ScheduledAtUtc >= DateTime.UtcNow);

                if (!hasUpcomingAppointment)
                {
                    var followUp = new Appointment
                    {
                        PatientId = request.PatientId,
                        ScheduledAtUtc = DateTime.UtcNow.AddHours(2),
                        DoctorName = "On-Call Cardiologist",
                        Reason = "Urgent follow-up — automatically scheduled after a Critical vital sign reading."
                    };

                    await _appointmentRepository.AddAsync(followUp);
                    await _appointmentRepository.SaveChangesAsync();
                }
            }

            await transaction.CommitAsync();

            return new VitalSignResponse(
                vitalSign.Id, vitalSign.PatientId, vitalSign.HeartRateBpm,
                vitalSign.SystolicBp, vitalSign.DiastolicBp,
                vitalSign.OxygenSaturationPercent, vitalSign.RecordedAtUtc,
                vitalSign.RiskLevel);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
