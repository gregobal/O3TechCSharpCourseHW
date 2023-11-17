namespace Domain.Dtos;

public sealed record PaginatedListDto<T>
(
    int Page,
    int TotalPages,
    IReadOnlyCollection<T> Records
);