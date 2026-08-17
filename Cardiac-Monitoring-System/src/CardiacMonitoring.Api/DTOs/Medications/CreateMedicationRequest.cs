namespace CardiacMonitoring.Api.DTOs.Medications;

public record CreateMedicationRequest(
    int PatientId,
    string Name,
    double DosageMg,
    string Frequency);
