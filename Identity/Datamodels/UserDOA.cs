using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Helpers;
using Prinubes.Common.Kafka.Producer;

namespace Prinubes.Identity.Datamodels
{
    public class UserDOA : IDisposable
    {
        private PrinubesIdentityDBContext userDBContext;
        private ILogger logger;
        private IMessageProducer kafkaProducer;
        private IMapper mapper;
        private IDistributedCache distributedCaching;
        private string cachingListKey = "userlist";

        public UserDOA(IServiceProvider serviceProvider)
        {
            userDBContext = serviceProvider.GetRequiredService<PrinubesIdentityDBContext>();
            logger = serviceProvider.GetRequiredService<ILogger<UserDOA>>();
            kafkaProducer = serviceProvider.GetRequiredService<IMessageProducer>();
            mapper = serviceProvider.GetRequiredService<IMapper>();
            distributedCaching = serviceProvider.GetRequiredService<IDistributedCache>();
        }
        public async Task<UserDatabaseModel> AuthenticateAsync(string emailAddress, string password)
        {
            if (string.IsNullOrEmpty(emailAddress) || string.IsNullOrEmpty(password))
                return null;

            var cachingKey = emailAddress;
            UserDatabaseModel user;
            var cachedUser = await distributedCaching.GetStringAsync(cachingKey);
            if (cachedUser == null)
            {
                user = await userDBContext.Users.FirstOrDefaultAsync(x => x.EmailAddress == emailAddress);
                if (user != null)
                {
                    await distributedCaching.SetCachingAsync(user, cachingKey);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                user = JsonConvert.DeserializeObject<UserDatabaseModel>(cachedUser) ?? new UserDatabaseModel();
            }

            // check if password is correct
            if (!CipherService.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // authentication successful
            return user;
        }
        public async Task<UserDatabaseModel> CreateAsync(UserCRUDDataModel user)
        {
            UserDatabaseModel newUser;
            using (var transaction = userDBContext.Database.BeginTransaction())
            {
                try
                {
                    UserDatabaseModel privateUser = mapper.Map<UserDatabaseModel>(user);
                    privateUser.Id = Guid.NewGuid();
                    byte[] passwordHash, passwordSalt;
                    CipherService.CreatePasswordHash(user.Password, out passwordHash, out passwordSalt);

                    privateUser.PasswordHash = passwordHash;
                    privateUser.PasswordSalt = passwordSalt;

                    newUser = (await userDBContext.Users.AddAsync(privateUser)).Entity;
                    await userDBContext.SaveChangesAsync();
                    KafkaMessage.SubmitKafkaMessageAync(
                        new UserKafkaMessage()
                        {
                            Action = ActionEnum.create,
                            UserID = newUser.Id,
                            User = newUser
                        },
                        logger,
                        kafkaProducer);
                    await transaction.CommitAsync();
                    await distributedCaching.SetCachingAsync(newUser, newUser.Id.ToString());
                    await distributedCaching.SetCachingAsync(newUser, newUser.EmailAddress);
                    await distributedCaching.RemoveAsync(cachingListKey);
                    return newUser;
                }
                catch (DbUpdateException mysqlex)
                {
                    await transaction.RollbackAsync();
                    logger.LogError(mysqlex.InnerException?.Message);
                    throw new InvalidOperationException(mysqlex.InnerException?.Message);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    logger.LogError(ex.Message);
                    throw;
                }
            }
        }
        public async Task<List<UserDatabaseModel>> GetListAsync()
        {

            List<UserDatabaseModel> userList;
            try
            {
                var cachedList = await distributedCaching.GetStringAsync(cachingListKey);
                if (cachedList == null)
                {
                    userList = await userDBContext.Users.Include(x => x.Groups).ToListAsync();
                    await distributedCaching.SetCachingAsync(userList, cachingListKey);
                }
                else
                {
                    userList = JsonConvert.DeserializeObject<List<UserDatabaseModel>>(cachedList) ?? new List<UserDatabaseModel>();
                }
                return userList;
            }
            catch (DbUpdateException mysqlex)
            {
                logger.LogError(mysqlex.InnerException?.Message);
                throw new InvalidOperationException(mysqlex.InnerException?.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                throw;
            }
        }
        public async Task<UserDatabaseModel> GetByIDAsync(Guid id)
        {
            try
            {
                var cachingKey = id.ToString();
                UserDatabaseModel user;
                var cachedUser = await distributedCaching.GetStringAsync(cachingKey);
                if (cachedUser == null)
                {
                    user = await userDBContext.Users.SingleAsync(x => x.Id == id);
                    await distributedCaching.SetCachingAsync(user, cachingKey);
                }
                else
                {
                    user = JsonConvert.DeserializeObject<UserDatabaseModel>(cachedUser) ?? new UserDatabaseModel();
                }
                return user;
            }
            catch (DbUpdateException mysqlex)
            {
                logger.LogError(mysqlex.InnerException?.Message);
                throw new InvalidOperationException(mysqlex.InnerException?.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                throw;
            }
        }
        public async Task<UserDatabaseModel> GetByEmailAddressAsync(string emailAddress)
        {
            try
            {
                var cachingKey = emailAddress;
                UserDatabaseModel user;
                var cachedUser = await distributedCaching.GetStringAsync(cachingKey);
                if (cachedUser == null)
                {
                    user = await userDBContext.Users.SingleAsync(x => x.EmailAddress == emailAddress);
                    await distributedCaching.SetCachingAsync(user, cachingKey);
                }
                else
                {
                    user = JsonConvert.DeserializeObject<UserDatabaseModel>(cachedUser) ?? new UserDatabaseModel();
                }
                return user;
            }
            catch (DbUpdateException mysqlex)
            {
                logger.LogError(mysqlex.InnerException?.Message);
                throw new InvalidOperationException(mysqlex.InnerException?.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                throw;
            }
        }
        public async Task<UserDatabaseModel> UpdateAsync(Guid id, UserCRUDDataModel user)
        {
            UserDatabaseModel updateUser;
            using (var transaction = userDBContext.Database.BeginTransaction())
            {
                try
                {
                    updateUser = await userDBContext.Users.SingleAsync(x => x.Id == id);
                    PropertyCopier.Populate(user, updateUser);
                    await userDBContext.SaveChangesAsync();
                    KafkaMessage.SubmitKafkaMessageAync(
                        new UserKafkaMessage()
                        {
                            Action = ActionEnum.update,
                            UserID = updateUser.Id,
                            User = updateUser,
                            RowVersion = user.RowVersion
                        },
                        logger,
                        kafkaProducer);
                    await transaction.CommitAsync();
                    await distributedCaching.SetCachingAsync(updateUser, id.ToString());
                    await distributedCaching.SetCachingAsync(updateUser, updateUser.EmailAddress);
                    await distributedCaching.RemoveAsync(cachingListKey);
                    return updateUser;
                }
                catch (DbUpdateException mysqlex)
                {
                    await transaction.RollbackAsync();
                    logger.LogError(mysqlex.InnerException?.Message);
                    throw new InvalidOperationException(mysqlex.InnerException?.Message);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    logger.LogError(ex.Message);
                    throw;
                }
            }
        }
        public async Task DeleteAsync(Guid id)
        {
            using (var transaction = userDBContext.Database.BeginTransaction())
            {
                try
                {
                    var userItem = await userDBContext.Users.SingleAsync(x => x.Id == id);
                    userDBContext.Users.Remove(userItem);
                    await userDBContext.SaveChangesAsync();
                    KafkaMessage.SubmitKafkaMessageAync(
                        new UserKafkaMessage()
                        {
                            Action = ActionEnum.delete,
                            UserID = userItem.Id,
                            User = userItem,
                            RowVersion = userItem.RowVersion
                        },
                        logger,
                        kafkaProducer);
                    await transaction.CommitAsync();
                    await distributedCaching.RemoveAsync(id.ToString());
                    await distributedCaching.RemoveAsync(userItem.EmailAddress);
                    await distributedCaching.RemoveAsync(cachingListKey);
                }
                catch (DbUpdateException mysqlex)
                {
                    await transaction.RollbackAsync();
                    logger.LogError(mysqlex.InnerException?.Message);
                    throw new InvalidOperationException(mysqlex.InnerException?.Message);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    logger.LogError(ex.Message);
                    throw;
                }
            }
        }


        public void Dispose()
        {
            //((IDisposable)userDBContext).Dispose();
        }
        public async Task<bool> UserIdExistsAsync(Guid id) => await this.userDBContext.Users.AnyAsync(e => e.Id == id);
        public async Task<bool> UserEmailAddressExistsAsync(string emailAddress) => await this.userDBContext.Users.AnyAsync(e => e.EmailAddress == emailAddress);



    }
}
