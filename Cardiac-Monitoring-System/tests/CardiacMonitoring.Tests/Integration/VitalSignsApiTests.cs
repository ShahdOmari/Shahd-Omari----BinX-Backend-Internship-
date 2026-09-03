using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace CardiacMonitoring.Tests.Integration;

// Dedicated test class for a single concern: does the [Authorize]
// boundary on a protected endpoint actually behave as expected end-to-end,
// with a real issued JWT — not a mocked or bypassed authentication scheme.
public class VitalSignsApiTests : IClassFixture<CardiacApiFactory>
{
    private readonly CardiacApiFactory _factory;

    public VitalSignsApiTests(CardiacApiFactory factory)
    {
        _factory = factory;
    }

    // Sprint 2, Day 3: GetAll is now restricted to Doctor/Auditor/Admin —
    // a plain Nurse account (the registration default) is deliberately
    // excluded, so this test authenticates as the seeded Admin account
    // instead of a freshly self-registered one.
    [Fact]
    public async Task GetAll_ReturnsSuccess_WhenRequestCarriesAPrivilegedJwt()
    {
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            new { email = "admin@cardiac.com", password = "AdminPass123!" });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult!["token"]);

        var response = await client.GetAsync("/api/v1/VitalSigns");

        response.EnsureSuccessStatusCode();
    }

    // The ownership/RBAC counterpart to the success case above — a
    // regular Nurse is genuinely authenticated but lacks the Doctor/
    // Auditor/Admin role this endpoint now requires, so it must be 403,
    // not 401.
    [Fact]
    public async Task GetAll_ReturnsForbidden_ForPlainNurseRole()
    {
        var client = _factory.CreateClient();
        var email = $"vitalsigns.nurse.{Guid.NewGuid():N}@cardiac.test";
        var token = await _factory.RegisterAndLoginAsync(client, email, "TestPass@123");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/VitalSigns");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsUnauthorized_WhenNoTokenIsAttached()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/VitalSigns");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsUnauthorized_WhenTokenIsMalformed()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "this-is-not-a-real-jwt");

        var response = await client.GetAsync("/api/v1/VitalSigns");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
