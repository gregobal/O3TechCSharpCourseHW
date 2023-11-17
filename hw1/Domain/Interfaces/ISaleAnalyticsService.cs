namespace Domain.Interfaces;

public interface ISaleAnalyticsService
{
    decimal CalculateAds(int id);
    decimal CalculateSalesPrediction(int id, int days);
    decimal CalculateDemand(int id, int days, DateOnly? supplyDate);
}