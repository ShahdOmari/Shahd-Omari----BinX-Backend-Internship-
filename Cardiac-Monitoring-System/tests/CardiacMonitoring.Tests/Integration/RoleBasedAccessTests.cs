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
//
// Collection attribute forces these tests to run sequentially rather than
// in parallel — several of them promote a fresh account to Doctor via the
// shared seeded Admin account and login endpoint, which is rate-limited
// to 5 requests/minute. Running them in parallel risks tripping that
// limiter and getting an empty 429 response instead of a real token.
[Collection("Sequential")]
public class RoleBasedAccessTests : IClassFixture<CardiacApiFactory>
{
    private readonly CardiacApiFactory _factory;

    public RoleBasedAccessTests(CardiacApiFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateClientWithRoleAsync(string role)
    {
        var client = _factory.CreateClient();
        var email = $"{role.ToLower()}.{Guid.NewGuid():N}@cardiac.test";
        const string password = "TestPass@123";
        const string department = "Testing";

        var registerResponse = await client.PostAsJsonAsync("/api/v1/Auth/register", new { email, password, department });
        var registerBody = await registerResponse.Content.ReadAsStringAsync();
        if (!registerResponse.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Register failed: {(int)registerResponse.StatusCode} {registerResponse.StatusCode}. Body: {registerBody}");

        if (role != "Nurse")
        {
            var adminClient = await CreateAdminClientAsync();
            var assignResponse = await adminClient.PostAsync(
                $"/api/v1/Auth/assign-role?email={Uri.EscapeDataString(email)}&role={role}",
                content: null);
            var assignBody = await assignResponse.Content.ReadAsStringAsync();
            if (!assignResponse.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    $"Assign-role failed: {(int)assignResponse.StatusCode} {assignResponse.StatusCode}. Body: {assignBody}");
        }

        var loginResponse = await client.PostAsJsonAsync("/api/v1/Auth/login", new { email, password });
        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        if (!loginResponse.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Login failed: {(int)loginResponse.StatusCode} {loginResponse.StatusCode}. Body: {loginBody}");

        var loginResult = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(loginBody);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult!["token"]);

        return client;
    }

    private async Task<HttpClient> CreateAdminClientAsync()
    {
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            new { email = "admin@cardiac.com", password = "AdminPass123!" });
        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        if (!loginResponse.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Admin login failed: {(int)loginResponse.StatusCode} {loginResponse.StatusCode}. Body: {loginBody}");

        var loginResult = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(loginBody);

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
        var nurseClient = await CreateClientWithRoleAsync("Nurse");
        var patientId = await CreateTestPatientAsync(nurseClient);

        var request = new CreateMedicationRequest(patientId, "Metoprolol", 50, "Twice daily");

        var response = await nurseClient.PostAsJsonAsync("/api/v1/Medications", request);

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
