using MediatR;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.DatabaseModels.PlatformEnums;
using Prinubes.Common.Kafka;
using Prinubes.Common.Kafka.Producer;
using Prinubes.Platforms.Datamodels;
using Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware;
using Prinubes.PlatformWorkers.Helpers;

namespace Prinubes.PlatformWorker.Kafka
{
    public class ComputePlatformTestingRequestKafkaHandler : INotificationHandler<MessageNotification<ComputePlatformTestingRequestKafkaMessage>>
    {
        private ILogger logger;
        private IMessageProducer kafkaProducer;
        private PrinubesPlatformWorkerDBContext DBContext;

        public ComputePlatformTestingRequestKafkaHandler(IServiceProvider _serviceProvider)
        {
            var scope = _serviceProvider.CreateScope();
            logger = scope.ServiceProvider.GetRequiredService<ILogger<ComputePlatformKafkaHandler>>();
            DBContext = scope.ServiceProvider.GetRequiredService<PrinubesPlatformWorkerDBContext>();
            kafkaProducer = scope.ServiceProvider.GetRequiredService<IMessageProducer>();
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
                                string endpointDetails;
                                bool success = false;
                                try
                                {
                                    var credentials = DBContext.Credentials.Find(computePlatformKafkaMessage.ComputePlatform.CredentialID);
                                    var hostname = computePlatformKafkaMessage.ComputePlatform.UrlEndpoint;
                                    ArgumentNullException.ThrowIfNull(credentials);
                                    ArgumentNullException.ThrowIfNull(hostname);
                                    VCService vcService = new VCService();
                                    vcService.Logon(hostname, credentials.Username, credentials.DecryptedPassword);
                                    endpointDetails = vcService.ApiVersion;
                                    success = true;
                                }
                                catch (Exception ex)
                                {
                                    endpointDetails = ex.Message;
                                }
                                KafkaMessage.SubmitKafkaMessageAync(
                                new ComputePlatformTestingResponseModel()
                                {
                                    Message = endpointDetails,
                                    Success = success,
                                }, logger, kafkaProducer, topic: computePlatformKafkaMessage.ReturnTopic);
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
