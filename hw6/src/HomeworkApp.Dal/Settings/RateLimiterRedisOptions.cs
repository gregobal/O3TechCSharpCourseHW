namespace HomeworkApp.Dal.Settings;

public record RateLimiterRedisOptions
{
    public int KeyTtlSec { get; init; }

    public int MaxRequestsPerTtl { get; init; }
}