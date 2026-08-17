namespace CardiacMonitoring.Api.Entities;

// A single vital-sign reading for a patient, taken at a specific moment.
public class VitalSign
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public int HeartRateBpm { get; set; }
    public int SystolicBp { get; set; }
    public int DiastolicBp { get; set; }
    public double OxygenSaturationPercent { get; set; }
    public DateTime RecordedAtUtc { get; set; } = DateTime.UtcNow;

    // Computed once, at creation time, by CardiacRiskEvaluator (added in a
    // later step) — stored rather than recalculated on every read, so a
    // historical reading always shows the risk level that was true at the
    // moment it was actually taken.
    public RiskLevel RiskLevel { get; set; }
}
