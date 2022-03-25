using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Prinubes.Common.Models;
using System.Text;

namespace Prinubes.Common.Kafka.Consumer
{
    public class KafkaTopicMessageConsumer : IKafkaTopicMessageConsumer
    {
        private readonly IKafkaConsumerBuilder kafkaConsumerBuilder;
        private readonly ILogger<KafkaTopicMessageConsumer> logger;
        private readonly IServiceProvider serviceProvider;

        public KafkaTopicMessageConsumer(ILogger<KafkaTopicMessageConsumer> _logger, IKafkaConsumerBuilder _kafkaConsumerBuilder, IServiceProvider _serviceProvider)
        {
            logger = _logger;
            kafkaConsumerBuilder = _kafkaConsumerBuilder;
            serviceProvider = _serviceProvider;
        }

        public void StartConsuming(string topic, CancellationToken cancellationToken)
        {
            using (var consumer = kafkaConsumerBuilder.Build())
            {
                KafkaHelpers.CreateTopic(topic, serviceProvider.GetRequiredService<ServiceSettings>(), logger);
                logger.LogInformation($"Starting consumer for {topic}");
                consumer.Subscribe(topic);

                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume(TimeSpan.FromSeconds(5));
                            if (consumeResult != null)
                            {
                                // TODO: log error if missing header
                                var messageTypeEncoded = consumeResult.Message.Headers.GetLastBytes("message-type");
                                var messageTypeHeader = Encoding.UTF8.GetString(messageTypeEncoded);
                                var messageType = Type.GetType(messageTypeHeader);

                                ArgumentNullException.ThrowIfNull(messageType);

                                var message = JsonConvert.DeserializeObject(consumeResult.Message.Value, messageType);
                                var messageNotificationType = typeof(MessageNotification<>).MakeGenericType(messageType);
                                var messageNotification = Activator.CreateInstance(messageNotificationType, message);

                                ArgumentNullException.ThrowIfNull(messageNotification);

                                using (var scope = serviceProvider.CreateScope())
                                {
                                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                                    mediator.Publish(messageNotification, cancellationToken).GetAwaiter().GetResult();
                                }
                                logger.LogInformation($"Commit message consumer for {topic}");
                                consumer.Commit(consumeResult);
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
            }
        }
    }
}