using System.Threading.Channels;
using homework_4.Entities;

namespace homework_4.Interfaces;

public interface ICalculateService
{
    Task CalculateDemandAsync(Channel<ProductAnalytics> input, Channel<ProductDemand> output,
        CancellationToken cancelToken);
}