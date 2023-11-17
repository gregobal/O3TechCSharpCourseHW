namespace Domain.Entities;

public record Product(
    int Id,
    string Name,
    decimal Price,
    int Weight,
    ProductCategory Category,
    int WarehouseId,
    DateTime CreatedAt
);