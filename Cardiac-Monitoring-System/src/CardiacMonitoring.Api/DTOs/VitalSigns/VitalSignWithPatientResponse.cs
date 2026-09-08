namespace CardiacMonitoring.Api.DTOs.VitalSigns;

public record VitalSignWithPatientResponse(
    int Id,
    int PatientId,
    string PatientName,
    int HeartRateBpm,
    int SystolicBp,
    int DiastolicBp,
    double OxygenSaturationPercent,
    DateTime RecordedAtUtc,
    CardiacMonitoring.Api.Entities.RiskLevel RiskLevel);
