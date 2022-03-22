using AutoMapper;
using MediatR;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Kafka;
using Prinubes.Common.DatabaseModels.PlatformEnums;
using Prinubes.Common.Kafka.Producer;
using Prinubes.Platforms.Datamodels;
using Prinubes.PlatformWorkers.Helpers;

namespace Prinubes.PlatformWorker.Kafka
{
    public class NetworkPlatformTestingRequestKafkaHandler : INotificationHandler<MessageNotification<NetworkPlatformTestingRequestKafkaMessage>>
    {
        private readonly ILogger logger;
        private IMapper mapper; 
        private IMessageProducer kafkaProducer;
        private PrinubesPlatformWorkerDBContext DBContext;

        public NetworkPlatformTestingRequestKafkaHandler(IServiceProvider _serviceProvider)
        {
            logger = _serviceProvider.GetRequiredService<ILogger<NetworkPlatformTestingRequestKafkaMessage>>();
            mapper = _serviceProvider.GetRequiredService<IMapper>();
            kafkaProducer = _serviceProvider.GetRequiredService<IMessageProducer>();
            DBContext = _serviceProvider.GetRequiredService<PrinubesPlatformWorkerDBContext>();

        }

        public async Task Handle(MessageNotification<NetworkPlatformTestingRequestKafkaMessage> notification, CancellationToken cancellationToken)
        {
            NetworkPlatformTestingRequestKafkaMessage networkPlatformKafkaMessage = notification.Message;
            if (networkPlatformKafkaMessage != null)
            {
                logger.LogInformation($"NetworkPlatform message received with key: {networkPlatformKafkaMessage.NetworkPlatform.Platform} and action: {networkPlatformKafkaMessage.Action}");
                switch (networkPlatformKafkaMessage.Action)
                {
                    case ActionEnum.test:
                        await RecordExistanceConfirmation.CredentialExistsAsync(networkPlatformKafkaMessage.NetworkPlatform.CredentialID, logger, DBContext);
                        switch (networkPlatformKafkaMessage.NetworkPlatform.PlatformType)
                        {
                            case NetworkPlatformType.NSXT:
                                KafkaMessage.SubmitKafkaMessageAync(
                                new NetworkPlatformTestingResponseModel()
                                {
                                     Message = "Sucess!!",
                                     Success = true,
                                }, logger, kafkaProducer, topic: networkPlatformKafkaMessage.ReturnTopic);
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
