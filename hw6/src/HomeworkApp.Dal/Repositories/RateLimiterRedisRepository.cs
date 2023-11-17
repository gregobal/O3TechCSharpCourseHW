using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.Dal.Settings;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace HomeworkApp.Dal.Repositories;

public sealed class RateLimiterRedisRepository : RedisRepository, IRateLimiterRepository
{
    private const int DelayIfNotKeyExistsAndLockFailMs = 100;

    private readonly int _maxRequestsPerTtl;

    protected override TimeSpan KeyTtl { get; }

    protected override string KeyPrefix => "rate_limit";

    public RateLimiterRedisRepository(IOptions<DalOptions> dalSettings,
        IOptions<RateLimiterRedisOptions> rateLimiterSettings)
        : base(dalSettings.Value)
    {
        _maxRequestsPerTtl = rateLimiterSettings.Value.MaxRequestsPerTtl;
        KeyTtl = TimeSpan.FromSeconds(rateLimiterSettings.Value.KeyTtlSec);
    }

    public async Task<bool> IsAllowed(string keySuffix, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var connection = await GetConnection();

        var key = GetKey(keySuffix);

        await connection.StringSetAsync(key, _maxRequestsPerTtl, KeyTtl, When.NotExists);

        var actualRequestCount = await connection.StringDecrementAsync(key);

        return actualRequestCount > -1;
    }
}