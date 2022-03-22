using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Prinubes.Common.Kafka.Consumer
{
    public class KafkaMessageConsumerManager : IKafkaMessageConsumerManager
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IServiceCollection services;

        public KafkaMessageConsumerManager(IServiceProvider _serviceProvider, IServiceCollection _services)
        {
            serviceProvider = _serviceProvider;
            services = _services;
        }
        public void StartConsumers(CancellationToken _cancellationToken)
        {
            var topicsWithNotificationHandlers = GetTopicsWithNotificationHandlers(services);
            foreach (var topic in topicsWithNotificationHandlers)
            {
                var kafkaTopicMessageConsumer = serviceProvider.GetRequiredService<IKafkaTopicMessageConsumer>();
                Thread _thread  = new Thread(() => kafkaTopicMessageConsumer.StartConsuming(topic, _cancellationToken));
                _thread.Name = $"Kafka thread for topic {topic}";
                _thread.Start();
            }
        }

        private static IEnumerable<string> GetTopicsWithNotificationHandlers(IServiceCollection services)
        {
            var messageTypesWithNotificationHandlers = services
                .Where(s => s.ServiceType.IsGenericType &&
                            s.ServiceType.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
                .Select(s => s.ServiceType.GetGenericArguments()[0])
                .Where(s => s.IsGenericType &&
                            s.GetGenericTypeDefinition() == typeof(MessageNotification<>))
                .Select(s => s.GetGenericArguments()[0])
                .Where(s => typeof(IMessage).IsAssignableFrom(s))
                .Distinct();

            return messageTypesWithNotificationHandlers
                .SelectMany(t => Attribute.GetCustomAttributes(t))
                .OfType<MessageTopicAttribute>()
                .Select(t => t.Topic)
                .Distinct()
                .ToList();
        }
    }
}