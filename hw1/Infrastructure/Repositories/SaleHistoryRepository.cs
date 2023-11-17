using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Repositories.Base;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Repositories;

public sealed class SaleHistoryRepository : BaseCsvFileRepository<SaleHistory>, ISaleHistoryRepository
{
    public SaleHistoryRepository(IConfiguration appConfig) : base(appConfig, nameof(SaleHistory))
    {
    }

    public IQueryable<SaleHistory> GetAllByIdAndInStock(int id) => GetAll()
        .Where(x => x.Id == id && x.Stock > 0);
}