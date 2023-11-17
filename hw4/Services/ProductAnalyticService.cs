using homework_4.Entities;
using homework_4.Interfaces;

namespace homework_4.Services;

public sealed class ProductAnalyticService : IProductAnalyticService
{
    public ProductDemand CalculateDemand(ProductAnalytics analytics)
    {
        const int dataScientistsPredictionCoefficientComplexity = 100_000_000;
        var dataScientistsPredictionCoefficient = 1F;

        for (var i = 0; i < dataScientistsPredictionCoefficientComplexity; i++)
            dataScientistsPredictionCoefficient /= 3;

        var (productId, prediction, stock) = analytics;
        var demand = prediction > stock + (int)dataScientistsPredictionCoefficient ? prediction - stock : 0;
        return new ProductDemand(productId, demand);
    }
}