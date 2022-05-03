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
        private readonly ServiceSettings serviceSettings;
        private readonly IMediator mediator;
        private readonly IServiceProvider context;

        public KafkaTopicMessageConsumer(IServiceProvider _context, ILogger<KafkaTopicMessageConsumer> _logger, IKafkaConsumerBuilder _kafkaConsumerBuilder, ServiceSettings _serviceSettings, IMediator _mediator)
        {
            logger = _logger;
            kafkaConsumerBuilder = _kafkaConsumerBuilder;
            serviceSettings = _serviceSettings;
            mediator = _mediator;
            context = _context;
        }

        public void StartConsuming(string topic, CancellationToken cancellationToken)
        {
            using (var consumer = kafkaConsumerBuilder.Build())
            {
                KafkaHelpers.CreateTopic(topic, serviceSettings, logger);
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

                                mediator.Publish(messageNotification, cancellationToken).GetAwaiter().GetResult();
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