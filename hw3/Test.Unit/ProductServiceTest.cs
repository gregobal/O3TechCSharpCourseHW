using Domain.Dtos;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Services;
using Domain.Validators;
using FluentAssertions;
using FluentValidation;
using Moq;
using Test.Unit.fixtures;

namespace Test.Unit;

public class ProductServiceTest : IClassFixture<ProductsFixture>
{
    private readonly ProductsFixture _fixture;
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly IProductService _service;

    public ProductServiceTest(ProductsFixture fixture)
    {
        _fixture = fixture;
        _repositoryMock = new Mock<IProductRepository>();
        IValidator<UpdatePriceDto> priceUpdateValidator = new UpdatePriceDtoValidator();
        IValidator<ProductCreateDto> productCreateValidator = new ProductCreateDtoValidator();
        _service = new ProductService(_repositoryMock.Object, productCreateValidator, priceUpdateValidator);
    }

    [Fact]
    public void TestCreateSuccessful()
    {
        var createDto = new ProductCreateDto("TestName", 10m, 1, ProductCategory.General, 1);
        var expected = new Product(default, createDto.Name, createDto.Price, createDto.Weight, createDto.Category,
            createDto.WarehouseId, default);
        _repositoryMock.Setup(r => r.Create(createDto)).Returns(expected);

        var actual = _service.Create(createDto);

        actual.Should().BeEquivalentTo(expected, options =>
            options.ExcludingMissingMembers()
        );
    }

    [Fact]
    public void TestCreateWhenPriceValidationFailed()
    {
        var createDto = new ProductCreateDto("TestName", -1m, 1, ProductCategory.General, 1);

        var act = () => _service.Create(createDto);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void TestCreateWhenWeightValidationFailed()
    {
        var createDto = new ProductCreateDto("TestName", 1m, 0, ProductCategory.General, 1);

        var act = () => _service.Create(createDto);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void TestGetByIdWhenProductWithIdNotInRepository()
    {
        const int id = int.MaxValue;
        _repositoryMock.Setup(r => r.GetById(id)).Returns((Product?)null);

        var act = () => _service.GetById(id);

        act.Should().Throw<ProductNotFoundException>();
    }

    [Fact]
    public void TestUpdatePriceSuccessful()
    {
        const decimal newPrice = 100.1m;

        var product = _fixture.ProductList[1];
        var expected = product with { Price = newPrice };
        _repositoryMock.Setup(r => r.GetById(product.Id)).Returns(product);
        _repositoryMock.Setup(r => r.Update(product.Id, expected)).Returns(expected);

        var actual = _service.UpdatePrice(product.Id, new UpdatePriceDto(newPrice));

        actual.Should().Be(expected);
    }

    [Fact]
    public void TestUpdatePriceWhenProductWithIdNotInRepository()
    {
        const int id = int.MaxValue;
        _repositoryMock.Setup(r => r.GetById(id)).Returns((Product?)null);

        var act = () => _service.UpdatePrice(id, new UpdatePriceDto(0));

        act.Should().Throw<ProductNotFoundException>();
    }

    [Fact]
    public void TestUpdatePriceWhenPriceValidationFailed()
    {
        var act = () => _service.UpdatePrice(1, new UpdatePriceDto(-1));

        act.Should().Throw<ValidationException>();
    }
}