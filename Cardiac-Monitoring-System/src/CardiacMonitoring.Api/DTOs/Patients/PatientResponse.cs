namespace CardiacMonitoring.Api.DTOs.Patients;

// What we actually return to the client — separate from the CreatePatientRequest
// on purpose, since a response shape and a request shape often diverge
// (e.g. Id only makes sense in a response, never in a create request).
public record PatientResponse(int Id, string FullName, DateTime DateOfBirth, string Gender);
