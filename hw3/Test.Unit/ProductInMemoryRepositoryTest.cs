using Domain.Dtos;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Infrastructure.Repositories;
using Test.Unit.fixtures;

namespace Test.Unit;

public class ProductInMemoryRepositoryTest : IClassFixture<ProductsFixture>
{
    private readonly ProductsFixture _fixture;
    private readonly IProductRepository _repository;

    public ProductInMemoryRepositoryTest(ProductsFixture fixture)
    {
        _fixture = fixture;
        _repository = new ProductInMemoryRepository(fixture.ProductList);
    }

    [Fact]
    public void TestGetListWhenEmptyFilters()
    {
        var emptyFilters = new FilterProductsDto(null, null, null, null, 0, 0);
        var expected = new PaginatedListDto<Product>(1, 1, _fixture.ProductList);
        var actual = _repository.GetList(emptyFilters);

        actual.Page.Should().Be(expected.Page);
        actual.TotalPages.Should().Be(expected.TotalPages);
        actual.Records.Should().Equal(expected.Records);
    }

    [Theory]
    [InlineData(0, 0, 1, 1, 10)]
    [InlineData(1, 10, 1, 1, 10)]
    [InlineData(1, 1, 1, 10, 1)]
    [InlineData(11, 1, 10, 10, 1)]
    [InlineData(-2, 5, 1, 2, 5)]
    [InlineData(2, 1, 2, 10, 1)]
    [InlineData(2, 2, 2, 5, 2)]
    [InlineData(2, 5, 2, 2, 5)]
    [InlineData(3, 3, 3, 4, 3)]
    [InlineData(4, 3, 4, 4, 1)]
    public void TestGetListPaging(int page, int recordsPerPage, int expectedPage, int expectedTotalPages, int expected)
    {
        var filters = new FilterProductsDto(null, null, null, null, page, recordsPerPage);
        var actual = _repository.GetList(filters);

        actual.Page.Should().Be(expectedPage);
        actual.TotalPages.Should().Be(expectedTotalPages);
        actual.Records.Count.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, null, ProductCategory.General, null, 5)]
    [InlineData(null, null, ProductCategory.Food, null, 5)]
    [InlineData(null, null, ProductCategory.HouseholdChemicals, null, 0)]
    [InlineData(null, null, null, 1, 7)]
    [InlineData(null, null, null, 2, 3)]
    [InlineData(null, null, null, 3, 0)]
    [InlineData(-1, 0, null, null, 1)]
    [InlineData(-7, -1, null, null, 3)]
    [InlineData(-1000, -100, null, null, 0)]
    [InlineData(-10, 0, ProductCategory.General, 1, 2)]
    [InlineData(-20, -10, ProductCategory.Food, 2, 1)]
    public void TestGetListWhenFiltering(int? fromAddNow, int? toAddNow, ProductCategory? category, int? warehouseId,
        int expected)
    {
        var from = GetDateTimeFromNullableIntAddedToNow(fromAddNow);
        var to = GetDateTimeFromNullableIntAddedToNow(toAddNow);
        var filters = new FilterProductsDto(from, to, category, warehouseId, 1, 100);
        var actual = _repository.GetList(filters);

        if (from is not null) Assert.True(actual.Records.All(x => x.CreatedAt >= from));

        if (to is not null) Assert.True(actual.Records.All(x => x.CreatedAt <= to));

        if (category is not null) Assert.True(actual.Records.All(x => x.Category == category));

        if (warehouseId is not null) Assert.True(actual.Records.All(x => x.WarehouseId == warehouseId));

        actual.Records.Count.Should().Be(expected);

        return;

        DateTime? GetDateTimeFromNullableIntAddedToNow(int? arg)
        {
            if (arg is null) return null;
            return DateTime.Now.AddDays((double)arg);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(17)]
    public void TestGetById(int id)
    {
        var expected = _fixture.ProductList.FirstOrDefault(x => x.Id == id);
        var actual = _repository.GetById(id);

        actual.Should().Be(expected);
    }

    [Fact]
    public void TestCreateSuccessful()
    {
        var createDto = new ProductCreateDto("TestName", 10m, 1, ProductCategory.Appliances, 1);
        var actual = _repository.Create(createDto);

        actual.Name.Should().Be(createDto.Name);
        actual.Price.Should().Be(createDto.Price);
        actual.Weight.Should().Be(createDto.Weight);
        actual.Category.Should().Be(createDto.Category);
        actual.WarehouseId.Should().Be(createDto.WarehouseId);
    }

    [Fact]
    public void TestCreateThatIdAndCreatedAtWasAppointed()
    {
        var expectedId = _fixture.ProductList.Count + 1;
        var expectedDate = DateTime.Now;

        var createDto = new ProductCreateDto("TestName", 10m, 1, ProductCategory.Appliances, 1);
        var actual = _repository.Create(createDto);

        actual.Id.Should().Be(expectedId);
        (actual.CreatedAt - expectedDate).Should().BeLessThan(TimeSpan.FromMinutes(10));
    }

    [Fact]
    public void TestCreateThatCreatedStored()
    {
        var createDto = new ProductCreateDto("TestName", 10m, 1, ProductCategory.Appliances, 1);
        var expected = _repository.Create(createDto);
        var actual = _repository.GetById(expected.Id);

        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(7)]
    public void TestUpdate(int id)
    {
        var expected = _fixture.ProductList[id] with { Price = 100 };
        var actual = _repository.Update(id, expected);

        actual.Should().Be(expected);
    }
}