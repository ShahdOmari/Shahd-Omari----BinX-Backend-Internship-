using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CardiacMonitoring.Api.DTOs.Patients;
using CardiacMonitoring.Api.DTOs.VitalSigns;
using Xunit;

namespace CardiacMonitoring.Tests.Integration;

[Collection("Sequential")]
public class CoverageGapTests : IClassFixture<CardiacApiFactory>
{
    private readonly CardiacApiFactory _factory;

    public CoverageGapTests(CardiacApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsWrong()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/Auth/login", new
        {
            email = "admin@cardiac.com",
            password = "WrongPassword999!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenEmailAlreadyExists()
    {
        var client = _factory.CreateClient();
        var email = $"duplicate.{Guid.NewGuid():N}@cardiac.test";
        var payload = new { email, password = "TestPass@123", department = "Cardiology" };

        var first = await client.PostAsJsonAsync("/api/v1/Auth/register", payload);
        first.EnsureSuccessStatusCode();

        var second = await client.PostAsJsonAsync("/api/v1/Auth/register", payload);

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task GetCriticalPatients_ReturnsForbidden_ForNurseRole()
    {
        var client = _factory.CreateClient();
        var email = $"critical.nurse.{Guid.NewGuid():N}@cardiac.test";
        var token = await _factory.RegisterAndLoginAsync(client, email, "TestPass@123");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/VitalSigns/critical");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetCriticalPatients_ReturnsSuccess_ForAdminRole()
    {
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/v1/Auth/login",
            new { email = "admin@cardiac.com", password = "AdminPass123!" });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult!["token"]);

        var response = await client.GetAsync("/api/v1/VitalSigns/critical");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task DeletePatient_ReturnsForbidden_ForNurseRole()
    {
        var client = _factory.CreateClient();
        var email = $"delete.nurse.{Guid.NewGuid():N}@cardiac.test";
        var token = await _factory.RegisterAndLoginAsync(client, email, "TestPass@123");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var created = await client.PostAsJsonAsync("/api/v1/Patients",
            new CreatePatientRequest("Delete Test Patient", new DateTime(1980, 1, 1), "Male"));
        var patient = await created.Content.ReadFromJsonAsync<PatientResponse>();

        var response = await client.DeleteAsync($"/api/v1/Patients/{patient!.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateVitalSign_ReturnsCreated_WithValidData()
    {
        var client = _factory.CreateClient();
        var email = $"vitals.nurse.{Guid.NewGuid():N}@cardiac.test";
        var token = await _factory.RegisterAndLoginAsync(client, email, "TestPass@123");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var patientResponse = await client.PostAsJsonAsync("/api/v1/Patients",
            new CreatePatientRequest("Vitals Test Patient", new DateTime(1975, 6, 15), "Female"));
        var patient = await patientResponse.Content.ReadFromJsonAsync<PatientResponse>();

        var vitalRequest = new CreateVitalSignRequest(
            PatientId: patient!.Id,
            HeartRateBpm: 78,
            SystolicBp: 120,
            DiastolicBp: 80,
            OxygenSaturationPercent: 98.0);

        var response = await client.PostAsJsonAsync("/api/v1/VitalSigns", vitalRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains($"\"{patient.Id}\"", body.Replace(patient.Id.ToString(), $"\"{patient.Id}\""));
        Assert.Contains("78", body);
    }

    [Fact]
    public async Task GetVitalSignById_ReturnsForbidden_ForDifferentNurse()
    {
        var clientA = _factory.CreateClient();
        var emailA = $"owner.nurseA.{Guid.NewGuid():N}@cardiac.test";
        var tokenA = await _factory.RegisterAndLoginAsync(clientA, emailA, "TestPass@123");
        clientA.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenA);

        var patientResponse = await clientA.PostAsJsonAsync("/api/v1/Patients",
            new CreatePatientRequest("Ownership Test Patient", new DateTime(1970, 1, 1), "Male"));
        var patient = await patientResponse.Content.ReadFromJsonAsync<PatientResponse>();

        var vitalRequest = new CreateVitalSignRequest(patient!.Id, 72, 118, 78, 97.5);
        var createResponse = await clientA.PostAsJsonAsync("/api/v1/VitalSigns", vitalRequest);
        var vitalBody = await createResponse.Content.ReadAsStringAsync();
        var vitalId = System.Text.Json.JsonDocument.Parse(vitalBody).RootElement.GetProperty("id").GetInt32();

        var clientB = _factory.CreateClient();
        var emailB = $"owner.nurseB.{Guid.NewGuid():N}@cardiac.test";
        var tokenB = await _factory.RegisterAndLoginAsync(clientB, emailB, "TestPass@123");
        clientB.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenB);

        var response = await clientB.GetAsync($"/api/v1/VitalSigns/{vitalId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetVitalSignById_ReturnsSuccess_ForRecordingNurse()
    {
        var client = _factory.CreateClient();
        var email = $"owner.self.{Guid.NewGuid():N}@cardiac.test";
        var token = await _factory.RegisterAndLoginAsync(client, email, "TestPass@123");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var patientResponse = await client.PostAsJsonAsync("/api/v1/Patients",
            new CreatePatientRequest("Self Ownership Patient", new DateTime(1980, 5, 20), "Female"));
        var patient = await patientResponse.Content.ReadFromJsonAsync<PatientResponse>();

        var vitalRequest = new CreateVitalSignRequest(patient!.Id, 80, 122, 82, 96.0);
        var createResponse = await client.PostAsJsonAsync("/api/v1/VitalSigns", vitalRequest);
        var vitalBody = await createResponse.Content.ReadAsStringAsync();
        var vitalId = System.Text.Json.JsonDocument.Parse(vitalBody).RootElement.GetProperty("id").GetInt32();

        var response = await client.GetAsync($"/api/v1/VitalSigns/{vitalId}");

        response.EnsureSuccessStatusCode();
    }
}

