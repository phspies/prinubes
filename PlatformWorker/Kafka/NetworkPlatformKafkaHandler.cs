using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Helpers;
using Prinubes.Common.Kafka;
using Prinubes.Platforms.Datamodels;
using Prinubes.PlatformWorkers.Helpers;
using System.Collections;

namespace Prinubes.PlatformWorker.Kafka
{
    public class NetworkPlatformKafkaHandler : INotificationHandler<MessageNotification<NetworkPlatformKafkaMessage>>
    {
        private readonly ILogger<NetworkPlatformKafkaHandler> logger;
        private readonly PrinubesPlatformWorkerDBContext DBContext;
        private string cachingListKey = "networkplatformslist";
        private IDistributedCache distributedCaching;

        public NetworkPlatformKafkaHandler(IServiceProvider _serviceProvider)
        {
            var scope = _serviceProvider.CreateScope();
            logger = scope.ServiceProvider.GetRequiredService<ILogger<NetworkPlatformKafkaHandler>>();
            DBContext = scope.ServiceProvider.GetRequiredService<PrinubesPlatformWorkerDBContext>();
            distributedCaching = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        }

        public async Task Handle(MessageNotification<NetworkPlatformKafkaMessage> notification, CancellationToken cancellationToken)
        {
            NetworkPlatformKafkaMessage networkplatformKafkaMessage = notification.Message;
            if (networkplatformKafkaMessage != null)
            {
                logger.LogInformation($"NetworkPlatform message received with key: {networkplatformKafkaMessage.NetworkPlatformID} and action: {networkplatformKafkaMessage.Action}");
                switch (networkplatformKafkaMessage.Action)
                {
                    case ActionEnum.create:
                        await RecordExistanceConfirmation.OrganizationExistsAsync(networkplatformKafkaMessage.OrganizationID, logger, DBContext);
                        if (!DBContext.NetworkPlatforms.Any(x => x.Id == networkplatformKafkaMessage.NetworkPlatformID))
                        {
                            var networkplatform = new NetworkPlatformDatabaseModel();
                            PropertyCopier.Populate(networkplatformKafkaMessage.NetworkPlatform, networkplatform, new string[] { "Organization", "Credential" });
                            DBContext.NetworkPlatforms.Add(networkplatform);
                            DBContext.SaveChanges();
                            distributedCaching.SetCaching(networkplatform, networkplatform.Id.ToString());
                            distributedCaching.Remove(cachingListKey);
                        }
                        else
                        {
                            logger.LogDebug($"NetworkPlatform message, networkplatform already exists: {networkplatformKafkaMessage.NetworkPlatformID} with row version: {networkplatformKafkaMessage.RowVersion}");
                        }
                        break;
                    case ActionEnum.update:
                        var updateNetworkPlatform = DBContext.NetworkPlatforms.FirstOrDefault(x => x.Id == networkplatformKafkaMessage.NetworkPlatformID && x.OrganizationID == networkplatformKafkaMessage.OrganizationID);
                        if (updateNetworkPlatform != null)
                        {
                            if (StructuralComparisons.StructuralEqualityComparer.Equals(updateNetworkPlatform.RowVersion, networkplatformKafkaMessage.RowVersion))
                            {
                                PropertyCopier.Populate(networkplatformKafkaMessage.NetworkPlatform, updateNetworkPlatform, new string[] { "Organization", "Credential" });
                                DBContext.SaveChanges();
                                distributedCaching.SetCaching(updateNetworkPlatform, updateNetworkPlatform.Id.ToString());
                                distributedCaching.Remove(cachingListKey);
                            }
                            else
                            {
                                logger.LogDebug($"NetworkPlatform message out of order, networkplatform rowversion not found: {networkplatformKafkaMessage.NetworkPlatformID} and row version: {networkplatformKafkaMessage.RowVersion}");
                            }
                        }
                        else
                        {
                            logger.LogDebug($"NetworkPlatform message out of order, networkplatform not found: {networkplatformKafkaMessage.NetworkPlatformID} and row version: {networkplatformKafkaMessage.RowVersion}");
                        }
                        break;
                    case ActionEnum.delete:
                        var deleteNetworkPlatform = DBContext.NetworkPlatforms.FirstOrDefault(x => x.Id == networkplatformKafkaMessage.NetworkPlatformID && x.OrganizationID == networkplatformKafkaMessage.OrganizationID);
                        if (deleteNetworkPlatform != null && CommonHelpers.ByteArrayCompare(deleteNetworkPlatform.RowVersion, networkplatformKafkaMessage.RowVersion))
                        {
                            DBContext.NetworkPlatforms.Remove(deleteNetworkPlatform);
                            DBContext.SaveChanges();
                            distributedCaching.Remove(deleteNetworkPlatform.Id.ToString());
                            distributedCaching.Remove(cachingListKey);
                        }
                        else
                        {
                            logger.LogDebug($"NetworkPlatform message out of order, networkplatform rowversion not found: {networkplatformKafkaMessage.NetworkPlatformID} and row version: {networkplatformKafkaMessage.RowVersion}");
                        }
                        break;
                    default:
                        logger.LogError($"NetworkPlatform message action not implemented {networkplatformKafkaMessage.Action}");
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
