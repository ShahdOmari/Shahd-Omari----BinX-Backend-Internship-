using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CardiacMonitoring.Api.DTOs.Medications;
using CardiacMonitoring.Api.DTOs.Patients;
using Xunit;

namespace CardiacMonitoring.Tests.Integration;

// This is the highest-risk area identified in Day 5's risk analysis:
// role-based access control. A silent regression here (as actually
// happened once already — see Day 3's writeup on the accidental
// class-level [Authorize] bug) could let an unprivileged Nurse account
// perform Doctor-only actions like prescribing or removing medication.
public class RoleBasedAccessTests : IClassFixture<CardiacApiFactory>
{
    private readonly CardiacApiFactory _factory;

    public RoleBasedAccessTests(CardiacApiFactory factory)
    {
        _factory = factory;
    }

    // Registers a user, assigns the given role, and logs in again — a
    // second login is required because the JWT's role claim is baked in
    // at token-issue time, not read live from the database on every
    // request (see Week 4 Day 3's lesson on this exact point).
    private async Task<HttpClient> CreateClientWithRoleAsync(string role)
    {
        var client = _factory.CreateClient();
        var email = $"{role.ToLower()}.{Guid.NewGuid():N}@cardiac.test";
        const string password = "TestPass@123";

        await client.PostAsJsonAsync("/api/v1/Auth/register", new { email, password });
        await client.PostAsync(
            $"/api/v1/Auth/assign-role?email={Uri.EscapeDataString(email)}&role={role}",
            content: null);

        var loginResponse = await client.PostAsJsonAsync("/api/v1/Auth/login", new { email, password });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult!["token"]);

        return client;
    }

    private async Task<int> CreateTestPatientAsync(HttpClient authenticatedClient)
    {
        var request = new CreatePatientRequest("Risk Test Patient", new DateTime(1985, 1, 1), "Female");
        var response = await authenticatedClient.PostAsJsonAsync("/api/v1/Patients", request);
        var patient = await response.Content.ReadFromJsonAsync<PatientResponse>();
        return patient!.Id;
    }

    [Fact]
    public async Task CreateMedication_ReturnsForbidden_ForNurseRole()
    {
        // Arrange — a Nurse is authenticated (identity is known), but
        // MedicationsController.Create is restricted to Doctor only.
        var nurseClient = await CreateClientWithRoleAsync("Nurse");
        var patientId = await CreateTestPatientAsync(nurseClient);

        var request = new CreateMedicationRequest(patientId, "Metoprolol", 50, "Twice daily");

        // Act
        var response = await nurseClient.PostAsJsonAsync("/api/v1/Medications", request);

        // Assert — 403, not 401: the Nurse is genuinely authenticated,
        // just lacks the required role. Confusing these two is a common,
        // subtle authorization bug.
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateMedication_Succeeds_ForDoctorRole()
    {
        var doctorClient = await CreateClientWithRoleAsync("Doctor");
        var patientId = await CreateTestPatientAsync(doctorClient);

        var request = new CreateMedicationRequest(patientId, "Metoprolol", 50, "Twice daily");

        var response = await doctorClient.PostAsJsonAsync("/api/v1/Medications", request);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task DeleteMedication_ReturnsForbidden_ForNurseRole()
    {
        // The same boundary, checked on Delete specifically — Create and
        // Delete are separate [Authorize(Roles = "Doctor")] attributes in
        // the controller, so a regression could plausibly affect one
        // without affecting the other.
        var doctorClient = await CreateClientWithRoleAsync("Doctor");
        var patientId = await CreateTestPatientAsync(doctorClient);
        var createResponse = await doctorClient.PostAsJsonAsync(
            "/api/v1/Medications",
            new CreateMedicationRequest(patientId, "Metoprolol", 50, "Twice daily"));
        var medication = await createResponse.Content.ReadFromJsonAsync<MedicationResponse>();

        var nurseClient = await CreateClientWithRoleAsync("Nurse");

        var response = await nurseClient.DeleteAsync($"/api/v1/Medications/{medication!.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
