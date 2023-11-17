using Domain.Entities;
using Google.Protobuf.WellKnownTypes;
using GrpcService;
using Test.Unit.TheoryData.AutoMapperProfileTestData.Base;
using ProductCategory = GrpcService.ProductCategory;

namespace Test.Unit.TheoryData.AutoMapperProfileTestData;

public class MapProductToProductModelData : MapFromToDataBase<Product, ProductModel>
{
    protected override ProductModel MapByHand(Product product)
    {
        return new ProductModel
        {
            Id = product.Id, Name = product.Name, Price = product.Price,
            Weight = product.Weight, WarehouseId = product.WarehouseId,
            Category = (ProductCategory)product.Category,
            CreatedAt = product.CreatedAt.ToTimestamp()
        };
    }
}