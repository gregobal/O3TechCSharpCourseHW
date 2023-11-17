using Domain.Exceptions;
using Domain.Interfaces;

namespace Domain.Services;

public class SaleAnalyticsService : ISaleAnalyticsService
{
    private readonly ISaleHistoryRepository _saleHistoryRepository;
    private readonly IMonthFactorRepository _monthFactorRepository;

    public SaleAnalyticsService(ISaleHistoryRepository saleHistoryRepository,
        IMonthFactorRepository monthFactorRepository)
    {
        _saleHistoryRepository = saleHistoryRepository;
        _monthFactorRepository = monthFactorRepository;
    }

    public decimal CalculateAds(int id)
    {
        var inStockByIdList = _saleHistoryRepository.GetAllByIdAndInStock(id).ToList();

        var count = inStockByIdList.Count;
        if (count < 1)
        {
            throw new DomainException($"Sale history for product Id = {id} not contain records with in stock");
        }

        var sum = inStockByIdList.Sum(x => x.Sales);
        var result = sum * 1m / count;

        return result;
    }

    public decimal CalculateSalesPrediction(int id, int days)
    {
        if (days < 1)
        {
            throw new DomainException($"Parameter <{nameof(days)}> for calculate prediction must be greater than 0");
        }

        var result = CalculatePredictionsByDay(id, days)
            .Sum();

        return result;
    }

    public decimal CalculateDemand(int id, int days, DateOnly? supplyDate = null)
    {
        if (days < 1)
        {
            throw new DomainException($"Parameter <{nameof(days)}> for calculate demand must be greater than 0");
        }

        var lastHistoryValue = _saleHistoryRepository.GetAllByIdAndInStock(id)
            .OrderBy(x => x.Date).LastOrDefault();
        if (lastHistoryValue is null)
        {
            throw new DomainException($"Sale history for product Id = {id} not contain records with in stock");
        }

        var stock = lastHistoryValue.Stock;

        var predictionByDays = CalculatePredictionsByDay(id, days).ToList();
        var supplyDays = 0;

        if (supplyDate.HasValue)
        {
            var dateNow = DateOnly.FromDateTime(DateTime.Now);
            if (supplyDate <= dateNow)
            {
                throw new DomainException(
                    $"Parameter <{nameof(supplyDate)}> for calculate demand must be a future date");
            }

            supplyDays = supplyDate.Value.DayNumber - dateNow.DayNumber;
        }

        var predictionBeforeSupply = predictionByDays.Take(supplyDays).Sum();
        var predictionAfterSupply = predictionByDays.Skip(supplyDays).Sum();
        var surplus = stock - predictionBeforeSupply;
        var result = surplus > 0 ? predictionAfterSupply - surplus : predictionAfterSupply;

        return result < 0 ? 0 : result;
    }

    private IEnumerable<decimal> CalculatePredictionsByDay(int id, int days)
    {
        var ads = CalculateAds(id);
        var monthFactorsByIdList = _monthFactorRepository.GetAllById(id).ToList();
        var now = DateTime.Now;

        var result = Enumerable.Range(0, days)
            .Select(x =>
                ads * (monthFactorsByIdList.Find(mf => mf.Month == now.AddDays(x).Month)?.Coef ?? 1)
            );

        return result;
    }
}