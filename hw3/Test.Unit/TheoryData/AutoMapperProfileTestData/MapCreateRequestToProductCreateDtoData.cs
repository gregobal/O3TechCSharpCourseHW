using Domain.Dtos;
using GrpcService;
using Test.Unit.TheoryData.AutoMapperProfileTestData.Base;
using ProductCategory = Domain.Entities.ProductCategory;

namespace Test.Unit.TheoryData.AutoMapperProfileTestData;

public class MapCreateRequestToProductCreateDtoData : MapFromToDataBase<CreateRequest, ProductCreateDto>
{
    protected override ProductCreateDto MapByHand(CreateRequest request)
    {
        return new ProductCreateDto(request.Name, request.Price, request.Weight,
            (ProductCategory)request.Category, request.WarehouseId);
    }
}