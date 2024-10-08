﻿using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Helpers;
using Prinubes.Common.Kafka;
using Prinubes.PlatformWorker.BackgroundWorkers;
using Prinubes.PlatformWorker.Datamodels;
using Prinubes.PlatformWorker.Helpers;
using System.Collections;

namespace Prinubes.PlatformWorker.Kafka
{
    public class LoadBalancerPlatformKafkaHandler : INotificationHandler<MessageNotification<LoadBalancerPlatformKafkaMessage>>
    {
        private readonly ILogger<LoadBalancerPlatformKafkaHandler> logger;
        private readonly PrinubesPlatformWorkerDBContext DBContext;
        private string cachingListKey = "loadbalancerplatformslist";
        private IDistributedCache distributedCaching;
        private GlobalLoadBalancerPlatformBackgroundWorker loadbalancerWorker;


        public LoadBalancerPlatformKafkaHandler(IServiceProvider _serviceProvider)
        {
            logger = ServiceActivator.GetRequiredService<ILogger<LoadBalancerPlatformKafkaHandler>>(_serviceProvider);
            DBContext = ServiceActivator.GetRequiredService<PrinubesPlatformWorkerDBContext>(_serviceProvider);
            distributedCaching = ServiceActivator.GetRequiredService<IDistributedCache>(_serviceProvider);
            loadbalancerWorker = ServiceActivator.GetRequiredService<GlobalLoadBalancerPlatformBackgroundWorker>(_serviceProvider);
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
                            PropertyCopier.Populate(loadbalancerplatformKafkaMessage.LoadBalancerPlatform, loadbalancerplatform, new string[] { "Organization", "Credential" });
                            DBContext.LoadBalancerPlatforms.Add(loadbalancerplatform);
                            DBContext.SaveChanges();
                            distributedCaching.SetCaching(loadbalancerplatform, loadbalancerplatform.Id.ToString());
                            distributedCaching.Remove(cachingListKey);
                            await loadbalancerWorker.AddPlatformAsync(loadbalancerplatformKafkaMessage.LoadBalancerPlatformID);
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
                            await loadbalancerWorker.StopPlatformAsync(loadbalancerplatformKafkaMessage.LoadBalancerPlatformID);
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
