using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CardiacMonitoring.Api.DTOs.VitalSigns;
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

    [Fact]
    public async Task GetAll_ReturnsSuccess_WhenRequestCarriesAValidJwt()
    {
        // Arrange — a real client that first goes through the real
        // register/login flow, exactly as a genuine caller would, then
        // attaches the resulting JWT as a bearer token on the request.
        var client = _factory.CreateClient();
        var email = $"vitalsigns.auth.{Guid.NewGuid():N}@cardiac.test";
        var token = await _factory.RegisterAndLoginAsync(client, email, "TestPass@123");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/v1/VitalSigns");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetAll_ReturnsUnauthorized_WhenNoTokenIsAttached()
    {
        // A plain client with no Authorization header at all — the
        // baseline negative case every protected endpoint must reject.
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/VitalSigns");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsUnauthorized_WhenTokenIsMalformed()
    {
        // A token-shaped string that isn't a real, validly-signed JWT —
        // confirms the API actually validates the token's signature and
        // structure, rather than just checking that "something" was sent
        // in the Authorization header.
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "this-is-not-a-real-jwt");

        var response = await client.GetAsync("/api/v1/VitalSigns");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
