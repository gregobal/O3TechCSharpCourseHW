using System.Reflection;
using FluentAssertions;
using Moq;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities.ValueObjects;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Interfaces;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Services;
using Xunit;

namespace Ozon.Route256.Kafka.OrderEventConsumer.UnitTests;

public class ItemServiceTests
{
    private readonly Mock<IItemRepository> _repositoryMock = new();

    [Fact]
    public async void AccountingByStatus_ShouldReturnMergedInRepositoryRowsCount()
    {
        // Arrange
        const int expected = 1;

        var orderEvents = new List<OrderEvent>
        {
            new(
                new OrderId(1),
                default,
                default,
                Status.Created,
                default,
                new OrderEventPosition[] { new(new ItemId(111111222222), default, default) })
        };

        _repositoryMock.Setup(r => r.MergeAccountingByStatus(It.IsAny<IEnumerable<AccountingByStatus>>(), default))
            .ReturnsAsync(expected);

        var service = new ItemService(_repositoryMock.Object);

        // Act
        var results = await service.AccountingByStatus(orderEvents, default);

        // Asserts
        results.Should().Be(expected);
    }

    [Fact]
    public async void AccountingByStatus_ShouldReturn0_WhenEmptyArgList()
    {
        // Arrange
        var service = new ItemService(_repositoryMock.Object);

        // Act
        var results = await service.AccountingByStatus(new List<OrderEvent>(0), default);

        // Asserts
        results.Should().Be(0);
    }

    [Fact]
    public void GetAccountingFromOrderPositionByStatus_Success()
    {
        // Arrange
        var service = new ItemService(_repositoryMock.Object);
        var typ = typeof(ItemService);
        var method = typ.GetMethod(
            "GetAccountingFromOrderPositionByStatus",
            BindingFlags.NonPublic | BindingFlags.Instance);

        var itemId = new ItemId(1);
        var position = new OrderEventPosition(itemId, 1, default);
        var args1 = new object[] { position, Status.Created };
        var args2 = new object[] { position, Status.Delivered };
        var args3 = new object[] { position, Status.Canceled };

        var expected1 = new AccountingByStatus(itemId, 1, 0, 0);
        var expected2 = new AccountingByStatus(itemId, 0, 1, 0);
        var expected3 = new AccountingByStatus(itemId, 0, 0, 1);

        // Act
        var result1 = method?.Invoke(service, args1) as AccountingByStatus;
        var result2 = method?.Invoke(service, args2) as AccountingByStatus;
        var result3 = method?.Invoke(service, args3) as AccountingByStatus;

        // Asserts
        result1.Should().BeEquivalentTo(expected1);
        result2.Should().BeEquivalentTo(expected2);
        result3.Should().BeEquivalentTo(expected3);
    }

    [Fact]
    public async void AccountingBySeller_ShouldReturnMergedInRepositoryRowsCount()
    {
        // Arrange
        const int expected = 1;

        var orderEvents = new List<OrderEvent>
        {
            new(
                new OrderId(1),
                default,
                default,
                Status.Delivered,
                default,
                new OrderEventPosition[] { new(new ItemId(111111222222), default, default) })
        };

        _repositoryMock.Setup(r => r.MergeAccountingBySeller(It.IsAny<IEnumerable<AccountingBySeller>>(), default))
            .ReturnsAsync(expected);

        var service = new ItemService(_repositoryMock.Object);

        // Act
        var results = await service.AccountingBySeller(orderEvents, default);

        // Asserts
        results.Should().Be(expected);
    }

    [Fact]
    public async void AccountingBySeller_ShouldReturn0_WhenEmptyArgList()
    {
        // Arrange
        var service = new ItemService(_repositoryMock.Object);

        // Act
        var results = await service.AccountingBySeller(new List<OrderEvent>(0), default);

        // Asserts
        results.Should().Be(0);
    }
}
