using System.Threading.Channels;
using homework_4.Entities;

namespace homework_4.Interfaces;

public interface IProductDemandRepository
{
    Task WriteAsync(Channel<ProductDemand> input, CancellationToken cancelToken);
}