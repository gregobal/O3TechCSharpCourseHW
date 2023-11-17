using Api.Utils;
using AutoMapper;
using Domain.Dtos;
using Domain.Entities;
using FluentAssertions;
using GrpcService;
using Test.Unit.TheoryData.AutoMapperProfileTestData;

namespace Test.Unit;

public class AutoMapperProfileTest
{
    private readonly IMapper _mapper;

    public AutoMapperProfileTest()
    {
        var configuration = new MapperConfiguration(configure =>
            configure.AddProfile<AutoMapperProfile>());
        _mapper = new Mapper(configuration);
    }

    [Theory]
    [ClassData(typeof(MapProductModelToProductData))]
    public void MapProductModelToProductTest(ProductModel forMapping, Product expected)
    {
        MapTestBase(forMapping, expected);
    }

    [Theory]
    [ClassData(typeof(MapProductToProductModelData))]
    public void MapProductToProductModelTest(Product forMapping, ProductModel expected)
    {
        MapTestBase(forMapping, expected);
    }

    [Theory]
    [ClassData(typeof(MapCreateRequestToProductCreateDtoData))]
    public void MapCreateRequestToProductCreateDtoTest(CreateRequest forMapping, ProductCreateDto expected)
    {
        MapTestBase(forMapping, expected);
    }

    [Theory]
    [ClassData(typeof(MapUpdatePriceRequestToUpdatePriceDtoData))]
    public void MapUpdatePriceRequestToUpdatePriceDtoTest(UpdatePriceRequest forMapping, UpdatePriceDto expected)
    {
        MapTestBase(forMapping, expected);
    }

    [Theory]
    [ClassData(typeof(MapGetListRequestToFilterProductsDtoData))]
    public void MapGetListRequestToFilterProductsDtoTest(GetListRequest forMapping, FilterProductsDto expected)
    {
        MapTestBase(forMapping, expected);
    }

    [Theory]
    [ClassData(typeof(MapPaginatedListDtoToGetListResponseData))]
    public void MapPaginatedListDtoToGetListResponseTest(PaginatedListDto<Product> forMapping, GetListResponse expected)
    {
        MapTestBase(forMapping, expected);
    }

    private void MapTestBase<T, TR>(T forMapping, TR expected) where T : class where TR : class
    {
        var actual = _mapper.Map<T, TR>(forMapping);

        actual.Should().BeOfType<TR>();
        actual.Should().Be(expected);
    }
}