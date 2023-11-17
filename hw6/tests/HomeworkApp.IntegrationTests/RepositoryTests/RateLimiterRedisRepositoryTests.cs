using FluentAssertions;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.IntegrationTests.Fixtures;
using Xunit;

namespace HomeworkApp.IntegrationTests.RepositoryTests;

[Collection(nameof(TestFixture))]
public class RateLimiterRedisRepositoryTests
{
    private readonly IRateLimiterRepository _repository;

    private readonly int _maxRequests;

    public RateLimiterRedisRepositoryTests(TestFixture fixture)
    {
        _repository = fixture.RateLimiterRepository;
        _maxRequests = fixture.RateLimiterMaxRequestsPerTtl;
    }

    [Fact]
    public async Task IsAllowed_Success()
    {
        // Arrange
        var keySuffix = Guid.NewGuid().ToString();

        // Act
        var act = async () =>
            await _repository.IsAllowed(keySuffix, default);

        // Asserts
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task IsAllowed_False_WhenTooManyRequests()
    {
        // Arrange
        var requestsCount = _maxRequests + 1;
        var keySuffix = Guid.NewGuid().ToString();

        // Act
        var result = true;
        for (var i = 0; i < requestsCount; i++)
        {
            result = await _repository.IsAllowed(keySuffix, default);
            await Task.Delay(1);
        }

        // Asserts
        result.Should().BeFalse();
    }
}