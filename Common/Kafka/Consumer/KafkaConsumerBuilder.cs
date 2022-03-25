using Confluent.Kafka;

namespace Prinubes.Common.Kafka.Consumer
{
    public class KafkaConsumerBuilder : IKafkaConsumerBuilder
    {
        private readonly ConsumerConfig consumerConfig;

        public KafkaConsumerBuilder(ConsumerConfig _consumerConfig)
        {
            consumerConfig = _consumerConfig ?? throw new ArgumentNullException(nameof(_consumerConfig));
        }

        public IConsumer<string, string> Build()
        {
            consumerConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
            var consumerBuilder = new ConsumerBuilder<string, string>(consumerConfig);
            return consumerBuilder.Build();
        }
    }
}