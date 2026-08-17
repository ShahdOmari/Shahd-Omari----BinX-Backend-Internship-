namespace CardiacMonitoring.Api.DTOs.VitalSigns;

public record CreateVitalSignRequest(
    int PatientId,
    int HeartRateBpm,
    int SystolicBp,
    int DiastolicBp,
    double OxygenSaturationPercent);
