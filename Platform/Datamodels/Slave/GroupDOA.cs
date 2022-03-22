using AutoMapper;
using Prinubes.Common.Kafka;
using Prinubes.Common.Kafka.Producer;
using Microsoft.EntityFrameworkCore;
using Prinubes.Common.DatabaseModels;
using Prinubes.Platforms.Datamodels;

namespace Prinubes.Platforms.Datamodels.Slave
{
    public class GroupDOA : IDisposable
    {
        private PrinubesPlatformDBContext dbContext;
        private ILogger logger;
        private IMapper mapper;
        private Guid organizationId;
        private IMessageProducer kafkaProducer;

        public GroupDOA(Guid _organizationId, IServiceProvider _serviceProvider)
        {
            organizationId = _organizationId;
            dbContext = _serviceProvider.GetRequiredService<PrinubesPlatformDBContext>();
            logger = _serviceProvider.GetRequiredService<ILogger<GroupDOA>>();
            mapper = _serviceProvider.GetRequiredService<IMapper>();
            kafkaProducer = _serviceProvider.GetRequiredService<IMessageProducer>();

        }
        public async Task<GroupDatabaseModel> CreateAsync(GroupCRUDDataModel group)
        {

            GroupDatabaseModel privateGroup = mapper.Map<GroupDatabaseModel>(group);
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    privateGroup.OrganizationID = organizationId;
                    dbContext.Groups.Add(privateGroup);
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
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
        public async Task<GroupDatabaseModel> attachUserAsync(Guid groupID, Guid userId)
        {
            GroupDatabaseModel privateGroup = await dbContext.Groups.Include(x => x.Organization).SingleAsync(x => x.Id == groupID && x.OrganizationID == organizationId);
            UserDatabaseModel privateUser = await dbContext.Users.SingleAsync(x => x.Id == userId);
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    privateGroup.Users.Add(privateUser);
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
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
                return privateGroup;
            }
        }
        public async Task<GroupDatabaseModel> detachUserAsync(Guid groupID, Guid userId)
        {

            GroupDatabaseModel privateGroup = await dbContext.Groups.Include(x => x.Organization).SingleAsync(x => x.Id == groupID && x.OrganizationID == organizationId);
            UserDatabaseModel privateUser = await dbContext.Users.SingleAsync(x => x.Id == userId);
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    privateGroup.Users.Remove(privateUser);
                    await dbContext.SaveChangesAsync();

                    await transaction.CommitAsync();
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
            return privateGroup;

        }
        public async Task<List<GroupDatabaseModel>> GetListAsync()
        {
            try
            {
                return await dbContext.Groups.Include(b => b.Users).Include(x => x.Organization).Where(x => x.Id == organizationId).ToListAsync();
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
        public async Task<GroupDatabaseModel> GetByIDAsync(Guid id)
        {
            try
            {
                return await dbContext.Groups.Include(b => b.Users).SingleAsync(x => x.Id == id && x.OrganizationID == organizationId);
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
        public async Task<GroupDatabaseModel> UpdateAsync(Guid id, GroupCRUDDataModel group)
        {

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var updatedGroup = await dbContext.Groups.Include(x => x.Organization).Include(x => x.Users).SingleAsync(x => x.Id == id && x.OrganizationID == organizationId);
                    updatedGroup = mapper.Map<GroupCRUDDataModel, GroupDatabaseModel>(group);
                    updatedGroup.OrganizationID = organizationId;
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
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
        public async Task DeleteAsync(Guid groupId)
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    dbContext.Groups.Remove(await dbContext.Groups.SingleAsync(x => x.Id == groupId && x.OrganizationID == organizationId));
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
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
            //((IDisposable)dbContext).Dispose();
        }

        public async Task<bool> GroupExistsAsync(Guid id) => await dbContext.Groups.AnyAsync(e => e.Id == id && e.OrganizationID == organizationId);
        public async Task<bool> GroupUserExistsAsync(Guid groupId, Guid userUid) => await dbContext.Groups.Where(e => e.Id == groupId &&  e.OrganizationID == organizationId).AnyAsync(x => x.Users.Any(y => y.Id == userUid));

    }
}
