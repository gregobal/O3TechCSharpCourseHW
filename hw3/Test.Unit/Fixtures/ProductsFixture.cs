using Domain.Entities;

namespace Test.Unit.fixtures;

// ReSharper disable once ClassNeverInstantiated.Global
public class ProductsFixture
{
    public readonly List<Product> ProductList;

    public ProductsFixture()
    {
        const int productsCount = 10;
        
        var now = DateTime.Now;
        ProductList = new List<Product>();

        for (var i = 1; i <= productsCount; i++)
        {
            var category = i % 2 == 0 ? ProductCategory.Food : ProductCategory.General;
            var warehouseId = i % 3 == 0 ? 2 : 1;
            var createdAt = now.AddDays((i - 1) * -2);

            ProductList.Add(new Product(i, $"TestProduct{i}", i * 1.1m, i * 10, category, warehouseId, createdAt));
        }
    }
}