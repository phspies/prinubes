using MediatR;
using nsxtalbsdk;
using nsxtalbsdk.Models;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.DatabaseModels.PlatformEnums;
using Prinubes.Common.Helpers;
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
            logger = ServiceActivator.GetRequiredService<ILogger<LoadBalancerPlatformTestingRequestKafkaHandler>>(_serviceProvider);
            DBContext = ServiceActivator.GetRequiredService<PrinubesPlatformWorkerDBContext>(_serviceProvider);
            kafkaProducer = ServiceActivator.GetRequiredService<IMessageProducer>(_serviceProvider);
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
