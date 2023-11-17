using Domain.Dtos;
using GrpcService;
using Test.Unit.TheoryData.AutoMapperProfileTestData.Base;
using ProductCategory = Domain.Entities.ProductCategory;

namespace Test.Unit.TheoryData.AutoMapperProfileTestData;

public class MapGetListRequestToFilterProductsDtoData : MapFromToDataBase<GetListRequest, FilterProductsDto>
{
    protected override FilterProductsDto MapByHand(GetListRequest request)
    {
        return new FilterProductsDto(
            request.DateFrom?.ToDateTime(), request.DateTo?.ToDateTime(),
            (ProductCategory?)request.Category?.Value,
            request.WarehouseId, request.Page, request.RecordsPerPage);
    }
}