using System.Collections.Concurrent;
using System.Text.Json;
using Fourteen.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Fourteen.Infrastructure.Services
{
    public sealed class MemoryQueryCache : IQueryCache
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<MemoryQueryCache> _logger;
        private readonly ConcurrentDictionary<string, ConcurrentBag<string>> _prefixIndex = new();
 
        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };
 
        public MemoryQueryCache(IMemoryCache cache, ILogger<MemoryQueryCache> logger)
        {
            _cache = cache;
            _logger = logger;
        }
 
        public Task<T?> Get<T>(string key, CancellationToken ct = default) where T : class
        {
            if (_cache.TryGetValue(key, out var raw) && raw is string json)
            {
                try
                {
                    var value = JsonSerializer.Deserialize<T>(json, _jsonOpts);
                    _logger.LogDebug("Cache HIT  key={Key}", key);
                    return Task.FromResult(value);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Cache deserialisation failed for key={Key}; evicting", key);
                    _cache.Remove(key);
                }
            }
 
            _logger.LogDebug("Cache MISS key={Key}", key);
            return Task.FromResult<T?>(null);
        }
 
        public Task Set<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default) where T : class
        {
            var json = JsonSerializer.Serialize(value, _jsonOpts);
 
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl,
                Size = 1
            };
 
            _cache.Set(key, json, options);
 
            var prefix = ExtractPrefix(key);
            if (prefix is not null)
            {
                var bag = _prefixIndex.GetOrAdd(prefix, _ => new ConcurrentBag<string>());
                bag.Add(key);
            }
 
            _logger.LogDebug("Cache SET  key={Key} ttl={Ttl}", key, ttl);
            return Task.CompletedTask;
        }
 
        public Task Remove(string key, CancellationToken ct = default)
        {
            _cache.Remove(key);
            _logger.LogDebug("Cache DEL  key={Key}", key);
            return Task.CompletedTask;
        }
 
        public Task RemoveByPrefix(string prefix, CancellationToken ct = default)
        {
            if (_prefixIndex.TryRemove(prefix, out var keys))
            {
                foreach (var key in keys)
                    _cache.Remove(key);
 
                _logger.LogDebug("Cache EVICT prefix={Prefix} count={Count}", prefix, keys.Count);
            }
 
            return Task.CompletedTask;
        }

        private static string? ExtractPrefix(string key)
        {
            var idx = key.IndexOf(':');
            return idx > 0 ? key[..idx] : null;
        }
    }
}