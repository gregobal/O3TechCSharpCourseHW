using FluentAssertions;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities.ValueObjects;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Interfaces;
using Ozon.Route256.Kafka.OrderEventConsumer.IntegrationTests.Fixtures;
using Xunit;

namespace Ozon.Route256.Kafka.OrderEventConsumer.IntegrationTests;

[Collection(nameof(TestFixture))]
public class ItemRepositoryTests
{
    private readonly IItemRepository _repository;

    public ItemRepositoryTests(TestFixture fixture) => _repository = fixture.ItemRepository;

    [Fact]
    public async void UpsertItemsAccounting_Success()
    {
        // Arrange
        var itemsAccounting = new AccountingByStatus[]
        {
            new(new ItemId(1), 1, 0, 0),
            new(new ItemId(2), 1, 0, 0),
            new(new ItemId(1), 0, 0, 1),
            new(new ItemId(3), 2, 1, 1),
            new(new ItemId(3), 3, 2, 1)
        };

        // Act
        var result = await _repository.MergeAccountingByStatus(itemsAccounting, default);

        // Asserts
        result.Should().Be(3);
    }

    [Fact]
    public async void UpsertItemsSales_Success()
    {
        // Arrange
        var itemsSales = new AccountingBySeller[]
        {
            new(new SellerId(1), new ProductId(1), "RUR", 1.2m, 1),
            new(new SellerId(1), new ProductId(2), "RUR", 1.2m, 1),
            new(new SellerId(1), new ProductId(1), "USD", 1.2m, 1),
            new(new SellerId(2), new ProductId(1), "RUR", 1.2m, 1),
            new(new SellerId(2), new ProductId(2), "RUR", 1.2m, 1),
            new(new SellerId(2), new ProductId(1), "USD", 1.2m, 1),
            new(new SellerId(2), new ProductId(2), "USD", 1.2m, 1),
            new(new SellerId(1), new ProductId(1), "RUR", 1.2m, 10),
            new(new SellerId(2), new ProductId(1), "RUR", 1.2m, 2)
        };

        // Act
        var result = await _repository.MergeAccountingBySeller(itemsSales, default);

        // Asserts
        result.Should().Be(7);
    }
}
