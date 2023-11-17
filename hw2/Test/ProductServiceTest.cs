using Domain.Dtos;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Services;
using Domain.Validators;
using FluentValidation;

namespace Test;

[TestClass]
public class ProductServiceTest
{
    private readonly IValidator<UpdatePriceDto> _priceUpdateValidator = new UpdatePriceDtoValidator();

    private readonly IValidator<ProductCreateDto> _productCreateValidator = new ProductCreateDtoValidator();

    private readonly IQueryable<Product> _repoTestDate = new[]
    {
        new Product(1, "Product1", 1m, 1, ProductCategory.General, 1, DateTime.Now.AddDays(-10)),
        new Product(2, "Product1", 2m, 2, ProductCategory.General, 2, DateTime.Now.AddDays(-20)),
        new Product(3, "Product1", 3m, 3, ProductCategory.General, 1, DateTime.Now.AddDays(-30)),
        new Product(4, "Product1", 4m, 4, ProductCategory.General, 2, DateTime.Now.AddDays(-40)),
        new Product(5, "Product1", 5m, 5, ProductCategory.General, 1, DateTime.Now.AddDays(-50)),
        new Product(6, "Product1", 4m, 4, ProductCategory.Appliances, 2, DateTime.Now.AddDays(-40)),
        new Product(7, "Product1", 5m, 5, ProductCategory.Appliances, 1, DateTime.Now.AddDays(-50))
    }.AsQueryable();

    private Mock<IProductRepository> _repositoryMock = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _repositoryMock = new Mock<IProductRepository>();
    }

    [TestMethod]
    public void TestCreateSuccessful()
    {
        var input = new ProductCreateDto("Test", 1m, 1, ProductCategory.General, 1);

        var expected = new Product(1, input.Name, input.Price, input.Weight, input.Category, input.WarehouseId,
            DateTime.Now);

        _repositoryMock.Setup(r => r.Create(input))
            .Returns(expected);

        var service = new ProductService(_repositoryMock.Object, _productCreateValidator, _priceUpdateValidator);
        var actual = service.Create(input);

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public void TestCreateValidationFailed()
    {
        var input = new ProductCreateDto("Test", 1m, 0, ProductCategory.General, 1);
        var service = new ProductService(_repositoryMock.Object, _productCreateValidator, _priceUpdateValidator);
        service.Create(input);
    }

    [TestMethod]
    public void TestGetList()
    {
        const int page = 2;
        const int perPage = 2;

        var input = new FilterProductsDto(null, null, null, null, page, perPage);

        var expected = new PaginatedListDto<Product>(2, 4,
            _repoTestDate.Skip((page - 1) * perPage).Take(perPage).ToList()
        );

        _repositoryMock.Setup(r => r.GetList(input))
            .Returns(expected);

        var service = new ProductService(_repositoryMock.Object, _productCreateValidator, _priceUpdateValidator);
        var actual = service.GetList(input);

        Assert.AreEqual(expected.Page, actual.Page);
        Assert.AreEqual(expected.TotalPages, actual.TotalPages);
        CollectionAssert.AreEqual(expected.Records.ToArray(), actual.Records.ToArray());
    }

    [TestMethod]
    public void TestGetByIdSuccessful()
    {
        var input = 1;

        var expected = _repoTestDate.First(x => x.Id == input);

        _repositoryMock.Setup(r => r.GetById(input))
            .Returns(expected);

        var service = new ProductService(_repositoryMock.Object, _productCreateValidator, _priceUpdateValidator);
        var actual = service.GetById(input);

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [ExpectedException(typeof(ProductNotFoundException))]
    public void TestGetByIdWhenProductNotFound()
    {
        var input = 1;
        var service = new ProductService(_repositoryMock.Object, _productCreateValidator, _priceUpdateValidator);
        service.GetById(input);
    }

    [TestMethod]
    public void TestUpdatePriceSuccessful()
    {
        var input = (1, new UpdatePriceDto(10m));

        var product = _repoTestDate.First(x => x.Id == input.Item1);
        var expected = product with { Price = input.Item2.Price };

        _repositoryMock.Setup(r => r.GetById(input.Item1))
            .Returns(product);
        _repositoryMock
            .Setup(r => r.Update(input.Item1, expected))
            .Returns(expected);

        var service = new ProductService(_repositoryMock.Object, _productCreateValidator, _priceUpdateValidator);
        var actual = service.UpdatePrice(input.Item1, input.Item2);

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public void TestUpdatePriceValidationFailed()
    {
        var input = (1, new UpdatePriceDto(-10m));
        var service = new ProductService(_repositoryMock.Object, _productCreateValidator, _priceUpdateValidator);
        service.UpdatePrice(input.Item1, input.Item2);
    }

    [TestMethod]
    [ExpectedException(typeof(ProductNotFoundException))]
    public void TestUpdatePriceFailedWhenProductNotFound()
    {
        var input = (1, new UpdatePriceDto(10m));
        var service = new ProductService(_repositoryMock.Object, _productCreateValidator, _priceUpdateValidator);
        service.UpdatePrice(input.Item1, input.Item2);
    }
}