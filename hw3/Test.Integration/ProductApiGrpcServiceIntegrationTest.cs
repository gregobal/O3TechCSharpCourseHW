using Domain.Interfaces;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcService;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Test.Integration.Fixtures;

namespace Test.Integration;

public class ProductApiGrpcServiceIntegrationTest : IClassFixture<WebApplicationFactory<Program>>,
    IClassFixture<ProductsFixture>
{
    private readonly ProductsFixture _fixture;
    private readonly ProductGrpcService.ProductGrpcServiceClient _grpcClient;

    public ProductApiGrpcServiceIntegrationTest(WebApplicationFactory<Program> factory,
        ProductsFixture fixture)
    {
        _fixture = fixture;

        var httpClient = factory.WithWebHostBuilder(builder => builder
                .ConfigureServices(services => services.Replace(
                    ServiceDescriptor.Singleton<IProductRepository>(_ =>
                        new ProductInMemoryRepository(_fixture.ProductList)))
                ))
            .CreateClient();

        var channel = GrpcChannel.ForAddress(httpClient.BaseAddress!, new GrpcChannelOptions
        {
            HttpClient = httpClient
        });

        _grpcClient = new ProductGrpcService.ProductGrpcServiceClient(channel);
    }

    [Fact]
    public void IntegrationTestCreateSuccessful()
    {
        var request = new CreateRequest
        {
            Name = "test", Price = new DecimalValue(0, 0), Weight = 1, WarehouseId = 1, Category = ProductCategory.Food
        };

        var response = _grpcClient.Create(request);
        response.Product.Id.Should().Be(_fixture.ProductList.Count + 1);
        response.Product.CreatedAt.ToDateTime().Date.Should().Be(DateTime.Now.Date);
        response.Product.Should().BeEquivalentTo(request, options =>
            options.ComparingByMembers<CreateRequest>());
    }

    [Fact]
    public void IntegrationTestCreateWhenNotValidPrice()
    {
        var request = new CreateRequest
        {
            Name = "test", Price = -1, Weight = 1, WarehouseId = 1, Category = ProductCategory.General
        };

        var response = () => _grpcClient.Create(request);
        response.Should().Throw<RpcException>()
            .And.StatusCode.Should().Be(StatusCode.InvalidArgument);
    }

    [Fact]
    public void IntegrationTestCreateWhenNotValidWeight()
    {
        var request = new CreateRequest
        {
            Name = "test", Price = 1, Weight = 0, WarehouseId = 1, Category = ProductCategory.General
        };

        var response = () => _grpcClient.Create(request);
        response.Should().Throw<RpcException>()
            .And.StatusCode.Should().Be(StatusCode.InvalidArgument);
    }

    [Fact]
    public void IntegrationTestGetListWhenEmptyFilters()
    {
        var response = _grpcClient.GetList(new GetListRequest());
        response.Page.Should().Be(1);
        response.TotalPages.Should().Be(1);
        response.Records.Select(x => x.Id).Should().Equal(_fixture.ProductList.Select(x => x.Id));
    }

    [Fact]
    public void IntegrationTestGetListWithPagingFilters()
    {
        const int page = 2;
        const int perPage = 1;

        var response = _grpcClient.GetList(new GetListRequest { Page = page, RecordsPerPage = perPage });
        response.Page.Should().Be(page);
        response.TotalPages.Should().Be(_fixture.ProductList.Count);
        response.Records.Count.Should().Be(perPage);
    }

    [Fact]
    public void IntegrationTestGetListWhenFilteredByCategory()
    {
        const ProductCategory category = ProductCategory.Food;

        var response = _grpcClient.GetList(
            new GetListRequest { Category = new NullableProductCategory { Value = category } });
        response.Records.Count.Should()
            .Be(_fixture.ProductList.Count(x => x.Category == Domain.Entities.ProductCategory.Food));
        response.Records.All(x => x.Category == category).Should().Be(true);
    }

    [Fact]
    public void IntegrationTestGetListWhenFilteredByWarehouse()
    {
        const int warehouseId = 2;

        var response = _grpcClient.GetList(
            new GetListRequest { WarehouseId = warehouseId });
        response.Records.Count.Should().Be(_fixture.ProductList.Count(x => x.WarehouseId == warehouseId));
        response.Records.All(x => x.WarehouseId == warehouseId).Should().Be(true);
    }

    [Fact]
    public void IntegrationTestGetListWhenFilteredByCreatedAt()
    {
        var testData = _fixture.ProductList;
        var dateFrom = testData[0].CreatedAt.AddHours(1);
        var dateTo = testData[^1].CreatedAt.AddHours(-1);

        var response = _grpcClient.GetList(
            new GetListRequest { DateFrom = dateFrom.ToTimestamp(), DateTo = dateTo.ToTimestamp() });
        response.Records.Count.Should()
            .Be(testData.Count(x => x.CreatedAt >= dateFrom && x.CreatedAt <= dateTo));
    }

    [Fact]
    public void IntegrationTestGetListWhenFilteredByFewFilters()
    {
        const int warehouseId = 2;
        var testData = _fixture.ProductList;
        var dateTo = testData[^1].CreatedAt.AddHours(-1);

        var response = _grpcClient.GetList(
            new GetListRequest { DateTo = dateTo.ToTimestamp(), WarehouseId = warehouseId });
        response.Records.Select(x => x.Id).Should()
            .Equal(testData.Where(x => x.CreatedAt <= dateTo && x.WarehouseId == warehouseId).Select(x => x.Id));
    }

    [Fact]
    public void IntegrationTestGetByIdSuccessful()
    {
        var product = _fixture.ProductList[0];

        var response = _grpcClient.GetById(new GetByIdRequest { Id = product.Id });
        response.Product.Should().BeEquivalentTo(product, options =>
            options.Excluding(x => x.Price).Excluding(x => x.CreatedAt));
        response.Product.Price.Should().Be((DecimalValue)product.Price);
        response.Product.CreatedAt.Should().Be(product.CreatedAt.ToTimestamp());
    }

    [Fact]
    public void IntegrationTestGetByIdWhenNotFounded()
    {
        var wrongId = _fixture.ProductList.Count + 10;

        var responseAct = () => _grpcClient.GetById(new GetByIdRequest { Id = wrongId });
        responseAct.Should().Throw<RpcException>()
            .And.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public void IntegrationTestUpdatePriceSuccessful()
    {
        const decimal newPrice = 1000.11m;
        var product = _fixture.ProductList[0];

        var response = _grpcClient.UpdatePrice(new UpdatePriceRequest { Id = product.Id, Price = newPrice });
        response.Product.Id.Should().Be(product.Id);
        response.Product.Price.Should().Be((DecimalValue)newPrice);
    }

    [Fact]
    public void IntegrationTestUpdatePriceWhenProductNotFounded()
    {
        var wrongId = _fixture.ProductList.Count + 10;

        var response = () =>
            _grpcClient.UpdatePrice(new UpdatePriceRequest { Id = wrongId, Price = 0 });
        response.Should().Throw<RpcException>().And.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public void IntegrationTestUpdatePriceWhenNotValidPrice()
    {
        var product = _fixture.ProductList[0];

        var response = () =>
            _grpcClient.UpdatePrice(new UpdatePriceRequest { Id = product.Id, Price = -10m });
        response.Should().Throw<RpcException>()
            .And.StatusCode.Should().Be(StatusCode.InvalidArgument);
    }
}