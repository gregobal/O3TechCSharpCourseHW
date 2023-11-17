using homework_4.Entities;

namespace homework_4.Interfaces;

public interface IProductAnalyticService
{
    ProductDemand CalculateDemand(ProductAnalytics analytics);
}