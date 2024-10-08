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
    public class ComputePlatformKafkaHandler : INotificationHandler<MessageNotification<ComputePlatformKafkaMessage>>
    {
        private readonly ILogger<ComputePlatformKafkaHandler> logger;
        private readonly PrinubesPlatformWorkerDBContext DBContext;
        private readonly GlobalComputePlatformBackgroundWorker computeWorker;
        
        private string cachingListKey = "computeplatformslist";
        private IDistributedCache distributedCaching;

        public ComputePlatformKafkaHandler(IServiceProvider _serviceProvider)
        {
            var scope = _serviceProvider.CreateScope();
            logger = ServiceActivator.GetRequiredService<ILogger<ComputePlatformKafkaHandler>>(_serviceProvider);
            DBContext = ServiceActivator.GetRequiredService<PrinubesPlatformWorkerDBContext>(_serviceProvider);
            distributedCaching = ServiceActivator.GetRequiredService<IDistributedCache>(_serviceProvider);
            computeWorker = ServiceActivator.GetRequiredService<GlobalComputePlatformBackgroundWorker>(_serviceProvider);
        }

        public async Task Handle(MessageNotification<ComputePlatformKafkaMessage> notification, CancellationToken cancellationToken)
        {
            ComputePlatformKafkaMessage computeplatformKafkaMessage = notification.Message;
            if (computeplatformKafkaMessage != null)
            {
                logger.LogInformation($"ComputePlatform message received with key: {computeplatformKafkaMessage.ComputePlatformID} and action: {computeplatformKafkaMessage.Action}");
                switch (computeplatformKafkaMessage.Action)
                {
                    case ActionEnum.create:
                        await RecordExistanceConfirmation.OrganizationExistsAsync(computeplatformKafkaMessage.OrganizationID, logger, DBContext);
                        if (!DBContext.ComputePlatforms.Any(x => x.Id == computeplatformKafkaMessage.ComputePlatformID))
                        {
                            var computeplatform = new ComputePlatformDatabaseModel();
                            PropertyCopier.Populate(computeplatformKafkaMessage.ComputePlatform, computeplatform, new string[] { "Organization", "Credential" });
                            DBContext.ComputePlatforms.Add(computeplatform);
                            DBContext.SaveChanges();
                            distributedCaching.SetCaching(computeplatform, computeplatform.Id.ToString());
                            distributedCaching.Remove(cachingListKey);
                            await computeWorker.AddPlatformAsync(computeplatformKafkaMessage.ComputePlatformID);
                        }
                        else
                        {
                            logger.LogDebug($"ComputePlatform message, computeplatform already exists: {computeplatformKafkaMessage.ComputePlatformID} with row version: {computeplatformKafkaMessage.RowVersion}");
                        }
                        break;
                    case ActionEnum.update:
                        var updateComputePlatform = DBContext.ComputePlatforms.FirstOrDefault(x => x.Id == computeplatformKafkaMessage.ComputePlatformID && x.OrganizationID == computeplatformKafkaMessage.OrganizationID);
                        if (updateComputePlatform != null)
                        {
                            if (StructuralComparisons.StructuralEqualityComparer.Equals(updateComputePlatform.RowVersion, computeplatformKafkaMessage.RowVersion))
                            {
                                PropertyCopier.Populate(computeplatformKafkaMessage.ComputePlatform, updateComputePlatform, new string[] { "Organization", "Credential" });
                                DBContext.SaveChanges();
                                distributedCaching.SetCaching(updateComputePlatform, updateComputePlatform.Id.ToString());
                                distributedCaching.Remove(cachingListKey);
                            }
                            else
                            {
                                logger.LogDebug($"ComputePlatform message out of order, computeplatform not found: {computeplatformKafkaMessage.ComputePlatformID} and row version: {computeplatformKafkaMessage.RowVersion}");
                            }
                        }
                        else
                        {
                            logger.LogDebug($"ComputePlatform message out of order, computeplatform not found: {computeplatformKafkaMessage.ComputePlatformID} and row version: {computeplatformKafkaMessage.RowVersion}");
                        }
                        break;
                    case ActionEnum.delete:
                        var deleteComputePlatform = DBContext.ComputePlatforms.FirstOrDefault(x => x.Id == computeplatformKafkaMessage.ComputePlatformID && x.OrganizationID == computeplatformKafkaMessage.OrganizationID);
                        if (deleteComputePlatform != null && CommonHelpers.ByteArrayCompare(deleteComputePlatform.RowVersion, computeplatformKafkaMessage.RowVersion))
                        {
                            await computeWorker.StopPlatformAsync(computeplatformKafkaMessage.ComputePlatformID);
                            DBContext.ComputePlatforms.Remove(deleteComputePlatform);
                            DBContext.SaveChanges();
                            distributedCaching.Remove(deleteComputePlatform.Id.ToString());
                            distributedCaching.Remove(cachingListKey);
                        }
                        else
                        {
                            logger.LogDebug($"ComputePlatform message out of order, computeplatform not found: {computeplatformKafkaMessage.ComputePlatformID} and row version: {computeplatformKafkaMessage.RowVersion}");
                        }
                        break;
                    default:
                        logger.LogError($"ComputePlatform message action not implemented {computeplatformKafkaMessage.Action}");
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
