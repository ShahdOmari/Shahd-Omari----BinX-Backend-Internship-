namespace CardiacMonitoring.Api.DTOs.Appointments;

public record AppointmentResponse(
    int Id,
    int PatientId,
    DateTime ScheduledAtUtc,
    string DoctorName,
    string Reason);
