using System.Net;
using System.Net.Http.Json;
using Domain.Dtos;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Test.Integration.Fixtures;

namespace Test.Integration;

public class ProductApiAspNetServiceIntegrationTest : IClassFixture<WebApplicationFactory<Program>>,
    IClassFixture<ProductsFixture>
{
    private const string ApiBaseUri = "/v2/Product";
    private readonly ProductsFixture _fixture;
    private readonly HttpClient _httpClient;

    public ProductApiAspNetServiceIntegrationTest(WebApplicationFactory<Program> factory,
        ProductsFixture fixture)
    {
        _fixture = fixture;

        _httpClient = factory.WithWebHostBuilder(builder => builder
                .ConfigureServices(services => services.Replace(
                    ServiceDescriptor.Singleton<IProductRepository>(_ =>
                        new ProductInMemoryRepository(_fixture.ProductList)))
                ))
            .CreateClient();
    }

    [Fact]
    public void IntegrationTestCreateSuccessful()
    {
        var requestBody = new
        {
            Name = "test", Price = 0, Weight = 1, WarehouseId = 1, Category = ProductCategory.General
        };

        var response = _httpClient.PostAsJsonAsync(ApiBaseUri, requestBody).Result;
        response.EnsureSuccessStatusCode();

        var product = response.Content.ReadFromJsonAsync<Product>().Result!;
        product.Id.Should().Be(_fixture.ProductList.Count + 1);
        product.CreatedAt.Date.Should().Be(DateTime.Now.Date);
        product.Should().BeEquivalentTo(product, options =>
            options.Excluding(x => x.Id).Excluding(x => x.CreatedAt));
    }

    [Fact]
    public void IntegrationTestCreateWhenNotValidPrice()
    {
        var requestBody = new
        {
            Name = "test", Price = -1, Weight = 1, WarehouseId = 1, Category = ProductCategory.General
        };

        var response = _httpClient.PostAsJsonAsync(ApiBaseUri, requestBody).Result;
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = response.Content.ReadFromJsonAsync<ProblemDetails>().Result!;
        problemDetails.Extensions.Should().ContainKey("errors");
    }

    [Fact]
    public void IntegrationTestCreateWhenNotValidWeight()
    {
        var requestBody = new
        {
            Name = "test", Price = 0, Weight = 0, WarehouseId = 1, Category = ProductCategory.General
        };

        var response = _httpClient.PostAsJsonAsync(ApiBaseUri, requestBody).Result;
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = response.Content.ReadFromJsonAsync<ProblemDetails>().Result!;
        problemDetails.Extensions.Should().ContainKey("errors");
    }

    [Fact]
    public void IntegrationTestGetListWhenEmptyFilters()
    {
        var response = _httpClient.GetFromJsonAsync<PaginatedListDto<Product>>(ApiBaseUri).Result!;

        response.Page.Should().Be(1);
        response.TotalPages.Should().Be(1);
        response.Records.Select(x => x.Id).Should().Equal(_fixture.ProductList.Select(x => x.Id));
    }

    [Fact]
    public void IntegrationTestGetListWithPagingFilters()
    {
        const int page = 2;
        const int perPage = 1;

        var response = _httpClient.GetFromJsonAsync<PaginatedListDto<Product>>(
            $"{ApiBaseUri}?Page={page}&RecordsPerPage={perPage}").Result!;

        response.Page.Should().Be(page);
        response.TotalPages.Should().Be(_fixture.ProductList.Count);
        response.Records.Count.Should().Be(perPage);
    }

    [Fact]
    public void IntegrationTestGetListWhenFilteredByCategory()
    {
        const ProductCategory category = ProductCategory.Food;

        var response = _httpClient.GetFromJsonAsync<PaginatedListDto<Product>>(
            $"{ApiBaseUri}?Category={(int)category}").Result!;

        response.Records.Count.Should()
            .Be(_fixture.ProductList.Count(x => x.Category == ProductCategory.Food));
        response.Records.All(x => x.Category == category).Should().Be(true);
    }

    [Fact]
    public void IntegrationTestGetListWhenFilteredByWarehouse()
    {
        const int warehouseId = 2;

        var response = _httpClient.GetFromJsonAsync<PaginatedListDto<Product>>(
            $"{ApiBaseUri}?WarehouseId={warehouseId}").Result!;

        response.Records.Count.Should().Be(_fixture.ProductList.Count(x => x.WarehouseId == warehouseId));
        response.Records.All(x => x.WarehouseId == warehouseId).Should().Be(true);
    }

    [Fact]
    public void IntegrationTestGetListWhenFilteredByCreatedAt()
    {
        var testData = _fixture.ProductList;
        var dateFrom = testData[0].CreatedAt.AddHours(1);
        var dateTo = testData[^1].CreatedAt.AddHours(-1);

        var response = _httpClient.GetFromJsonAsync<PaginatedListDto<Product>>(
            $"{ApiBaseUri}?DateFrom={dateFrom}&DateTo={dateTo}").Result!;

        response.Records.Count.Should()
            .Be(testData.Count(x => x.CreatedAt >= dateFrom && x.CreatedAt <= dateTo));
    }

    [Fact]
    public void IntegrationTestGetListWhenFilteredByFewFilters()
    {
        const int warehouseId = 2;
        var testData = _fixture.ProductList;
        var dateTo = testData[^1].CreatedAt.AddHours(-1);

        var response = _httpClient.GetFromJsonAsync<PaginatedListDto<Product>>(
            $"{ApiBaseUri}?DateTo={dateTo}&WarehouseId={warehouseId}").Result!;

        response.Records.Select(x => x.Id).Should()
            .Equal(testData.Where(x => x.CreatedAt <= dateTo && x.WarehouseId == warehouseId).Select(x => x.Id));
    }

    [Fact]
    public void IntegrationTestGetByIdSuccessful()
    {
        var expected = _fixture.ProductList[0];

        var actual = _httpClient.GetFromJsonAsync<Product>($"{ApiBaseUri}/{expected.Id}").Result!;
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void IntegrationTestGetByIdWhenNotFounded()
    {
        var wrongId = _fixture.ProductList.Count + 10;

        var response = _httpClient.GetAsync($"{ApiBaseUri}/{wrongId}").Result;
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problemDetails = response.Content.ReadFromJsonAsync<ProblemDetails>().Result!;
        problemDetails.Detail.Should().NotBeNull();
    }

    [Fact]
    public void IntegrationTestUpdatePriceSuccessful()
    {
        const decimal newPrice = 1000.11m;
        var product = _fixture.ProductList[0];

        var response = _httpClient.PatchAsJsonAsync(
            $"{ApiBaseUri}/{product.Id}", new { Price = newPrice }).Result;
        response.EnsureSuccessStatusCode();

        var updatedProduct = response.Content.ReadFromJsonAsync<Product>().Result!;
        updatedProduct.Id.Should().Be(product.Id);
        updatedProduct.Price.Should().Be(newPrice);
    }

    [Fact]
    public void IntegrationTestUpdatePriceWhenProductNotFounded()
    {
        var wrongId = _fixture.ProductList.Count + 10;

        var response = _httpClient.PatchAsJsonAsync(
            $"{ApiBaseUri}/{wrongId}", new { Price = 0 }).Result;
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problemDetails = response.Content.ReadFromJsonAsync<ProblemDetails>().Result!;
        problemDetails.Detail.Should().NotBeNull();
    }

    [Fact]
    public void IntegrationTestUpdatePriceWhenNotValidPrice()
    {
        var product = _fixture.ProductList[0];

        var response = _httpClient.PatchAsJsonAsync(
            $"{ApiBaseUri}/{product.Id}", new { Price = -10m }).Result;
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = response.Content.ReadFromJsonAsync<ProblemDetails>().Result!;
        problemDetails.Extensions.Should().ContainKey("errors");
    }
}