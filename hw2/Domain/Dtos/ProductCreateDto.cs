using Domain.Entities;

namespace Domain.Dtos;

public sealed record ProductCreateDto
(
    string Name,
    decimal Price,
    int Weight,
    ProductCategory Category,
    int WarehouseId
);