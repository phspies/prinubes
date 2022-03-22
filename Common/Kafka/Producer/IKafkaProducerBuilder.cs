using Confluent.Kafka;

namespace Prinubes.Common.Kafka.Producer
{
    public interface IKafkaProducerBuilder
    {
        IProducer<string, string> Build();

        public ProducerConfig ProducerConfig();

    }
}