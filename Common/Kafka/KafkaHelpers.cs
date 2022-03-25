using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Prinubes.Common.Kafka.Consumer;
using Prinubes.Common.Models;
using System.Text;

namespace Prinubes.Common.Kafka
{
    public class KafkaHelpers
    {
        public static void CreateTopic(string topic, ServiceSettings serviceSettings, ILogger logger)
        {
            using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = serviceSettings.KAFKA_BOOTSTRAP }).Build())
            {
                if (adminClient.GetMetadata(topic, TimeSpan.FromSeconds(5000)) == null)
                {
                    logger.LogDebug($"Creating non-existent topic {topic}");
                    adminClient.CreateTopicsAsync(new TopicSpecification[] { new TopicSpecification() { Name = topic, NumPartitions = 1, ReplicationFactor = 1 } }).Wait();
                }
            }
        }
        public static T ConsumeTopicAdhoc<T>(string topic, IKafkaConsumerBuilder kafkaConsumerBuilder, ILogger logger) where T : new()
        {

            T response = new T();
            using (var consumer = kafkaConsumerBuilder.Build())
            {
                logger.LogInformation($"Starting consumer for {topic}");
                consumer.Subscribe(topic);
                try
                {
                    while (true)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume(TimeSpan.FromSeconds(5));
                            if (consumeResult != null)
                            {
                                var messageTypeEncoded = consumeResult.Message.Headers.GetLastBytes("message-type");
                                ArgumentNullException.ThrowIfNull(messageTypeEncoded);
                                var messageTypeHeader = Encoding.UTF8.GetString(messageTypeEncoded);

                                response = JsonConvert.DeserializeObject<T>(consumeResult.Message.Value) ?? new T();

                                ArgumentNullException.ThrowIfNull(response);

                                logger.LogInformation($"Commit message consumer for {topic}");
                                consumer.Commit(consumeResult);
                                break;

                            }
                            else
                            {
                                logger.LogInformation($"Topic {topic} didn't respond within 5 seconds");
                            }
                        }
                        catch (Exception ex)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // do nothing on cancellation
                }
                finally
                {
                    consumer.Close();
                }
                return response;
            }
        }
        public static void DeleteTopic(string topic, ServiceSettings serviceSettings)
        {
            using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = serviceSettings.KAFKA_BOOTSTRAP }).Build())
            {
                adminClient.DeleteTopicsAsync(new string[] { topic }).Wait();
            }
        }

    }
}
