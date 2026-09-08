using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CardiacMonitoring.Api.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;

    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(10);

    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var cached = await _cache.GetStringAsync(key);
            if (cached is null)
            {
                _logger.LogInformation("CACHE MISS | key={Key}", key);
                return default;
            }
            _logger.LogInformation("CACHE HIT  | key={Key}", key);
            return JsonSerializer.Deserialize<T>(cached);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CACHE ERROR on GET | key={Key} — falling through to DB", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? DefaultExpiry
            };
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(value), options);
            _logger.LogInformation("CACHE SET  | key={Key} | expiry={Expiry}", key, expiry ?? DefaultExpiry);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CACHE ERROR on SET | key={Key} — write skipped", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
            _logger.LogInformation("CACHE DEL  | key={Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CACHE ERROR on DEL | key={Key}", key);
        }
    }
}
