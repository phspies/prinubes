using System.Threading;

namespace Prinubes.Common.Kafka.Consumer
{
    public interface IKafkaMessageConsumerManager
    {
        void StartConsumers(CancellationToken cancellationToken);
    }
}