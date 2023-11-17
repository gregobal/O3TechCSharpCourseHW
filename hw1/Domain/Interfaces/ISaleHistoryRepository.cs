using Domain.Entities;

namespace Domain.Interfaces;

public interface ISaleHistoryRepository
{
    IQueryable<SaleHistory> GetAllByIdAndInStock(int id);
}