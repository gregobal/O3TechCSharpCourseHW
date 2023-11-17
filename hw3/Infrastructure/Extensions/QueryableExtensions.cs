using Domain.Dtos;

namespace Infrastructure.Extensions;

internal static class QueryableExtensions
{
    public static PaginatedListDto<T> Paginate<T>(this IQueryable<T> queryable, PaginationDto pagination,
        int recordsPerPageDefault)
    {
        var totalPages = 1;
        var page = pagination.Page <= 0 ? 1 : pagination.Page;
        var perPage = pagination.RecordsPerPage <= 0 ? recordsPerPageDefault : pagination.RecordsPerPage;

        var count = queryable.Count();

        if (count == 0)
        {
            page = 1;
        }
        else
        {
            totalPages = (int)Math.Ceiling(count * 1d / perPage);

            if (page > totalPages) page = totalPages;
        }

        var records = queryable.Skip((page - 1) * perPage)
            .Take(perPage)
            .ToList();

        return new PaginatedListDto<T>(page, totalPages, records);
    }
}