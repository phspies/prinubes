using MediatR;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Helpers;
using Prinubes.Common.Kafka;
using Prinubes.PlatformWorker.Datamodels;

namespace Prinubes.PlatformWorker.Kafka
{
    public class OrganizationKafkaHandler : INotificationHandler<MessageNotification<OrganizationKafkaMessage>>
    {
        private readonly ILogger<OrganizationKafkaHandler> logger;
        private readonly PrinubesPlatformWorkerDBContext DBContext;

        public OrganizationKafkaHandler(IServiceProvider _serviceProvider)
        {
            var scope = _serviceProvider.CreateScope();
            logger = scope.ServiceProvider.GetRequiredService<ILogger<OrganizationKafkaHandler>>();
            DBContext = scope.ServiceProvider.GetRequiredService<PrinubesPlatformWorkerDBContext>();
        }

        public Task Handle(MessageNotification<OrganizationKafkaMessage> notification, CancellationToken cancellationToken)
        {
            OrganizationKafkaMessage organizationKafkaMessage = notification.Message;
            if (organizationKafkaMessage != null)
            {
                logger.LogInformation($"Organization message received with key: {organizationKafkaMessage.OrganizationID} and action: {organizationKafkaMessage.Action}");
                switch (organizationKafkaMessage.Action)
                {
                    case ActionEnum.create:
                        if (!DBContext.Organizations.Any(x => x.Id == organizationKafkaMessage.OrganizationID))
                        {
                            DBContext.Organizations.Add(organizationKafkaMessage.Organization);
                            DBContext.SaveChanges();
                        }
                        else
                        {
                            logger.LogDebug($"Organization message, organization already exists: {organizationKafkaMessage.OrganizationID} with row version: {organizationKafkaMessage.Organization.RowVersion}");
                        }
                        break;
                    case ActionEnum.update:
                        var updateOrganization = DBContext.Organizations.FirstOrDefault(x => x.Id == organizationKafkaMessage.OrganizationID);
                        if (updateOrganization != null)
                        {
                            if (CommonHelpers.ByteArrayCompare(updateOrganization.RowVersion, organizationKafkaMessage.RowVersion))
                            {
                                PropertyCopier.Populate(organizationKafkaMessage.Organization, updateOrganization);
                                DBContext.SaveChanges();
                            }
                            else
                            {
                                logger.LogDebug($"Organization message out of order, organization not found: {organizationKafkaMessage.OrganizationID} and row version: {organizationKafkaMessage.Organization.RowVersion}");
                            }
                        }
                        else
                        {
                            logger.LogDebug($"Organization message out of order, organization not found: {organizationKafkaMessage.OrganizationID} and row version: {organizationKafkaMessage.Organization.RowVersion}");
                        }
                        break;
                    case ActionEnum.delete:
                        var deleteOrganization = DBContext.Organizations.FirstOrDefault(x => x.Id == organizationKafkaMessage.OrganizationID);
                        if (deleteOrganization != null)
                        {
                            if (CommonHelpers.ByteArrayCompare(deleteOrganization.RowVersion, organizationKafkaMessage.RowVersion))
                            {
                                DBContext.Organizations.Remove(deleteOrganization);
                                DBContext.SaveChanges();
                            }
                            else
                            {
                                logger.LogDebug($"Organization message out of order, organization not found: {organizationKafkaMessage.OrganizationID} and row version: {organizationKafkaMessage.Organization.RowVersion}");
                            }
                        }
                        else
                        {
                            logger.LogDebug($"Organization message out of order, organization not found: {organizationKafkaMessage.OrganizationID} and row version: {organizationKafkaMessage.Organization.RowVersion}");
                        }
                        break;
                    default:
                        logger.LogError($"Organization message action not implemented {organizationKafkaMessage.Action}");
                        break;
                }
            }
            else
            {
                logger.LogError($"Organization message received null value");
            }
            return Task.CompletedTask;
        }
    }
}
