namespace CardiacMonitoring.Api.Entities;

public class Medication
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public double DosageMg { get; set; }
    public string Frequency { get; set; } = string.Empty; // e.g. "Twice daily"
}
