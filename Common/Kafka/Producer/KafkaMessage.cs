using Microsoft.Extensions.Logging;

namespace Prinubes.Common.Kafka.Producer
{
    public class KafkaMessage
    {
        static public async void SubmitKafkaMessageAync<T>(T messageObject, ILogger logger, IMessageProducer kafkaProducer, CancellationToken cancellationToken = default(CancellationToken), string topic = null, int partition = 0)
        {
            int count = 0;
            string kafkaResult = string.Empty;
            while (true)
            {
                count++;
                try
                {
                    kafkaResult = await kafkaProducer.ProduceAsync(null, messageObject as IMessage, cancellationToken, topic, partition);
                    logger.LogInformation($"Kafka submit running ran: {kafkaResult}");
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError($"Kafka running failed: {DateTimeOffset.Now} - {ex.Message} - {kafkaResult}");
                    await Task.Delay((kafkaProducer.ProducerConfig().RetryBackoffMs ?? 10) * 1000);
                }
                if (count > kafkaProducer.ProducerConfig().MessageSendMaxRetries)
                {
                    throw new KafkaMessageException($"Kafka message failed after {kafkaProducer.ProducerConfig().MessageSendMaxRetries} tries : {kafkaResult}");
                }
            }
        }
    }
}
