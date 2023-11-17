using Domain.Dtos;
using Domain.Entities;
using Google.Protobuf.WellKnownTypes;
using GrpcService;
using Test.Unit.TheoryData.AutoMapperProfileTestData.Base;
using ProductCategory = GrpcService.ProductCategory;

namespace Test.Unit.TheoryData.AutoMapperProfileTestData;

public class MapPaginatedListDtoToGetListResponseData : MapFromToDataBase<PaginatedListDto<Product>, GetListResponse>
{
    protected override GetListResponse MapByHand(PaginatedListDto<Product> dto)
    {
        return new GetListResponse
        {
            Page = dto.Page, TotalPages = dto.TotalPages,
            Records =
            {
                dto.Records.Select(p => new ProductModel
                {
                    Id = p.Id, Name = p.Name, Price = p.Price, Weight = p.Weight, WarehouseId = p.WarehouseId,
                    Category = (ProductCategory)p.Category, CreatedAt = p.CreatedAt.ToTimestamp()
                })
            }
        };
    }
}