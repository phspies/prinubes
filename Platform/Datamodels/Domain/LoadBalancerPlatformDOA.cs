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
    public class LoadBalancerPlatformDOA : IDisposable
    {
        private PrinubesPlatformDBContext dbContext;
        private ILogger logger;
        private Guid organizationId;
        private IMessageProducer kafkaProducer;
        private string cachingListKey = "loadbalancerplatformlist";
        private IDistributedCache distributedCaching;
        private IKafkaConsumerBuilder kafkaConsumerBuilder;
        private ServiceSettings serviceSettings;

        public LoadBalancerPlatformDOA(Guid _organizationId, IServiceProvider _serviceProvider)
        {
            organizationId = _organizationId;
            dbContext = _serviceProvider.GetRequiredService<PrinubesPlatformDBContext>();
            logger = _serviceProvider.GetRequiredService<ILogger<LoadBalancerPlatformDOA>>();
            kafkaProducer = _serviceProvider.GetRequiredService<IMessageProducer>();
            distributedCaching = _serviceProvider.GetRequiredService<IDistributedCache>();
            kafkaConsumerBuilder = _serviceProvider.GetRequiredService<IKafkaConsumerBuilder>();
            serviceSettings = _serviceProvider.GetRequiredService<ServiceSettings>();
        }
        public async Task<LoadBalancerPlatformTestingResponseModel> TestConnectionAsync(LoadBalancerPlatformCRUDDataModel computePlatform)
        {
            LoadBalancerPlatformDatabaseModel platform = new LoadBalancerPlatformDatabaseModel();
            PropertyCopier.Populate(computePlatform, platform);
            platform.CredentialID = computePlatform.CredentialID;

            var requestObject = new LoadBalancerPlatformTestingRequestKafkaMessage()
            {
                LoadBalancerPlatform = platform,
                Action = ActionEnum.test
            };
            KafkaHelpers.CreateTopic(requestObject.ReturnTopic, serviceSettings, logger);
            KafkaMessage.SubmitKafkaMessageAync(requestObject, logger, kafkaProducer);
            LoadBalancerPlatformTestingResponseModel response = KafkaHelpers.ConsumeTopicAdhoc<LoadBalancerPlatformTestingResponseModel>(requestObject.ReturnTopic, kafkaConsumerBuilder, logger);
            KafkaHelpers.DeleteTopic(requestObject.ReturnTopic, serviceSettings);
            return response;
        }
        public async Task<LoadBalancerPlatformDatabaseModel> CreateAsync(LoadBalancerPlatformCRUDDataModel loadbalancerPlatform)
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var newLoadBalancerPlatform = new LoadBalancerPlatformDatabaseModel() { OrganizationID = organizationId };
                    PropertyCopier.Populate(loadbalancerPlatform, newLoadBalancerPlatform, new string[] { "OrganizationID" });
                    dbContext.LoadBalancerPlatforms.Add(newLoadBalancerPlatform);
                    await dbContext.SaveChangesAsync();
                    KafkaMessage.SubmitKafkaMessageAync(
                        new LoadBalancerPlatformKafkaMessage()
                        {
                            Action = ActionEnum.create,
                            LoadBalancerPlatformID = newLoadBalancerPlatform.Id,
                            LoadBalancerPlatform = newLoadBalancerPlatform,
                            OrganizationID = organizationId
                        },
                        logger,
                        kafkaProducer);
                    await transaction.CommitAsync();
                    await distributedCaching.SetCachingAsync(loadbalancerPlatform, newLoadBalancerPlatform.Id.ToString());
                    await distributedCaching.RemoveAsync(cachingListKey);
                    await dbContext.Entry(newLoadBalancerPlatform).Reference(x => x.Credential).LoadAsync();
                    await dbContext.Entry(newLoadBalancerPlatform).Reference(x => x.Organization).LoadAsync();

                    return newLoadBalancerPlatform;
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
        public async Task<List<LoadBalancerPlatformDatabaseModel>> GetListAsync()
        {
            try
            {
                List<LoadBalancerPlatformDatabaseModel> loadbalancerPlatformList;
                var cachedList = await distributedCaching.GetStringAsync(cachingListKey);
                if (cachedList == null)
                {
                    loadbalancerPlatformList = await dbContext.LoadBalancerPlatforms.Include(x => x.Credential).Include(x => x.Organization)
                        .Where(x => x.OrganizationID == organizationId).ToListAsync();
                    await distributedCaching.SetCachingAsync(loadbalancerPlatformList, cachingListKey);
                }
                else
                {
                    loadbalancerPlatformList = JsonConvert.DeserializeObject<List<LoadBalancerPlatformDatabaseModel>>(cachedList) ?? new List<LoadBalancerPlatformDatabaseModel>();
                }
                return loadbalancerPlatformList;
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
        public async Task<LoadBalancerPlatformDatabaseModel> GetByIDAsync(Guid id)
        {
            try
            {
                var loadbalancerPlatform = await dbContext.LoadBalancerPlatforms.SingleAsync(x => x.Id == id && x.OrganizationID == organizationId);
                await dbContext.Entry(loadbalancerPlatform).Reference(x => x.Credential).LoadAsync();
                await dbContext.Entry(loadbalancerPlatform).Reference(x => x.Organization).LoadAsync();
                return loadbalancerPlatform;
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
        public async Task<LoadBalancerPlatformDatabaseModel> UpdateAsync(Guid id, LoadBalancerPlatformCRUDDataModel loadbalancerPlatform)
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    LoadBalancerPlatformDatabaseModel updateLoadBalancerPlatform = await dbContext.LoadBalancerPlatforms.SingleAsync(x => x.Id == id && x.OrganizationID == organizationId);
                    await dbContext.Entry(updateLoadBalancerPlatform).Reference(x => x.Credential).LoadAsync();
                    await dbContext.Entry(updateLoadBalancerPlatform).Reference(x => x.Organization).LoadAsync();
                    PropertyCopier.Populate(loadbalancerPlatform, updateLoadBalancerPlatform, new string[] { "OrganizationID" });
                    await dbContext.SaveChangesAsync();
                    KafkaMessage.SubmitKafkaMessageAync(
                        new LoadBalancerPlatformKafkaMessage()
                        {
                            Action = ActionEnum.update,
                            LoadBalancerPlatformID = id,
                            LoadBalancerPlatform = updateLoadBalancerPlatform,
                            OrganizationID = organizationId
                        },
                        logger,
                        kafkaProducer);
                    await transaction.CommitAsync();
                    await distributedCaching.SetCachingAsync(updateLoadBalancerPlatform, id.ToString());
                    await distributedCaching.RemoveAsync(cachingListKey);
                    await dbContext.Entry(updateLoadBalancerPlatform).Reference(x => x.Credential).LoadAsync();
                    await dbContext.Entry(updateLoadBalancerPlatform).Reference(x => x.Organization).LoadAsync();
                    return updateLoadBalancerPlatform;
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
                    var loadbalancerPlatformItem = await dbContext.LoadBalancerPlatforms.SingleAsync(x => x.Id == id && x.OrganizationID == organizationId);
                    dbContext.LoadBalancerPlatforms.Remove(loadbalancerPlatformItem);
                    await dbContext.SaveChangesAsync();
                    KafkaMessage.SubmitKafkaMessageAync(
                        new LoadBalancerPlatformKafkaMessage()
                        {
                            Action = ActionEnum.delete,
                            LoadBalancerPlatformID = id,
                            OrganizationID = organizationId
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
        public async Task<bool> LoadBalancerPlatformIdExistsAsync(Guid id) => await dbContext.LoadBalancerPlatforms.AnyAsync(e => e.Id == id && e.OrganizationID == organizationId);

        public async Task<bool> CredentialsIdExistsAsync(Guid id) => await dbContext.Credentials.AnyAsync(e => e.Id == id && e.OrganizationID == organizationId);
    }
}
