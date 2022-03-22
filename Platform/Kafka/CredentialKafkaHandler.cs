using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Kafka;
using Prinubes.Common.Helpers;
using Prinubes.Platforms.Datamodels;
using System.Collections;
using Prinubes.Platform.Helpers;

namespace Prinubes.ComputePlatform.Kafka
{
    public class CredentialKafkaHandler : INotificationHandler<MessageNotification<CredentialKafkaMessage>>
    {
        private readonly ILogger<CredentialKafkaHandler> logger;
        private readonly PrinubesPlatformDBContext DBContext;
        private IMapper mapper;
        private string cachingListKey = "credentialslist";
        private IDistributedCache distributedCaching;

        public CredentialKafkaHandler(IServiceProvider _serviceProvider)
        {
            DBContext = _serviceProvider.GetRequiredService<PrinubesPlatformDBContext>();
            logger = _serviceProvider.GetRequiredService<ILogger<CredentialKafkaHandler>>();
            mapper = _serviceProvider.GetRequiredService<IMapper>();
            distributedCaching = _serviceProvider.GetRequiredService<IDistributedCache>();
        }

        public async Task Handle(MessageNotification<CredentialKafkaMessage> notification, CancellationToken cancellationToken)
        {
            CredentialKafkaMessage credentialKafkaMessage = notification.Message;
            if (credentialKafkaMessage != null)
            {
                logger.LogInformation($"Credential message received with key: {credentialKafkaMessage.CredentialID} and action: {credentialKafkaMessage.Action}");
                switch (credentialKafkaMessage.Action)
                {
                    case ActionEnum.create:
                        await RecordExistanceConfirmation.OrganizationExistsAsync(credentialKafkaMessage.OrganizationID, logger, DBContext);
                        if (!DBContext.Credentials.Any(x => x.Id == credentialKafkaMessage.CredentialID))
                        {
                            var credential = new CredentialDatabaseModel();
                            PropertyCopier.Populate(credentialKafkaMessage.Credential, credential);
                            DBContext.Credentials.Add(credential);
                            DBContext.SaveChanges();
                            distributedCaching.SetCaching(credential, credential.Id.ToString());
                            distributedCaching.Remove(cachingListKey);
                        }
                        else
                        {
                            logger.LogDebug($"Credential message, credential already exists: {credentialKafkaMessage.CredentialID} with row version: {credentialKafkaMessage.RowVersion}");
                        }
                        break;
                    case ActionEnum.update:
                        var updateCredential = DBContext.Credentials.FirstOrDefault(x => x.Id == credentialKafkaMessage.CredentialID && x.OrganizationID == credentialKafkaMessage.OrganizationID);
                        if (updateCredential != null)
                        {
                            if (StructuralComparisons.StructuralEqualityComparer.Equals(updateCredential.RowVersion, credentialKafkaMessage.RowVersion))
                            {
                                PropertyCopier.Populate(credentialKafkaMessage.Credential, updateCredential);
                                DBContext.SaveChanges();
                                distributedCaching.SetCaching(updateCredential, updateCredential.Id.ToString());
                                distributedCaching.Remove(cachingListKey);
                            }
                            else
                            {
                                logger.LogDebug($"Credential message out of order, credential not found: {credentialKafkaMessage.CredentialID} and row version: {credentialKafkaMessage.RowVersion}");
                            }
                        }
                        else
                        {
                            logger.LogDebug($"Credential message out of order, credential not found: {credentialKafkaMessage.CredentialID} and row version: {credentialKafkaMessage.RowVersion}");
                        }
                        break;
                    case ActionEnum.delete:
                        var deleteCredential = DBContext.Credentials.FirstOrDefault(x => x.Id == credentialKafkaMessage.CredentialID && x.OrganizationID == credentialKafkaMessage.OrganizationID);
                        if (deleteCredential != null && CommonHelpers.ByteArrayCompare(deleteCredential.RowVersion, credentialKafkaMessage.RowVersion))
                        {
                            DBContext.Credentials.Remove(deleteCredential);
                            DBContext.SaveChanges();
                            distributedCaching.Remove(deleteCredential.Id.ToString());
                            distributedCaching.Remove(cachingListKey);
                        }
                        else
                        {
                            logger.LogDebug($"Credential message out of order, credential not found: {credentialKafkaMessage.CredentialID} and row version: {credentialKafkaMessage.RowVersion}");
                        }
                        break;
                    default:
                        logger.LogError($"Credential message action not implemented {credentialKafkaMessage.Action}");
                        break;
                }
            }
            else
            {
                logger.LogError($"Credential message received null value");
            }
            return;
        }
    }
}
