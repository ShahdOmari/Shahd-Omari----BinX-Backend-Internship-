using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CardiacMonitoring.Api.DTOs.Patients;
using CardiacMonitoring.Api.DTOs.VitalSigns;
using Xunit;

namespace CardiacMonitoring.Tests.Integration;

// Second highest-risk area from Day 5's analysis: FluentValidation's
// async business rules (does the referenced PatientId actually exist?).
// These run differently from ordinary sync rules — see Day 4's
// FluentValidation writeup — and a regression here would let orphaned
// VitalSign/Medication/Appointment records reach the database with no
// real patient behind them.
public class BusinessRuleValidationTests : IClassFixture<CardiacApiFactory>
{
    private readonly CardiacApiFactory _factory;

    public BusinessRuleValidationTests(CardiacApiFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var email = $"validation.{Guid.NewGuid():N}@cardiac.test";
        const string password = "TestPass@123";

        await client.PostAsJsonAsync("/api/v1/Auth/register", new { email, password });
        var loginResponse = await client.PostAsJsonAsync("/api/v1/Auth/login", new { email, password });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult!["token"]);

        return client;
    }

    [Fact]
    public async Task CreateVitalSign_ReturnsBadRequest_WhenPatientIdDoesNotExist()
    {
        var client = await CreateAuthenticatedClientAsync();

        // No patient with this Id was ever created in this test — the
        // async MustAsync rule in CreateVitalSignValidator is expected to
        // catch this before anything reaches the database.
        var request = new CreateVitalSignRequest(
            PatientId: 999_999, HeartRateBpm: 75, SystolicBp: 120,
            DiastolicBp: 80, OxygenSaturationPercent: 98);

        var response = await client.PostAsJsonAsync("/api/v1/VitalSigns", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("does not exist", body);
    }

    [Fact]
    public async Task CreateVitalSign_ReturnsCreated_WhenPatientIdIsValid()
    {
        var client = await CreateAuthenticatedClientAsync();

        var patientRequest = new CreatePatientRequest("Validation Test Patient", new DateTime(1975, 6, 1), "Male");
        var patientResponse = await client.PostAsJsonAsync("/api/v1/Patients", patientRequest);
        var patient = await patientResponse.Content.ReadFromJsonAsync<PatientResponse>();

        var vitalRequest = new CreateVitalSignRequest(
            PatientId: patient!.Id, HeartRateBpm: 145, SystolicBp: 190,
            DiastolicBp: 100, OxygenSaturationPercent: 85);

var response = await client.PostAsJsonAsync("/api/v1/VitalSigns", vitalRequest);

response.EnsureSuccessStatusCode();

// JsonSerializerDefaults.Web matches ASP.NET Core's own default JSON
// behavior (camelCase, case-insensitive property matching) — the plain
// JsonSerializerOptions() constructor used earlier didn't enable
// case-insensitive property matching, which silently left every property
// at its default value instead of throwing, masking the real problem.
var jsonOptions = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web);
jsonOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());

var rawBody = await response.Content.ReadAsStringAsync();
var created = System.Text.Json.JsonSerializer.Deserialize<VitalSignResponse>(rawBody, jsonOptions);

Assert.Equal(CardiacMonitoring.Api.Entities.RiskLevel.Critical, created!.RiskLevel);    }
}
