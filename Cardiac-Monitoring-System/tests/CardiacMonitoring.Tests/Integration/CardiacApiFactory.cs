using CardiacMonitoring.Api.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
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
            var descriptorsToRemove = services
                .Where(d => d.ServiceType.FullName != null &&
                            d.ServiceType.FullName.Contains("AppDbContext"))
                .ToList();
            foreach (var descriptor in descriptorsToRemove)
                services.Remove(descriptor);
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));

            // The production rate limiter policies ("general": 100/min,
            // "login": 5/min) are real security controls worth keeping in
            // Program.cs untouched, but they aren't what these integration
            // tests are verifying. The middleware and [EnableRateLimiting]
            // attributes require named policies to exist at runtime, so
            // rather than removing the registration, it's replaced
            // wholesale here with a fresh one defining the same two
            // policy names at effectively-unlimited thresholds.
            var rateLimiterOptionsDescriptors = services
                .Where(d => d.ServiceType.FullName != null &&
                            d.ServiceType.FullName.Contains("RateLimiterOptions"))
                .ToList();
            foreach (var descriptor in rateLimiterOptionsDescriptors)
                services.Remove(descriptor);

            services.Configure<RateLimiterOptions>(options =>
            {
                options.AddFixedWindowLimiter("general", opt =>
                {
                    opt.PermitLimit = 100_000;
                    opt.Window = TimeSpan.FromMinutes(1);
                });
                options.AddFixedWindowLimiter("login", opt =>
                {
                    opt.PermitLimit = 100_000;
                    opt.Window = TimeSpan.FromMinutes(1);
                });
            });

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

    public async Task<string> RegisterAndLoginAsync(HttpClient client, string email, string password, string department = "General")
    {
        var registerResponse = await client.PostAsJsonAsync("/api/v1/Auth/register", new { email, password, department });
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
