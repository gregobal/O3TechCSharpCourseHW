using Domain.Entities;
using GrpcService;
using Test.Unit.TheoryData.AutoMapperProfileTestData.Base;
using ProductCategory = Domain.Entities.ProductCategory;

namespace Test.Unit.TheoryData.AutoMapperProfileTestData;

public class MapProductModelToProductData : MapFromToDataBase<ProductModel, Product>
{
    protected override Product MapByHand(ProductModel model)
    {
        return new Product(
            model.Id, model.Name, model.Price, model.Weight, (ProductCategory)model.Category,
            model.WarehouseId, model.CreatedAt.ToDateTime()
        );
    }
}