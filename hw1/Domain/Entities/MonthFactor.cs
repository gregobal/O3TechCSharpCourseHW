namespace Domain.Entities;

public record MonthFactor(
    int Id,
    int Month,
    decimal Coef
);