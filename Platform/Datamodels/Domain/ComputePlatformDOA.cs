using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Helpers;
using Prinubes.Common.Kafka;
using Prinubes.Common.Kafka.Consumer;
using Prinubes.Common.Kafka.Producer;
using Prinubes.Common.Models;

namespace Prinubes.Platforms.Datamodels
{
    public class ComputePlatformDOA : IDisposable
    {
        private PrinubesPlatformDBContext dbContext;
        private ILogger logger;
        private Guid organizationId;
        private IMessageProducer kafkaProducer;
        private string cachingListKey = "computeplatformlist";
        private IDistributedCache distributedCaching;
        private IKafkaConsumerBuilder kafkaConsumerBuilder;
        private ServiceSettings serviceSettings;

        public ComputePlatformDOA(Guid _organizationId, IServiceProvider _serviceProvider)
        {
            organizationId = _organizationId;
            dbContext = _serviceProvider.GetRequiredService<PrinubesPlatformDBContext>();
            logger = _serviceProvider.GetRequiredService<ILogger<ComputePlatformDOA>>();
            kafkaProducer = _serviceProvider.GetRequiredService<IMessageProducer>();
            distributedCaching = _serviceProvider.GetRequiredService<IDistributedCache>();
            kafkaConsumerBuilder = _serviceProvider.GetRequiredService<IKafkaConsumerBuilder>();
            serviceSettings = _serviceProvider.GetRequiredService<ServiceSettings>();
        }

