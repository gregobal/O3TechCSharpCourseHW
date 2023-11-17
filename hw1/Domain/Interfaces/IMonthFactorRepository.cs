using Domain.Entities;

namespace Domain.Interfaces;

public interface IMonthFactorRepository
{
    IQueryable<MonthFactor> GetAllById(int id);
}