using CardiacMonitoring.Api.Data;
using CardiacMonitoring.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
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
            // Replace AppDbContext with SQLite in-memory
            var descriptorsToRemove = services
                .Where(d => d.ServiceType.FullName != null &&
                            d.ServiceType.FullName.Contains("AppDbContext"))
                .ToList();
            foreach (var descriptor in descriptorsToRemove)
                services.Remove(descriptor);
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));

            // Replace Redis-backed IDistributedCache with in-memory for tests.
            // Tests are not verifying caching behaviour — they verify endpoint
            // correctness. Redis being unavailable in the test environment must
            // not cause 500 errors on any endpoint that injects ICacheService.
            var redisDescriptors = services
                .Where(d => d.ServiceType.FullName != null &&
                           (d.ServiceType.FullName.Contains("DistributedCache") ||
                            d.ServiceType.FullName.Contains("RedisCache") ||
                            d.ServiceType.FullName.Contains("StackExchange") ||
                            d.ImplementationType?.FullName?.Contains("Redis") == true))
                .ToList();
            foreach (var descriptor in redisDescriptors)
                services.Remove(descriptor);

            // In-memory distributed cache — same interface, no Redis needed
            services.AddDistributedMemoryCache();

            // Replace ICacheService singleton (was registered against Redis)
            var cacheServiceDescriptors = services
                .Where(d => d.ServiceType == typeof(ICacheService))
                .ToList();
            foreach (var descriptor in cacheServiceDescriptors)
                services.Remove(descriptor);
            services.AddSingleton<ICacheService, CacheService>();

            // Replace rate limiter with unlimited thresholds for tests
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
