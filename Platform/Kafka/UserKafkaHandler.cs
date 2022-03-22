using MediatR;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Helpers;
using Prinubes.Common.Kafka;
using Prinubes.Platforms.Datamodels;

namespace Prinubes.ComputePlatform.Kafka
{
    public class UserKafkaHandler : INotificationHandler<MessageNotification<UserKafkaMessage>>
    {
        private readonly ILogger<UserKafkaHandler> _logger;
        private readonly PrinubesPlatformDBContext DBContext;

        public UserKafkaHandler(ILogger<UserKafkaHandler> logger, PrinubesPlatformDBContext _DBContext)
        {
            _logger = logger;
            DBContext = _DBContext;
        }

        public Task Handle(MessageNotification<UserKafkaMessage> notification, CancellationToken cancellationToken)
        {
            UserKafkaMessage userKafkaMessage = notification.Message;
            if (userKafkaMessage != null)
            {
                _logger.LogInformation($"User message received with key: {userKafkaMessage.UserID} and action: {userKafkaMessage.Action}");
                switch (userKafkaMessage.Action)
                {
                    case ActionEnum.create:
                        if (!DBContext.Users.Any(x => x.Id == userKafkaMessage.UserID))
                        {
                            DBContext.Users.Add(userKafkaMessage.User);
                            DBContext.SaveChanges();
                        }
                        else
                        {
                            _logger.LogDebug($"User message, user already exists: {userKafkaMessage.UserID} with row version: {userKafkaMessage.User.RowVersion}");
                        }
                        break;
                    case ActionEnum.update:
                        var updateUser = DBContext.Users.FirstOrDefault(x => x.Id == userKafkaMessage.UserID);
                        if (updateUser != null)
                        {
                            if (CommonHelpers.ByteArrayCompare(updateUser.RowVersion, userKafkaMessage.RowVersion))
                            {
                                PropertyCopier.Populate(userKafkaMessage.User, updateUser);
                                DBContext.SaveChanges();
                            }
                            else
                            {
                                _logger.LogDebug($"User message out of order, user not found: {userKafkaMessage.UserID} and row version: {userKafkaMessage.User.RowVersion}");
                            }
                        }
                        else
                        {
                            _logger.LogDebug($"User message out of order, user not found: {userKafkaMessage.UserID} and row version: {userKafkaMessage.User.RowVersion}");
                        }
                        break;
                    case ActionEnum.delete:
                        var deleteUser = DBContext.Users.FirstOrDefault(x => x.Id == userKafkaMessage.UserID);
                        if (deleteUser != null)
                        {
                            if (CommonHelpers.ByteArrayCompare(deleteUser.RowVersion, userKafkaMessage.RowVersion))
                            {
                                DBContext.Users.Remove(deleteUser);
                                DBContext.SaveChanges();
                            }
                            else
                            {
                                _logger.LogDebug($"User message out of order, user not found: {userKafkaMessage.UserID} and row version: {userKafkaMessage.User.RowVersion}");
                            }
                        }
                        else
                        {
                            _logger.LogDebug($"User message out of order, user not found: {userKafkaMessage.UserID} and row version: {userKafkaMessage.User.RowVersion}");
                        }
                        break;
                    default:
                        _logger.LogError($"User message action not implemented {userKafkaMessage.Action}");
                        break;
                }
            }
            else
            {
                _logger.LogError($"User message received null value");
            }
            return Task.CompletedTask;
        }
    }
}
