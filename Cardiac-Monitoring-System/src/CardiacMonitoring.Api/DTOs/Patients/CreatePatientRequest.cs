namespace CardiacMonitoring.Api.DTOs.Patients;

// A record, not a class — immutable and has built-in value-based equality,
// which fits a request DTO well (Week 1's OOP lesson: records for data
// transfer objects).
public record CreatePatientRequest(string FullName, DateTime DateOfBirth, string Gender);
