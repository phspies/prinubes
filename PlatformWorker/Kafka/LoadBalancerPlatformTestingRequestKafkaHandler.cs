using MediatR;
using nsxtalbsdk;
using nsxtalbsdk.Models;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.DatabaseModels.PlatformEnums;
using Prinubes.Common.Kafka;
using Prinubes.Common.Kafka.Producer;
using Prinubes.PlatformWorker.CloudLibraries.NSXTALB;
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
                                NSXTALBFactory nsxtalbfactory = new NSXTALBFactory(loadbalancerPlatformKafkaMessage.LoadBalancerPlatform, DBContext, logger);
                                KafkaMessage.SubmitKafkaMessageAync(await nsxtalbfactory.TestCredentials(), logger, kafkaProducer, topic: loadbalancerPlatformKafkaMessage.ReturnTopic);
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
