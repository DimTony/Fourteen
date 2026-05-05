using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Fourteen.Application.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace Fourteen.Infrastructure.Services
{
    public class RedisServices : IRedisService
    {
        private readonly ILogger<RedisServices> _logger;
        private readonly IConnectionMultiplexer _redis;


        public RedisServices(ILogger<RedisServices> logger, IConnectionMultiplexer redis)
        {
            _logger = logger;
            _redis = redis;
        }

        public async Task PublishJob<T>(string queue, T message, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(queue))
                throw new ArgumentException("Queue name is required", nameof(queue));

            try
            {
                var db = _redis.GetDatabase();

                var json = JsonSerializer.Serialize(message);

                await db.ListRightPushAsync(queue, json);

                _logger.LogInformation("Message published to Redis queue {Queue}", queue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to push message to Redis queue {Queue}", queue);
                throw;
            }
        }


    }
}