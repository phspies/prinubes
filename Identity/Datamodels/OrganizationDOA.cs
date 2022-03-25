using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Helpers;
using Prinubes.Common.Kafka.Producer;

namespace Prinubes.Identity.Datamodels
{
    public class OrganizationDOA : IDisposable
    {
        private PrinubesIdentityDBContext dbContext;
        private ILogger logger;
        private IMessageProducer kafkaProducer;
        private IMapper mapper;
        private string cachingListKey = "organizationslist";
        private IDistributedCache distributedCaching;

        public OrganizationDOA(IServiceProvider _serviceProvider)
        {
            dbContext = _serviceProvider.GetRequiredService<PrinubesIdentityDBContext>();
            logger = _serviceProvider.GetRequiredService<ILogger<GroupDOA>>();
            mapper = _serviceProvider.GetRequiredService<IMapper>();
            kafkaProducer = _serviceProvider.GetRequiredService<IMessageProducer>();
            distributedCaching = _serviceProvider.GetRequiredService<IDistributedCache>();
        }
        public async Task<Common.DatabaseModels.OrganizationDatabaseModel> CreateAsync(OrganizationCRUDDataModel organization)
        {
            try
            {
                Common.DatabaseModels.OrganizationDatabaseModel privateOrganization = mapper.Map<Common.DatabaseModels.OrganizationDatabaseModel>(organization);
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    privateOrganization.Id = System.Guid.NewGuid();
                    privateOrganization = dbContext.Organizations.Add(privateOrganization).Entity;
                    await dbContext.SaveChangesAsync();
                    KafkaMessage.SubmitKafkaMessageAync(
                        new OrganizationKafkaMessage()
                        {
                            Action = ActionEnum.create,
                            OrganizationID = privateOrganization.Id,
                            Organization = privateOrganization
                        },
                        logger,
                        kafkaProducer);
                    await transaction.CommitAsync();
                    await distributedCaching.SetCachingAsync(privateOrganization, privateOrganization.Id.ToString());
                    await distributedCaching.RemoveAsync(cachingListKey);
                }
                return privateOrganization;

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
        public async Task<Common.DatabaseModels.OrganizationDatabaseModel> attachGroupAsync(System.Guid organizationID, System.Guid groupId)
        {

            Common.DatabaseModels.OrganizationDatabaseModel privateOrganization = await dbContext.Organizations.SingleAsync((System.Linq.Expressions.Expression<Func<Common.DatabaseModels.OrganizationDatabaseModel, bool>>)(x => x.Id == organizationID));
            GroupDatabaseModel privateGroup = await dbContext.Groups.SingleAsync(x => x.Id == groupId);
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    privateOrganization.Groups.Add(privateGroup);
                    KafkaMessage.SubmitKafkaMessageAync(
                          new OrganizationKafkaMessage()
                          {
                              Action = ActionEnum.attach,
                              OrganizationID = privateOrganization.Id,
                              Organization = privateOrganization
                          },
                          logger,
                          kafkaProducer);
                    await transaction.CommitAsync();
                    return privateOrganization;
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
        public async Task<Common.DatabaseModels.OrganizationDatabaseModel> detachGroupAsync(System.Guid organizationID, System.Guid groupId)
        {

            Common.DatabaseModels.OrganizationDatabaseModel privateOrganization = dbContext.Organizations.Include(x => x.Groups).First((System.Linq.Expressions.Expression<Func<Common.DatabaseModels.OrganizationDatabaseModel, bool>>)(x => x.Id == organizationID));
            GroupDatabaseModel privateGroup = dbContext.Groups.First(x => x.Id == groupId && x.OrganizationID == organizationID);
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    privateOrganization.Groups.Remove(privateGroup);
                    KafkaMessage.SubmitKafkaMessageAync(
                          new OrganizationKafkaMessage()
                          {
                              Action = ActionEnum.detach,
                              OrganizationID = privateOrganization.Id,
                              Organization = privateOrganization
                          },
                          logger,
                          kafkaProducer);
                    await transaction.CommitAsync();
                    return privateOrganization;
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
        }
        public async Task<List<Common.DatabaseModels.OrganizationDatabaseModel>> GetListAsync()
        {
            try
            {
                List<Common.DatabaseModels.OrganizationDatabaseModel> organizationList;
                var cachedList = await distributedCaching.GetStringAsync(cachingListKey);
                if (cachedList == null)
                {
                    organizationList = await dbContext.Organizations.ToListAsync();
                    await distributedCaching.SetCachingAsync(organizationList, cachingListKey);
                }
                else
                {
                    organizationList = JsonConvert.DeserializeObject<List<Common.DatabaseModels.OrganizationDatabaseModel>>(cachedList) ?? new List<Common.DatabaseModels.OrganizationDatabaseModel>();
                }
                return organizationList;
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
        public async Task<Common.DatabaseModels.OrganizationDatabaseModel> GetByIDAsync(System.Guid id)
        {
            try
            {
                Common.DatabaseModels.OrganizationDatabaseModel organization;
                var cachedOrganization = await distributedCaching.GetStringAsync(id.ToString());
                if (cachedOrganization == null)
                {
                    organization = await dbContext.Organizations.SingleAsync(x => x.Id == id);
                    await distributedCaching.SetCachingAsync(organization, id.ToString());
                }
                else
                {
                    organization = JsonConvert.DeserializeObject<Common.DatabaseModels.OrganizationDatabaseModel>(cachedOrganization) ?? new Common.DatabaseModels.OrganizationDatabaseModel();
                }
                return organization;
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
        public async Task<Common.DatabaseModels.OrganizationDatabaseModel> UpdateAsync(System.Guid id, OrganizationCRUDDataModel organization)
        {
            try
            {
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    var updatedOrganization = await dbContext.Organizations.SingleAsync(x => x.Id == id);
                    PropertyCopier.Populate(organization, updatedOrganization);
                    await dbContext.SaveChangesAsync();
                    KafkaMessage.SubmitKafkaMessageAync(
                          new OrganizationKafkaMessage()
                          {
                              Action = ActionEnum.update,
                              OrganizationID = id,
                              Organization = updatedOrganization,
                              RowVersion = organization.RowVersion
                          },
                          logger,
                          kafkaProducer);
                    await transaction.CommitAsync();
                    await distributedCaching.SetCachingAsync(updatedOrganization, id.ToString());
                    await distributedCaching.RemoveAsync(cachingListKey);
                    return updatedOrganization;
                }
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
        public async Task DeleteAsync(System.Guid id)
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var organizationItem = await dbContext.Organizations.SingleAsync(x => x.Id == id);
                    dbContext.Organizations.Remove(organizationItem);
                    await dbContext.SaveChangesAsync();
                    KafkaMessage.SubmitKafkaMessageAync(
                            new OrganizationKafkaMessage()
                            {
                                Action = ActionEnum.delete,
                                OrganizationID = organizationItem.Id,
                                Organization = organizationItem,
                                RowVersion = organizationItem.RowVersion
                            },
                            logger,
                            kafkaProducer);
                    await transaction.CommitAsync();
                    await distributedCaching.RefreshAsync(id.ToString());
                    await distributedCaching.RemoveAsync(cachingListKey);
                }
                catch (DbUpdateException mysqlex)
                {
                    transaction.Rollback();
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


        public void Dispose()
        {
            //((IDisposable)dbContext).Dispose();
        }

        public async Task<bool> OrganizationExistsAsync(System.Guid id) => await dbContext.Organizations.AnyAsync(e => e.Id == id);

    }
}
