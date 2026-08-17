using CardiacMonitoring.Api.Entities;

namespace CardiacMonitoring.Api.DTOs.VitalSigns;

// Includes RiskLevel in the response — the client needs to see the
// computed risk immediately after submitting a reading, not just the
// raw numbers they sent.
public record VitalSignResponse(
    int Id,
    int PatientId,
    int HeartRateBpm,
    int SystolicBp,
    int DiastolicBp,
    double OxygenSaturationPercent,
    DateTime RecordedAtUtc,
    RiskLevel RiskLevel);
