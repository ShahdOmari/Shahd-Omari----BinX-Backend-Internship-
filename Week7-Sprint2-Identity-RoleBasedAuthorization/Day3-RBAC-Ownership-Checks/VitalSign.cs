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

    // Sprint 2, Day 3: links each reading to the staff member who recorded
    // it, read from the "staffProfileId" JWT claim at creation time. This
    // is what makes ownership checks possible — a Nurse can be limited to
    // viewing only the readings she personally took, while Doctor/Auditor/
    // Admin retain full visibility. Nullable because older seeded/legacy
    // readings may not have a known recorder.
    public int? RecordedByStaffProfileId { get; set; }
}