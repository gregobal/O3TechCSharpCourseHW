using System.Globalization;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Services;
using Moq;

namespace Test;

[TestClass]
public class SaleHistoryServiceTest
{
    private Mock<ISaleHistoryRepository> _saleHistoryMock = null!;
    private Mock<IMonthFactorRepository> _monthFactorsMock = null!;

    [TestInitialize]
    public void Startup()
    {
        _saleHistoryMock = new Mock<ISaleHistoryRepository>();
        _monthFactorsMock = new Mock<IMonthFactorRepository>();
    }

    [TestMethod]
    public void TestCalculateAdsWhenProductWithIdWasInStockAndHadSales()
    {
        const int id = 123;
        var now = DateTime.Now;
        var sales = new[] { 10, 20, 30 };

        _saleHistoryMock.Setup(r => r.GetAllByIdAndInStock(id))
            .Returns(new List<SaleHistory>
            {
                new(id, DateOnly.FromDateTime(now), sales[0], 50),
                new(id, DateOnly.FromDateTime(now.AddDays(-1)), sales[1], 70),
                new(id, DateOnly.FromDateTime(now.AddMonths(-1)), sales[2], 100),
            }.AsQueryable);

        var expected = sales.Sum() * 1m / sales.Length;

        var service = new SaleAnalyticsService(_saleHistoryMock.Object, _monthFactorsMock.Object);
        var actual = service.CalculateAds(123);

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestCalculateAdsWhenProductWithIdNotInStock()
    {
        const int id = 123;

        _saleHistoryMock.Setup(r => r.GetAllByIdAndInStock(id))
            .Returns(new List<SaleHistory>
            {
                new(id, DateOnly.FromDateTime(DateTime.Now), 0, 0),
            }.AsQueryable);

        var expected = 0;

        var service = new SaleAnalyticsService(_saleHistoryMock.Object, _monthFactorsMock.Object);
        var actual = service.CalculateAds(123);

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestCalculateAdsWhenProductWithIdInStockButDidntHaveSales()
    {
        const int id = 123;

        _saleHistoryMock.Setup(r => r.GetAllByIdAndInStock(id))
            .Returns(new List<SaleHistory>
            {
                new(id, DateOnly.FromDateTime(DateTime.Now), 0, 10),
            }.AsQueryable);

        var expected = 0;

        var service = new SaleAnalyticsService(_saleHistoryMock.Object, _monthFactorsMock.Object);
        var actual = service.CalculateAds(123);

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestCalculateSalesPredictionWhenProductWithIdWasInStockAndHadSales()
    {
        const int id = 123;

        var calendar = new GregorianCalendar();
        var now = DateTime.Now;
        var daysInThisMonth = calendar.GetDaysInMonth(now.Year, now.Month) - now.Day + 1;
        var daysInNextMonth = calendar.GetDaysInMonth(now.Year, now.AddMonths(1).Month);

        var sales = new[] { 10, 10 };
        var ads = sales.Sum() / sales.Length;
        var coef = 0.5m;
        var dateWithCoef = DateOnly.FromDateTime(now.AddMonths(-11));

        _saleHistoryMock.Setup(r => r.GetAllByIdAndInStock(id))
            .Returns(new List<SaleHistory>
            {
                new(id, DateOnly.FromDateTime(now.AddMonths(-12)), sales[0], 50),
                new(id, dateWithCoef, sales[1], 100),
            }.AsQueryable);

        _monthFactorsMock.Setup(r => r.GetAllById(id))
            .Returns(new List<MonthFactor>
            {
                new(id, dateWithCoef.Month, coef),
            }.AsQueryable);

        var expected = ads * daysInThisMonth + ads * daysInNextMonth * coef;

        var service = new SaleAnalyticsService(_saleHistoryMock.Object, _monthFactorsMock.Object);
        var actual = service.CalculateSalesPrediction(id, daysInThisMonth + daysInNextMonth);

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestCalculateDemandWhenProductWithIdWasInStockAndHadSales()
    {
        const int id = 123;
        var sale = 10;
        var stock = 10;
        var now = DateOnly.FromDateTime(DateTime.Now);
        var days = 30;

        _saleHistoryMock.Setup(r => r.GetAllByIdAndInStock(id))
            .Returns(new List<SaleHistory>
            {
                new(id, now, sale, stock),
            }.AsQueryable);

        var expected = sale * days - stock;

        var service = new SaleAnalyticsService(_saleHistoryMock.Object, _monthFactorsMock.Object);
        var actual = service.CalculateDemand(123, days);

        Assert.AreEqual(expected, actual);
    }

    [DataTestMethod]
    [DataRow(5, 10)]
    [DataRow(10, 10)]
    [DataRow(15, 5)]
    [DataRow(17, 3)]
    [DataRow(20, 0)]
    [DataRow(25, 0)]
    public void TestCalculateDemandWhenProductWithIdWasInStockAndHadSalesWithSupplyDate(int supplyDays, int expected)
    {
        const int id = 123;
        var sale = 1;
        var stock = 10;
        var now = DateOnly.FromDateTime(DateTime.Now);
        var days = 20;
        var supplyDate = now.AddDays(supplyDays);

        _saleHistoryMock.Setup(r => r.GetAllByIdAndInStock(id))
            .Returns(new List<SaleHistory>
            {
                new(id, now, sale, stock),
            }.AsQueryable);

        var service = new SaleAnalyticsService(_saleHistoryMock.Object, _monthFactorsMock.Object);
        var actual = service.CalculateDemand(123, days, supplyDate);

        Assert.AreEqual(expected, actual);
    }
}