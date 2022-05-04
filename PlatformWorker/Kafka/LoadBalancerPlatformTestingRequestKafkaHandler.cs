using AutoMapper;
using MediatR;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.DatabaseModels.PlatformEnums;
using Prinubes.Common.Kafka;
using Prinubes.Common.Kafka.Producer;
using Prinubes.PlatformWorker.Datamodels;
using Prinubes.PlatformWorker.Helpers;

namespace Prinubes.PlatformWorker.Kafka
{
    public class LoadBalancerPlatformTestingRequestKafkaHandler : INotificationHandler<MessageNotification<LoadBalancerPlatformTestingRequestKafkaMessage>>
    {
        private readonly ILogger logger;
        private IMessageProducer kafkaProducer;
        private PrinubesPlatformWorkerDBContext DBContext;

        public LoadBalancerPlatformTestingRequestKafkaHandler(IServiceProvider _serviceProvider)
        {
            var scope = _serviceProvider.CreateScope();
            logger = scope.ServiceProvider.GetRequiredService<ILogger<LoadBalancerPlatformKafkaHandler>>();
            DBContext = scope.ServiceProvider.GetRequiredService<PrinubesPlatformWorkerDBContext>();
            kafkaProducer = scope.ServiceProvider.GetRequiredService<IMessageProducer>();
        }

        public async Task Handle(MessageNotification<LoadBalancerPlatformTestingRequestKafkaMessage> notification, CancellationToken cancellationToken)
        {
            LoadBalancerPlatformTestingRequestKafkaMessage loadbalancerPlatformKafkaMessage = notification.Message;
            if (loadbalancerPlatformKafkaMessage != null)
            {
                logger.LogInformation($"LoadbalancerPlatform testing message received with key: {loadbalancerPlatformKafkaMessage.LoadBalancerPlatform.Platform} and action: {loadbalancerPlatformKafkaMessage.Action}");
                switch (loadbalancerPlatformKafkaMessage.Action)
                {
                    case ActionEnum.test:
                        switch (loadbalancerPlatformKafkaMessage.LoadBalancerPlatform.PlatformType)
                        {
                            case LoadBalancerPlatformType.NSXTALB:
                                await RecordExistanceConfirmation.CredentialExistsAsync(loadbalancerPlatformKafkaMessage.LoadBalancerPlatform.CredentialID, logger, DBContext);
                                KafkaMessage.SubmitKafkaMessageAync(
                                new LoadBalancerPlatformTestingResponseModel()
                                {
                                    Message = "Sucess!!",
                                    Success = true,
                                }, logger, kafkaProducer, topic: loadbalancerPlatformKafkaMessage.ReturnTopic);
                                logger.LogInformation($"LoadbalancerPlatform test message sent to topic: {loadbalancerPlatformKafkaMessage.ReturnTopic}");

                                break;
                            default:
                                logger.LogInformation($"LoadbalancerPlatform type not known: {loadbalancerPlatformKafkaMessage.LoadBalancerPlatform.PlatformType}");

                                break;
                        }
                        break;

                    default:
                        logger.LogError($"LoadbalancerPlatform message action not implemented {loadbalancerPlatformKafkaMessage.Action}");
                        break;
                }
            }
            else
            {
                logger.LogError($"LoadbalancerPlatform message received null value");
            }
            return;
        }
    }
}
