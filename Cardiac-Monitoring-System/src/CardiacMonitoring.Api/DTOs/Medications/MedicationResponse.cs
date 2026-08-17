namespace CardiacMonitoring.Api.DTOs.Medications;

public record MedicationResponse(
    int Id,
    int PatientId,
    string Name,
    double DosageMg,
    string Frequency);
