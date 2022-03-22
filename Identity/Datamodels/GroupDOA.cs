using AutoMapper;
using Prinubes.Common.Kafka.Producer;
using Microsoft.EntityFrameworkCore;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Helpers;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Prinubes.Identity.Datamodels
{
    public class GroupDOA : IDisposable
    {
        private PrinubesIdentityDBContext dbContext;
        private ILogger logger;
        private IMessageProducer kafkaProducer;
        private IMapper mapper;
        private IDistributedCache distributedCaching;
        private string cachingListKey = "groupslist";
        private Guid organizationId;


        public GroupDOA(Guid _organizationId, IServiceProvider _serviceProvider)
        {
            dbContext = _serviceProvider.GetRequiredService<PrinubesIdentityDBContext>();
            logger = _serviceProvider.GetRequiredService<ILogger<UserDOA>>();
            kafkaProducer = _serviceProvider.GetRequiredService<IMessageProducer>();
            mapper = _serviceProvider.GetRequiredService<IMapper>();
            organizationId = _organizationId;
            distributedCaching = _serviceProvider.GetRequiredService<IDistributedCache>();
        }
        public async Task<GroupDatabaseModel> CreateAsync(GroupCRUDDataModel group)
        {

            GroupDatabaseModel privateGroup = new GroupDatabaseModel();
            PropertyCopier.Populate(group, privateGroup);
            privateGroup.OrganizationID = organizationId;
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    dbContext.Groups.Add(privateGroup);
                    dbContext.SaveChanges();
                    KafkaMessage.SubmitKafkaMessageAync(
                        new GroupKafkaMessage()
                        {
                            Action = ActionEnum.create,
                            GroupID = privateGroup.Id,
                            Group = mapper.Map<GroupKafkaDataModel>(privateGroup),
                            OrganizationID = organizationId
                        },
                        logger,
                        kafkaProducer);
                    transaction.Commit();
                    await distributedCaching.SetCachingAsync(privateGroup, privateGroup.Id.ToString());
                    await distributedCaching.RemoveAsync(cachingListKey);
                    return privateGroup;
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
        public async Task<GroupDatabaseModel> attachUserAsync(System.Guid groupID, System.Guid userId)
        {


            GroupDatabaseModel privateGroup = await dbContext.Groups.Include(x => x.Users).SingleAsync(x => x.Id == groupID && x.OrganizationID == organizationId);
            UserDatabaseModel privateUser = await dbContext.Users.SingleAsync(x => x.Id == userId);
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    privateGroup.Users.Add(privateUser);
                    await dbContext.SaveChangesAsync();

                    KafkaMessage.SubmitKafkaMessageAync(
                        new GroupKafkaMessage()
                        {
                            Action = ActionEnum.attach,
                            GroupID = privateGroup.Id,
                            Group = mapper.Map<GroupKafkaDataModel>(privateGroup),
                            OrganizationID = organizationId,
                            UserID = userId,
                            RowVersion = privateGroup.RowVersion
                        },
                        logger,
                        kafkaProducer);
                    await transaction.CommitAsync();
                    await distributedCaching.SetCachingAsync(privateGroup, groupID.ToString());
                    return privateGroup;
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
        public async Task<GroupDatabaseModel> detachUserAsync(System.Guid groupID, System.Guid userId)
        {

            GroupDatabaseModel privateGroup = await dbContext.Groups.Include(x => x.Users).SingleAsync(x => x.Id == groupID && x.OrganizationID == organizationId);
            UserDatabaseModel privateUser = await dbContext.Users.SingleAsync(x => x.Id == userId);
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    privateGroup.Users.Remove(privateUser);
                    await dbContext.SaveChangesAsync();
                    KafkaMessage.SubmitKafkaMessageAync(
                        new GroupKafkaMessage()
                        {
                            Action = ActionEnum.detach,
                            GroupID = privateGroup.Id,
                            Group = mapper.Map<GroupKafkaDataModel>(privateGroup),
                            OrganizationID = organizationId,
                            UserID = userId,
                            RowVersion = privateGroup.RowVersion
                        },
                        logger,
                        kafkaProducer);
                    await transaction.CommitAsync();
                    await distributedCaching.SetCachingAsync(privateGroup, groupID.ToString());
                    return privateGroup;


                }
                catch (DbUpdateException mysqlex)
                {
                    await transaction.RollbackAsync();
                    logger.LogError(mysqlex.InnerException?.Message);
                    throw new DbUpdateException(mysqlex.InnerException?.Message);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    logger.LogError(ex.Message);
                    throw;
                }
            }
        }
        public async Task<List<GroupDatabaseModel>> GetListAsync()
        {
            try
            {
                List<GroupDatabaseModel> groupList;
                var cachedList = await distributedCaching.GetStringAsync(cachingListKey);
                if (cachedList == null)
                {
                    groupList = await dbContext.Groups.Where(x => x.OrganizationID == organizationId).ToListAsync();
                    await distributedCaching.SetCachingAsync(groupList, cachingListKey);
                }
                else
                {
                    groupList = JsonConvert.DeserializeObject<List<GroupDatabaseModel>>(cachedList) ?? new List<GroupDatabaseModel>();
                }
                return groupList;
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
        public async Task<GroupDatabaseModel> GetByIDAsync(System.Guid id)
        {
            try
            {
                GroupDatabaseModel group;
                var cachedGroup = await distributedCaching.GetStringAsync(id.ToString());
                if (cachedGroup == null)
                {
                    group = await dbContext.Groups.SingleAsync(x => x.Id == id && x.OrganizationID == organizationId);
                    await distributedCaching.SetCachingAsync(group, id.ToString());
                }
                else
                {
                    group = JsonConvert.DeserializeObject<GroupDatabaseModel>(cachedGroup) ?? new GroupDatabaseModel();
                }
                return group;
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
        public async Task<GroupDatabaseModel> UpdateAsync(System.Guid id, GroupCRUDDataModel group)
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var updatedGroup = await dbContext.Groups.SingleAsync(x => x.Id == id);
                    PropertyCopier.Populate(group, updatedGroup);
                    await dbContext.SaveChangesAsync();
                    KafkaMessage.SubmitKafkaMessageAync(
                        new GroupKafkaMessage()
                        {
                            Action = ActionEnum.update,
                            GroupID = id,
                            Group = mapper.Map<GroupKafkaDataModel>(updatedGroup),
                            OrganizationID = organizationId,
                            RowVersion = group.RowVersion
                        },
                        logger,
                        kafkaProducer);
                    await transaction.CommitAsync();
                    await distributedCaching.SetCachingAsync(updatedGroup, id.ToString());
                    await distributedCaching.RemoveAsync(cachingListKey);
                    return updatedGroup;

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
        public async Task DeleteAsync(System.Guid groupId)
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var group = await dbContext.Groups.SingleAsync(x => x.Id == groupId);
                    dbContext.Groups.Remove(group);
                    await dbContext.SaveChangesAsync();
                    KafkaMessage.SubmitKafkaMessageAync(
                        new GroupKafkaMessage()
                        {
                            Action = ActionEnum.delete,
                            GroupID = groupId,
                            OrganizationID = organizationId,
                            RowVersion = group.RowVersion
                        },
                        logger,
                        kafkaProducer);
                    transaction.Commit();
                    await distributedCaching.RemoveAsync(cachingListKey);
                    await distributedCaching.RemoveAsync(groupId.ToString());
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
        }

        public async Task<bool> GroupExistsAsync(System.Guid id) => await dbContext.Groups.AnyAsync(e => e.Id == id && e.OrganizationID == organizationId);
        public async Task<bool> GroupNameExistsAsync(String group) => await dbContext.Groups.AnyAsync(e => e.Group == group && e.OrganizationID == organizationId);
        public async Task<bool> GroupUserExistsAsync(System.Guid groupId, System.Guid userUid) => (await dbContext.Groups.Include(x => x.Users).SingleAsync(e => e.Id == groupId && e.OrganizationID == organizationId)).Users.Any(y => y.Id == userUid);

    }
}
