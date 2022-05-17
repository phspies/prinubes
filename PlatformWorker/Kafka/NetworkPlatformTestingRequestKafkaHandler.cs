using AutoMapper;
using MediatR;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.DatabaseModels.PlatformEnums;
using Prinubes.Common.Kafka;
using Prinubes.Common.Kafka.Producer;
using Prinubes.PlatformWorker.CloudLibraries.NSXT;
using Prinubes.PlatformWorker.Datamodels;
using Prinubes.PlatformWorker.Helpers;

namespace Prinubes.PlatformWorker.Kafka
{
    public class NetworkPlatformTestingRequestKafkaHandler : INotificationHandler<MessageNotification<NetworkPlatformTestingRequestKafkaMessage>>
    {
        private readonly ILogger logger;
        private IMessageProducer kafkaProducer;
        private PrinubesPlatformWorkerDBContext DBContext;

        public NetworkPlatformTestingRequestKafkaHandler(IServiceProvider _serviceProvider)
        {
            var scope = _serviceProvider.CreateScope();
            logger = scope.ServiceProvider.GetRequiredService<ILogger<NetworkPlatformTestingRequestKafkaHandler>>();
            DBContext = scope.ServiceProvider.GetRequiredService<PrinubesPlatformWorkerDBContext>();
            kafkaProducer = _serviceProvider.GetRequiredService<IMessageProducer>();
        }

        public async Task Handle(MessageNotification<NetworkPlatformTestingRequestKafkaMessage> notification, CancellationToken cancellationToken)
        {
            NetworkPlatformTestingRequestKafkaMessage networkPlatformKafkaMessage = notification.Message;
            if (networkPlatformKafkaMessage != null)
            {
                logger.LogInformation($"NetworkPlatform testing message received with key: {networkPlatformKafkaMessage.NetworkPlatform.Platform} and action: {networkPlatformKafkaMessage.Action}");
                switch (networkPlatformKafkaMessage.Action)
                {
                    case ActionEnum.test:
                        await RecordExistanceConfirmation.CredentialExistsAsync(networkPlatformKafkaMessage.NetworkPlatform.CredentialID, logger, DBContext);
                        switch (networkPlatformKafkaMessage.NetworkPlatform.PlatformType)
                        {
                            case NetworkPlatformType.NSXT:
                                NSXTFactory factory = new NSXTFactory(networkPlatformKafkaMessage.NetworkPlatform, DBContext);
                                KafkaMessage.SubmitKafkaMessageAync(await factory.TestCredentials(), logger, kafkaProducer, topic: networkPlatformKafkaMessage.ReturnTopic);
                                logger.LogInformation($"NetworkPlatform test message sent to topic: {networkPlatformKafkaMessage.ReturnTopic}");

                                break;
                            default:
                                logger.LogInformation($"NetworkPlatform type not known: {networkPlatformKafkaMessage.NetworkPlatform.PlatformType}");

                                break;
                        }
                        break;

                    default:
                        logger.LogError($"NetworkPlatform message action not implemented {networkPlatformKafkaMessage.Action}");
                        break;
                }
            }
            else
            {
                logger.LogError($"NetworkPlatform message received null value");
            }
            return;
        }
    }
}
