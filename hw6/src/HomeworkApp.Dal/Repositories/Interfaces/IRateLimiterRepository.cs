namespace HomeworkApp.Dal.Repositories.Interfaces;

public interface IRateLimiterRepository
{
    Task<bool> IsAllowed(string keySuffix, CancellationToken token);
}