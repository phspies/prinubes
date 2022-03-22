using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Kafka;
using Prinubes.Common.Helpers;
using Prinubes.Platforms.Datamodels;
using System.Collections;
using Prinubes.PlatformWorkers.Helpers;

namespace Prinubes.LoadBalancerPlatform.Kafka
{
    public class LoadBalancerPlatformKafkaHandler : INotificationHandler<MessageNotification<LoadBalancerPlatformKafkaMessage>>
    {
        private readonly ILogger<LoadBalancerPlatformKafkaHandler> logger;
        private readonly PrinubesPlatformWorkerDBContext DBContext;
        private IMapper mapper;
        private string cachingListKey = "loadbalancerplatformslist";
        private IDistributedCache distributedCaching;

        public LoadBalancerPlatformKafkaHandler(IServiceProvider _serviceProvider)
        {
            DBContext = _serviceProvider.GetRequiredService<PrinubesPlatformWorkerDBContext>();
            logger = _serviceProvider.GetRequiredService<ILogger<LoadBalancerPlatformKafkaHandler>>();
            mapper = _serviceProvider.GetRequiredService<IMapper>();
            distributedCaching = _serviceProvider.GetRequiredService<IDistributedCache>();
        }

        public async Task Handle(MessageNotification<LoadBalancerPlatformKafkaMessage> notification, CancellationToken cancellationToken)
        {
            LoadBalancerPlatformKafkaMessage loadbalancerplatformKafkaMessage = notification.Message;
            if (loadbalancerplatformKafkaMessage != null)
            {
                logger.LogInformation($"LoadBalancerPlatform message received with key: {loadbalancerplatformKafkaMessage.LoadBalancerPlatformID} and action: {loadbalancerplatformKafkaMessage.Action}");
                switch (loadbalancerplatformKafkaMessage.Action)
                {
                    case ActionEnum.create:
                        await RecordExistanceConfirmation.OrganizationExistsAsync(loadbalancerplatformKafkaMessage.OrganizationID, logger, DBContext);
                        if (!DBContext.LoadBalancerPlatforms.Any(x => x.Id == loadbalancerplatformKafkaMessage.LoadBalancerPlatformID))
                        {
                            var loadbalancerplatform = new LoadBalancerPlatformDatabaseModel();
                            PropertyCopier.Populate(loadbalancerplatformKafkaMessage.LoadBalancerPlatform, loadbalancerplatform, new string[] {"Organization", "Credential" });
                            DBContext.LoadBalancerPlatforms.Add(loadbalancerplatform);
                            DBContext.SaveChanges();
                            distributedCaching.SetCaching(loadbalancerplatform, loadbalancerplatform.Id.ToString());
                            distributedCaching.Remove(cachingListKey);
                        }
                        else
                        {
                            logger.LogDebug($"LoadBalancerPlatform message, loadbalancerplatform already exists: {loadbalancerplatformKafkaMessage.LoadBalancerPlatformID} with row version: {loadbalancerplatformKafkaMessage.RowVersion}");
                        }
                        break;
                    case ActionEnum.update:
                        var updateLoadBalancerPlatform = DBContext.LoadBalancerPlatforms.FirstOrDefault(x => x.Id == loadbalancerplatformKafkaMessage.LoadBalancerPlatformID && x.OrganizationID == loadbalancerplatformKafkaMessage.OrganizationID);
                        if (updateLoadBalancerPlatform != null)
                        {
                            if (StructuralComparisons.StructuralEqualityComparer.Equals(updateLoadBalancerPlatform.RowVersion, loadbalancerplatformKafkaMessage.RowVersion))
                            {
                                PropertyCopier.Populate(loadbalancerplatformKafkaMessage.LoadBalancerPlatform, updateLoadBalancerPlatform, new string[] { "Organization", "Credential" });
                                DBContext.SaveChanges();
                                distributedCaching.SetCaching(updateLoadBalancerPlatform, updateLoadBalancerPlatform.Id.ToString());
                                distributedCaching.Remove(cachingListKey);
                            }
                            else
                            {
                                logger.LogDebug($"LoadBalancerPlatform message out of order, loadbalancerplatform not found: {loadbalancerplatformKafkaMessage.LoadBalancerPlatformID} and row version: {loadbalancerplatformKafkaMessage.RowVersion}");
                            }
                        }
                        else
                        {
                            logger.LogDebug($"LoadBalancerPlatform message out of order, loadbalancerplatform not found: {loadbalancerplatformKafkaMessage.LoadBalancerPlatformID} and row version: {loadbalancerplatformKafkaMessage.RowVersion}");
                        }
                        break;
                    case ActionEnum.delete:
                        var deleteLoadBalancerPlatform = DBContext.LoadBalancerPlatforms.FirstOrDefault(x => x.Id == loadbalancerplatformKafkaMessage.LoadBalancerPlatformID && x.OrganizationID == loadbalancerplatformKafkaMessage.OrganizationID);
                        if (deleteLoadBalancerPlatform != null && CommonHelpers.ByteArrayCompare(deleteLoadBalancerPlatform.RowVersion, loadbalancerplatformKafkaMessage.RowVersion))
                        {
                            DBContext.LoadBalancerPlatforms.Remove(deleteLoadBalancerPlatform);
                            DBContext.SaveChanges();
                            distributedCaching.Remove(deleteLoadBalancerPlatform.Id.ToString());
                            distributedCaching.Remove(cachingListKey);
                        }
                        else
                        {
                            logger.LogDebug($"LoadBalancerPlatform message out of order, loadbalancerplatform not found: {loadbalancerplatformKafkaMessage.LoadBalancerPlatformID} and row version: {loadbalancerplatformKafkaMessage.RowVersion}");
                        }
                        break;
                    default:
                        logger.LogError($"LoadBalancerPlatform message action not implemented {loadbalancerplatformKafkaMessage.Action}");
                        break;
                }
            }
            else
            {
                logger.LogError($"LoadBalancerPlatform message received null value");
            }
            return;
        }
    }
}
