using System.Threading.Channels;
using homework_4.Entities;
using homework_4.Interfaces;

namespace homework_4.Services;

public sealed class CalculateService : ICalculateService, IProgressCounter
{
    private readonly IProductAnalyticService _productAnalytic;
    private int _progressCount;

    public CalculateService(IProductAnalyticService productAnalytic)
    {
        _productAnalytic = productAnalytic;
    }

    public async Task CalculateDemandAsync(Channel<ProductAnalytics> input, Channel<ProductDemand> output,
        CancellationToken cancelToken)
    {
        if (cancelToken.IsCancellationRequested) return;

        await foreach (var analytics in input.Reader.ReadAllAsync(cancelToken))
        {
            var demand = _productAnalytic.CalculateDemand(analytics);
            Interlocked.Increment(ref _progressCount);

            await output.Writer.WriteAsync(demand, cancelToken);

            if (cancelToken.IsCancellationRequested) return;
        }
    }

    public int ProgressCount => _progressCount;
}