using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CardiacMonitoring.Api.DTOs.Patients;
using Xunit;

namespace CardiacMonitoring.Tests.Integration;

public class PatientsApiTests : IClassFixture<CardiacApiFactory>
{
    private readonly CardiacApiFactory _factory;

    public PatientsApiTests(CardiacApiFactory factory)
    {
        _factory = factory;
    }

    // Every test builds its own authenticated client with a unique email —
    // avoids collisions between tests and keeps each test fully
    // self-contained, matching how PatientsController is actually secured.
    private async Task<HttpClient> CreateAuthenticatedClientAsync(string emailPrefix)
    {
        var client = _factory.CreateClient();
        var email = $"{emailPrefix}.{Guid.NewGuid():N}@cardiac.test";
        var token = await _factory.RegisterAndLoginAsync(client, email, "TestPass@123");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    [Fact]
    public async Task GetById_ReturnsFullPatientBody_WhenPatientExists()
    {
        var client = await CreateAuthenticatedClientAsync("getbyid-happy");

        var createRequest = new CreatePatientRequest("Layla Ahmad", new DateTime(1990, 3, 12), "Female");
        var createResponse = await client.PostAsJsonAsync("/api/v1/Patients", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PatientResponse>();

        var response = await client.GetAsync($"/api/v1/Patients/{created!.Id}");

        response.EnsureSuccessStatusCode();
        var patient = await response.Content.ReadFromJsonAsync<PatientResponse>();

        Assert.NotNull(patient);
        Assert.Equal(created.Id, patient!.Id);
        Assert.Equal("Layla Ahmad", patient.FullName);
        Assert.Equal("Female", patient.Gender);
        Assert.Equal(new DateTime(1990, 3, 12), patient.DateOfBirth);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenPatientDoesNotExist()
    {
        var client = await CreateAuthenticatedClientAsync("getbyid-notfound");

        var response = await client.GetAsync("/api/v1/Patients/10000000");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsUnauthorized_WithoutAToken()
    {
        // A dedicated test confirming the protected-route boundary itself —
        // an unauthenticated client should never reach patient data.
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/Patients");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
