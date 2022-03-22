using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Prinubes.Common.Kafka.Producer
{
    public interface IMessageProducer
    {
        Task<string> ProduceAsync(string key, IMessage message, CancellationToken cancellationToken, string topic, int partition);

        public ProducerConfig ProducerConfig();
    }
}