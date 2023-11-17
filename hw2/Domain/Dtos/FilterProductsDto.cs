using Domain.Entities;

namespace Domain.Dtos;

public sealed record FilterProductsDto
    (
        DateTime? DateFrom,
        DateTime? DateTo,
        ProductCategory? Category,
        int? WarehouseId,
        int Page,
        int RecordsPerPage
    )
    : PaginationDto(Page, RecordsPerPage);