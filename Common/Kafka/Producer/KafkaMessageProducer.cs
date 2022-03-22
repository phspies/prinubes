using System.Text;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace Prinubes.Common.Kafka.Producer
{
    public class KafkaMessageProducer : IMessageProducer, IDisposable
    {
        private readonly Lazy<IProducer<string, string>> cachedProducer;
        public ProducerConfig producerConfig;

        public KafkaMessageProducer(IKafkaProducerBuilder kafkaProducerBuilder)
        {
            cachedProducer = new Lazy<IProducer<string, string>>(() => kafkaProducerBuilder.Build());
            producerConfig = kafkaProducerBuilder.ProducerConfig();
        }

        public void Dispose()
        {
            if (cachedProducer.IsValueCreated) cachedProducer.Value.Dispose();
        }
        public ProducerConfig ProducerConfig() => producerConfig;

        public async Task<string> ProduceAsync(string key, IMessage message, CancellationToken cancellationToken, string topic = null, int partition = 0)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            var serialisedMessage = JsonConvert.SerializeObject(message, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            var messageType = message.GetType().AssemblyQualifiedName;
            var producedMessage = new Message<string, string>
            {
                Key = key,
                Value = serialisedMessage,
                Headers = new Headers
                {
                    {"message-type", Encoding.UTF8.GetBytes(messageType)}
                }
            };
            DeliveryResult<string, string> result;
            if (topic != null)
            {
                result = await cachedProducer.Value.ProduceAsync(new TopicPartition(topic, new Partition(partition)), producedMessage, cancellationToken);
            }
            else
            {
                var attributeObject = Attribute.GetCustomAttributes(message.GetType()).OfType<MessageTopicAttribute>().Single();
                result = await cachedProducer.Value.ProduceAsync(new TopicPartition(attributeObject.Topic, new Partition(attributeObject.Partition)), producedMessage, cancellationToken);
            }
            return JsonConvert.SerializeObject(result);
        }
    }
}