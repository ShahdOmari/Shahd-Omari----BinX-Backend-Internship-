using CardiacMonitoring.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; 
using System.Net.Http.Json;

namespace CardiacMonitoring.Tests.Integration;

public class CardiacApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.ConfigureServices(services =>
        {
            // AddDbContext registers more than one service descriptor for
            // SQL Server internally (options, extension services, etc.) —
            // removing just the DbContextOptions<AppDbContext> entry alone
            // leaves the others behind, causing EF Core to see two
            // providers registered at once. Removing every descriptor tied
            // to AppDbContext (by namespace) clears all of them at once.
            var descriptorsToRemove = services
                .Where(d => d.ServiceType.FullName != null &&
                            d.ServiceType.FullName.Contains("AppDbContext"))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }  

    
// Shared helper so every integration test that needs an authenticated
// client can get one in a single line, instead of repeating the
// register/login HTTP calls in every test class that needs auth.

// Shared helper so every integration test that needs an authenticated
// client can get one in a single line. Checks each step explicitly and
// throws a descriptive exception on failure — a bare ReadFromJsonAsync
// on a non-2xx response (e.g. an empty 429 body) fails with a confusing
// "no JSON tokens" error that hides the real problem.
public async Task<string> RegisterAndLoginAsync(HttpClient client, string email, string password)
{
    var registerResponse = await client.PostAsJsonAsync("/api/v1/Auth/register", new { email, password });
    var registerBody = await registerResponse.Content.ReadAsStringAsync();
    if (!registerResponse.IsSuccessStatusCode)
        throw new InvalidOperationException(
            $"Register failed: {(int)registerResponse.StatusCode} {registerResponse.StatusCode}. Body: {registerBody}");

    var loginResponse = await client.PostAsJsonAsync("/api/v1/Auth/login", new { email, password });
    var loginBody = await loginResponse.Content.ReadAsStringAsync();
    if (!loginResponse.IsSuccessStatusCode)
        throw new InvalidOperationException(
            $"Login failed: {(int)loginResponse.StatusCode} {loginResponse.StatusCode}. Body: {loginBody}");

    var loginResult = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(loginBody);
    return loginResult!["token"];
}
}
