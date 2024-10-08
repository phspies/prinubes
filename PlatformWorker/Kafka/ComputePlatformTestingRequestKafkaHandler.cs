﻿using MediatR;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.DatabaseModels.PlatformEnums;
using Prinubes.Common.Helpers;
using Prinubes.Common.Kafka;
using Prinubes.Common.Kafka.Producer;
using Prinubes.PlatformWorker.CloudLibraries.vSphere;
using Prinubes.PlatformWorker.Datamodels;
using Prinubes.PlatformWorker.Helpers;

namespace Prinubes.PlatformWorker.Kafka
{
    public class ComputePlatformTestingRequestKafkaHandler : INotificationHandler<MessageNotification<ComputePlatformTestingRequestKafkaMessage>>
    {
        private ILogger logger;
        private IMessageProducer kafkaProducer;
        private PrinubesPlatformWorkerDBContext DBContext;

        public ComputePlatformTestingRequestKafkaHandler(IServiceProvider _serviceProvider)
        {
            logger = ServiceActivator.GetRequiredService<ILogger<ComputePlatformKafkaHandler>>(_serviceProvider);
            DBContext = ServiceActivator.GetRequiredService<PrinubesPlatformWorkerDBContext>(_serviceProvider);
            kafkaProducer = ServiceActivator.GetRequiredService<IMessageProducer>(_serviceProvider);
        }

        public async Task Handle(MessageNotification<ComputePlatformTestingRequestKafkaMessage> notification, CancellationToken cancellationToken)
        {
            ComputePlatformTestingRequestKafkaMessage computePlatformKafkaMessage = notification.Message;
            if (computePlatformKafkaMessage != null)
            {
                logger.LogInformation($"ComputePlatform testing message received with key: {computePlatformKafkaMessage.ComputePlatform.Platform} and action: {computePlatformKafkaMessage.Action}");
                switch (computePlatformKafkaMessage.Action)
                {
                    case ActionEnum.test:
                        await RecordExistanceConfirmation.CredentialExistsAsync(computePlatformKafkaMessage.ComputePlatform.CredentialID, logger, DBContext);
                        switch (computePlatformKafkaMessage.ComputePlatform.PlatformType)
                        {
                            case ComputePlatformType.vSphere:
                                vSphereFactory vsphereFactory = new vSphereFactory(computePlatformKafkaMessage.ComputePlatform, DBContext, logger);
                                KafkaMessage.SubmitKafkaMessageAync(await vsphereFactory.TestCredentials(), logger, kafkaProducer, topic: computePlatformKafkaMessage.ReturnTopic);
                                logger.LogInformation($"ComputePlatform test message sent to topic: {computePlatformKafkaMessage.ReturnTopic}");

                                break;
                            default:
                                logger.LogInformation($"ComputePlatform type not known: {computePlatformKafkaMessage.ComputePlatform.PlatformType}");
                                KafkaMessage.SubmitKafkaMessageAync(
                                    new ComputePlatformTestingResponseModel()
                                    {
                                        Message = $"Could platform not known: {computePlatformKafkaMessage.ComputePlatform.PlatformType}",
                                        Success = false,
                                    }, logger, kafkaProducer, topic: computePlatformKafkaMessage.ReturnTopic);
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
