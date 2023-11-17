using Domain.Entities;

namespace Test.Integration.Fixtures;

// ReSharper disable once ClassNeverInstantiated.Global
public class ProductsFixture
{
    public readonly List<Product> ProductList = new()
    {
        new Product(1, "Apple", 2.5m, 200, ProductCategory.Food, 1, new DateTime(2023, 10, 1).ToUniversalTime()),
        new Product(2, "Bleach", 17.33m, 1000, ProductCategory.HouseholdChemicals, 2,
            new DateTime(2023, 10, 1).ToUniversalTime()),
        new Product(3, "Coffee Maker", 120m, 3500, ProductCategory.Appliances, 2,
            new DateTime(2023, 10, 3).ToUniversalTime())
    };
}