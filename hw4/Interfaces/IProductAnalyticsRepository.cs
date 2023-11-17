using System.Threading.Channels;
using homework_4.Entities;

namespace homework_4.Interfaces;

public interface IProductAnalyticsRepository
{
    Task ReadAsync(Channel<ProductAnalytics> output, CancellationToken cancelToken);
}