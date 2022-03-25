using Confluent.Kafka;

namespace Prinubes.Common.Kafka.Producer
{
    public class KafkaProducerBuilder : IKafkaProducerBuilder
    {
        private readonly ProducerConfig producerConfig;

        public KafkaProducerBuilder(ProducerConfig _producerConfig)
        {
            producerConfig = _producerConfig ?? throw new ArgumentNullException(nameof(_producerConfig));
        }

        public ProducerConfig ProducerConfig() => producerConfig;

        public IProducer<string, string> Build()
        {
            var producerBuilder = new ProducerBuilder<string, string>(producerConfig);
            return producerBuilder.Build();
        }
    }
}