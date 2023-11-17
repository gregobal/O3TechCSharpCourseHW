namespace Domain.Dtos;

public record PaginationDto
(
    int Page,
    int RecordsPerPage
);