        public async Task<ComputePlatformTestingResponseModel> TestConnectionAsync(ComputePlatformCRUDDataModel computePlatform)
        {
            ComputePlatformDatabaseModel platform = new ComputePlatformDatabaseModel();
            PropertyCopier.Populate(computePlatform, platform);
            platform.CredentialID = computePlatform.CredentialID;

            var requestObject = new ComputePlatformTestingRequestKafkaMessage()
            {
                ComputePlatform = platform,
                Action = ActionEnum.test
            };
            KafkaHelpers.CreateTopic(requestObject.ReturnTopic, serviceSettings, logger);
            KafkaMessage.SubmitKafkaMessageAync(requestObject, logger, kafkaProducer);
            ComputePlatformTestingResponseModel response = KafkaHelpers.ConsumeTopicAdhoc<ComputePlatformTestingResponseModel>(requestObject.ReturnTopic, kafkaConsumerBuilder, logger);
            KafkaHelpers.DeleteTopic(requestObject.ReturnTopic, serviceSettings);

            return response;
        }
        public async Task<ComputePlatformDatabaseModel> CreateAsync(ComputePlatformCRUDDataModel computePlatform)
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var newComputePlatform = new ComputePlatformDatabaseModel() { OrganizationID = organizationId };
                    PropertyCopier.Populate(computePlatform, newComputePlatform, new string[] { "OrganizationID" });
                    dbContext.ComputePlatforms.Add(newComputePlatform);
                    await dbContext.SaveChangesAsync();
                    KafkaMessage.SubmitKafkaMessageAync(
                        new ComputePlatformKafkaMessage()
                        {
                            Action = ActionEnum.create,
                            ComputePlatformID = newComputePlatform.Id,
                            ComputePlatform = newComputePlatform,
                            OrganizationID = organizationId
                        },
                        logger,
                        kafkaProducer);
                    await transaction.CommitAsync();
                    await distributedCaching.SetCachingAsync(computePlatform, newComputePlatform.Id.ToString());
                    await distributedCaching.RemoveAsync(cachingListKey);
                    await dbContext.Entry(newComputePlatform).Reference(x => x.Credential).LoadAsync();
                    await dbContext.Entry(newComputePlatform).Reference(x => x.Organization).LoadAsync();
                    return newComputePlatform;
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
        public async Task<List<ComputePlatformDatabaseModel>> GetListAsync()
        {
            try
            {
                List<ComputePlatformDatabaseModel> computePlatformList;
                var cachedList = await distributedCaching.GetStringAsync(cachingListKey);
                if (cachedList == null)
                {
                    computePlatformList = await dbContext.ComputePlatforms.Include(x => x.Credential).Include(x => x.Organization)
                        .Where(x => x.OrganizationID == organizationId).ToListAsync();
                    await distributedCaching.SetCachingAsync(computePlatformList, cachingListKey);
                }
                else
                {
                    computePlatformList = JsonConvert.DeserializeObject<List<ComputePlatformDatabaseModel>>(cachedList) ?? new List<ComputePlatformDatabaseModel>();
                }
                return computePlatformList;
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
        public async Task<ComputePlatformDatabaseModel> GetByIDAsync(Guid id)
        {
            try
            {
                var computePlatform = await dbContext.ComputePlatforms.SingleAsync(x => x.Id == id && x.OrganizationID == organizationId);
                await dbContext.Entry(computePlatform).Reference(x => x.Credential).LoadAsync();
                await dbContext.Entry(computePlatform).Reference(x => x.Organization).LoadAsync();
                return computePlatform;
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
        public async Task<ComputePlatformDatabaseModel> UpdateAsync(Guid id, ComputePlatformCRUDDataModel computePlatform)
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    ComputePlatformDatabaseModel updateComputePlatform = await dbContext.ComputePlatforms.SingleAsync(x => x.Id == id && x.OrganizationID == organizationId);
                    await dbContext.Entry(updateComputePlatform).Reference(x => x.Credential).LoadAsync();
                    await dbContext.Entry(updateComputePlatform).Reference(x => x.Organization).LoadAsync();
                    PropertyCopier.Populate(computePlatform, updateComputePlatform, new string[] { "OrganizationID" });
                    await dbContext.SaveChangesAsync();
                    KafkaMessage.SubmitKafkaMessageAync(
                        new ComputePlatformKafkaMessage()
                        {
                            Action = ActionEnum.update,
                            ComputePlatformID = id,
                            ComputePlatform = updateComputePlatform,
                            OrganizationID = organizationId,
                            RowVersion = computePlatform.RowVersion
                        },
                        logger,
                        kafkaProducer);
                    await transaction.CommitAsync();
                    await distributedCaching.SetCachingAsync(updateComputePlatform, id.ToString());
                    await distributedCaching.RemoveAsync(cachingListKey);
                    await dbContext.Entry(updateComputePlatform).Reference(x => x.Credential).LoadAsync();
                    await dbContext.Entry(updateComputePlatform).Reference(x => x.Organization).LoadAsync();
                    return updateComputePlatform;
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
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var computePlatformItem = await dbContext.ComputePlatforms.SingleAsync(x => x.Id == id && x.OrganizationID == organizationId);
                    dbContext.ComputePlatforms.Remove(computePlatformItem);
                    await dbContext.SaveChangesAsync();
                    KafkaMessage.SubmitKafkaMessageAync(
                        new ComputePlatformKafkaMessage()
                        {
                            Action = ActionEnum.update,
                            ComputePlatformID = id,
                            OrganizationID = organizationId,
                            RowVersion = computePlatformItem.RowVersion
                        },
                        logger,
                        kafkaProducer);
                    transaction.Commit();
                    await distributedCaching.RemoveAsync(cachingListKey);
                    await distributedCaching.RemoveAsync(id.ToString());
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
            //  ((IDisposable)dbContext).Dispose();
        }
        public async Task<bool> ComputePlatformIdExistsAsync(Guid id) => await dbContext.ComputePlatforms.AnyAsync(e => e.Id == id && e.OrganizationID == organizationId);

        public async Task<bool> CredentialsIdExistsAsync(Guid id) => await dbContext.Credentials.AnyAsync(e => e.Id == id && e.OrganizationID == organizationId);
    }
}
