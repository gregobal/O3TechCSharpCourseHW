namespace Domain.Entities;

public record SaleHistory(
    int Id,
    DateOnly Date,
    int Sales,
    int Stock
);