namespace CardiacMonitoring.Api.Entities;

// Core patient profile. Kept intentionally minimal and non-identifying
// beyond what's needed for the prototype, since only synthetic data is
// used throughout this project (no real patient data).
public class Patient
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;

    // Navigation properties — EF Core uses these to build the one-to-many
    // relationships once we add the DbContext in a later step.
    public ICollection<VitalSign> VitalSigns { get; set; } = new List<VitalSign>();
    public ICollection<Medication> Medications { get; set; } = new List<Medication>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
