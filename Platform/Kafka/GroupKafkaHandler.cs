using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Helpers;
using Prinubes.Common.Kafka;
using Prinubes.Platform.Helpers;
using Prinubes.Platforms.Datamodels;

namespace Prinubes.ComputePlatform.Kafka
{
    public class GroupKafkaHandler : INotificationHandler<MessageNotification<GroupKafkaMessage>>
    {
        private readonly ILogger<GroupKafkaHandler> logger;
        private readonly PrinubesPlatformDBContext DBContext;
        private IMapper mapper;

        public GroupKafkaHandler(IServiceProvider _serviceProvider)
        {
            var scope = _serviceProvider.CreateScope();
            logger = scope.ServiceProvider.GetRequiredService<ILogger<GroupKafkaHandler>>();
            mapper = _serviceProvider.GetRequiredService<IMapper>();
            DBContext = scope.ServiceProvider.GetRequiredService<PrinubesPlatformDBContext>();
        }

        public async Task Handle(MessageNotification<GroupKafkaMessage> notification, CancellationToken cancellationToken)
        {
            GroupKafkaMessage groupKafkaMessage = notification.Message;
            if (groupKafkaMessage != null)
            {
                logger.LogInformation($"Group message received with key: {groupKafkaMessage.GroupID} and action: {groupKafkaMessage.Action}");
                switch (groupKafkaMessage.Action)
                {
                    case ActionEnum.create:
                        try
                        {
                            await RecordExistanceConfirmation.OrganizationExistsAsync(groupKafkaMessage.OrganizationID, logger, DBContext);
                            if (!DBContext.Groups.Any(x => x.Id == groupKafkaMessage.GroupID && x.OrganizationID == groupKafkaMessage.OrganizationID))
                            {
                                DBContext.Groups.Add(mapper.Map<GroupDatabaseModel>(groupKafkaMessage.Group));
                                DBContext.SaveChanges();
                            }
                            else
                            {
                                logger.LogDebug($"Group message, group already exists: {groupKafkaMessage.GroupID} with row version: {groupKafkaMessage.Group.RowVersion}");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogDebug(ex, "Group Kafka Create event");
                        }
                        break;
                    case ActionEnum.update:
                        try
                        {
                            var updateGroup = DBContext.Groups.FirstOrDefault(x => x.Id == groupKafkaMessage.GroupID && x.OrganizationID == groupKafkaMessage.OrganizationID);
                            if (updateGroup != null)
                            {
                                if (CommonHelpers.ByteArrayCompare(updateGroup.RowVersion, groupKafkaMessage.RowVersion))
                                {
                                    PropertyCopier.Populate(groupKafkaMessage.Group, updateGroup);
                                    DBContext.SaveChanges();
                                }
                                else
                                {
                                    logger.LogDebug($"Group message out of order, group not found: {groupKafkaMessage.GroupID} and row version: {groupKafkaMessage.Group.RowVersion}");
                                }
                            }
                            else
                            {
                                logger.LogDebug($"Group message out of order, group not found: {groupKafkaMessage.GroupID} and row version: {groupKafkaMessage.Group.RowVersion}");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogDebug(ex, "Group Kafka Update event");
                        }
                        break;
                    case ActionEnum.attach:
                    case ActionEnum.detach:
                        try
                        {
                            var attachGroup = DBContext.Groups.Include(x => x.Users).FirstOrDefault(x => x.Id == groupKafkaMessage.GroupID && x.OrganizationID == groupKafkaMessage.OrganizationID);
                            var userObject = DBContext.Users.FirstOrDefault(x => x.Id == groupKafkaMessage.UserID);
                            if (attachGroup != null && userObject != null)
                            {
                                if (CommonHelpers.ByteArrayCompare(attachGroup.RowVersion, groupKafkaMessage.RowVersion))
                                {
                                    if (groupKafkaMessage.Action == ActionEnum.attach)
                                    {
                                        attachGroup.Users.Add(userObject);
                                    }
                                    else if (groupKafkaMessage.Action == ActionEnum.detach)
                                    {
                                        attachGroup.Users.Remove(userObject);
                                    }
                                    DBContext.SaveChanges();
                                }
                                else
                                {
                                    logger.LogDebug($"Group message out of order, group not found: {groupKafkaMessage.GroupID} and row version: {groupKafkaMessage.Group.RowVersion}");
                                }
                            }
                            else
                            {
                                logger.LogDebug($"Group message out of order, group not found: {groupKafkaMessage.GroupID} and row version: {groupKafkaMessage.Group.RowVersion}");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogDebug(ex, "Group Kafka Attach/Detach event");
                        }
                        break;
                    case ActionEnum.delete:
                        try
                        {
                            var deleteGroup = DBContext.Groups.FirstOrDefault(x => x.Id == groupKafkaMessage.GroupID && x.OrganizationID == groupKafkaMessage.OrganizationID);
                            if (deleteGroup != null)
                            {
                                if (CommonHelpers.ByteArrayCompare(deleteGroup.RowVersion, groupKafkaMessage.RowVersion))
                                {
                                    DBContext.Groups.Remove(deleteGroup);
                                    DBContext.SaveChanges();
                                }
                                else
                                {
                                    logger.LogDebug($"Group message out of order, group not found: {groupKafkaMessage.GroupID} and row version: {groupKafkaMessage.Group.RowVersion}");
                                }
                            }
                            else
                            {
                                logger.LogDebug($"Group message out of order, group not found: {groupKafkaMessage.GroupID} and row version: {groupKafkaMessage.Group.RowVersion}");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogDebug(ex, "Group Kafka Delete event");
                        }
                        break;
                    default:
                        logger.LogError($"Group message action not implemented {groupKafkaMessage.Action}");
                        break;
                }
            }
            else
            {
                logger.LogError($"Group message received null value");
            }
            return;
        }
    }
}
