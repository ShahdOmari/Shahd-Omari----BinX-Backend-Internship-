namespace CardiacMonitoring.Api.DTOs.Appointments;

public record CreateAppointmentRequest(
    int PatientId,
    DateTime ScheduledAtUtc,
    string DoctorName,
    string Reason);
