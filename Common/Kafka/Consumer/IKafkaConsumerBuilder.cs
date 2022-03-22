using Confluent.Kafka;

namespace Prinubes.Common.Kafka.Consumer
{
    public interface IKafkaConsumerBuilder
    {
        IConsumer<string, string> Build();
    }
}