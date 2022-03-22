using AutoMapper;
using MediatR;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Kafka;
using Prinubes.Common.DatabaseModels.PlatformEnums;
using Prinubes.Common.Kafka.Producer;
using Prinubes.PlatformWorkers.Helpers;
using Prinubes.Platforms.Datamodels;

namespace Prinubes.PlatformWorker.Kafka
{
    public class ComputePlatformTestingRequestKafkaHandler : INotificationHandler<MessageNotification<ComputePlatformTestingRequestKafkaMessage>>
    {
        private ILogger logger;
        private IMapper mapper; 
        private IMessageProducer kafkaProducer;
        private PrinubesPlatformWorkerDBContext DBContext;

        public ComputePlatformTestingRequestKafkaHandler(IServiceProvider _serviceProvider)
        {
            logger = _serviceProvider.GetRequiredService<ILogger<ComputePlatformTestingRequestKafkaHandler>>();
            mapper = _serviceProvider.GetRequiredService<IMapper>();
            kafkaProducer = _serviceProvider.GetRequiredService<IMessageProducer>();
            DBContext = _serviceProvider.GetRequiredService<PrinubesPlatformWorkerDBContext>();

        }

        public async Task Handle(MessageNotification<ComputePlatformTestingRequestKafkaMessage> notification, CancellationToken cancellationToken)
        {
            ComputePlatformTestingRequestKafkaMessage computePlatformKafkaMessage = notification.Message;
            if (computePlatformKafkaMessage != null)
            {
                logger.LogInformation($"ComputePlatform message received with key: {computePlatformKafkaMessage.ComputePlatform.Platform} and action: {computePlatformKafkaMessage.Action}");
                switch (computePlatformKafkaMessage.Action)
                {
                    case ActionEnum.test:
                        await RecordExistanceConfirmation.CredentialExistsAsync(computePlatformKafkaMessage.ComputePlatform.CredentialID, logger, DBContext);
                        switch (computePlatformKafkaMessage.ComputePlatform.PlatformType)
                        {
                            case ComputePlatformType.vSphere:
                                KafkaMessage.SubmitKafkaMessageAync(
                                new ComputePlatformTestingResponseModel()
                                {
                                     Message = "Sucess!!",
                                     Success = true,
                                }, logger, kafkaProducer, topic: computePlatformKafkaMessage.ReturnTopic);
                                logger.LogInformation($"ComputePlatform test message sent to topic: {computePlatformKafkaMessage.ReturnTopic}");

                                break;
                            default:
                                logger.LogInformation($"ComputePlatform type not known: {computePlatformKafkaMessage.ComputePlatform.PlatformType}");

                                break;
                        }
                        break;

                    default:
                        logger.LogError($"ComputePlatform message action not implemented {computePlatformKafkaMessage.Action}");
                        break;
                }
            }
            else
            {
                logger.LogError($"ComputePlatform message received null value");
            }
            return;
        }
    }
}
