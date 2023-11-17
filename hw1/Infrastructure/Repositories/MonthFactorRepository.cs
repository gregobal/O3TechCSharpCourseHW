using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Repositories.Base;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Repositories;

public sealed class MonthFactorRepository : BaseCsvFileRepository<MonthFactor>, IMonthFactorRepository
{
    public MonthFactorRepository(IConfiguration appConfig) : base(appConfig, nameof(MonthFactor))
    {
    }

    public IQueryable<MonthFactor> GetAllById(int id) => GetAll().Where(x => x.Id == id);
}