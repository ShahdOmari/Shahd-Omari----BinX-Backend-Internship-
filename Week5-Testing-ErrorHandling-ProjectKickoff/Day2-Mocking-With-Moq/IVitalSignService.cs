using CardiacMonitoring.Api.DTOs.VitalSigns;

namespace CardiacMonitoring.Api.Services;

// Extracted from the controller so the "record a reading and score it"
// logic can be tested in isolation from HTTP concerns and mocked in unit
// tests, instead of being buried inside VitalSignsController.Create.
public interface IVitalSignService
{
    Task<VitalSignResponse> RecordReadingAsync(CreateVitalSignRequest request);
}
