namespace CardiacMonitoring.Api.Entities;

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public DateTime ScheduledAtUtc { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
