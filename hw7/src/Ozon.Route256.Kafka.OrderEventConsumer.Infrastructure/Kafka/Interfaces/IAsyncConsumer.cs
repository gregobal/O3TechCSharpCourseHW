using System.Threading;
using System.Threading.Tasks;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Kafka.Interfaces;

public interface IAsyncConsumer
{
    Task Consume(CancellationToken token);
}
