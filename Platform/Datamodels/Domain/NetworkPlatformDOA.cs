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
    public class NetworkPlatformDOA : IDisposable
    {
        private PrinubesPlatformDBContext dbContext;
        private ILogger logger;
        private Guid organizationId;
        private IMessageProducer kafkaProducer;
        private string cachingListKey = "networkplatformlist";
        private IDistributedCache distributedCaching;
        private IKafkaConsumerBuilder kafkaConsumerBuilder;
        private ServiceSettings serviceSettings;

        public NetworkPlatformDOA(Guid _organizationId, IServiceProvider _serviceProvider)
        {
            organizationId = _organizationId;
            dbContext = _serviceProvider.GetRequiredService<PrinubesPlatformDBContext>();
            logger = _serviceProvider.GetRequiredService<ILogger<NetworkPlatformDOA>>();
            kafkaProducer = _serviceProvider.GetRequiredService<IMessageProducer>();
            distributedCaching = _serviceProvider.GetRequiredService<IDistributedCache>();
            kafkaConsumerBuilder = _serviceProvider.GetRequiredService<IKafkaConsumerBuilder>();
            serviceSettings = _serviceProvider.GetRequiredService<ServiceSettings>();
        }
        public async Task<NetworkPlatformTestingResponseModel> TestConnectionAsync(NetworkPlatformCRUDDataModel computePlatform)
        {
            NetworkPlatformDatabaseModel platform = new NetworkPlatformDatabaseModel();
            PropertyCopier.Populate(computePlatform, platform);
            platform.CredentialID = computePlatform.CredentialID;

            var requestObject = new NetworkPlatformTestingRequestKafkaMessage()
            {
                NetworkPlatform = platform,
                Action = ActionEnum.test
            };
            KafkaHelpers.CreateTopic(requestObject.ReturnTopic, serviceSettings, logger);
            KafkaMessage.SubmitKafkaMessageAync(requestObject, logger, kafkaProducer);
            NetworkPlatformTestingResponseModel response = KafkaHelpers.ConsumeTopicAdhoc<NetworkPlatformTestingResponseModel>(requestObject.ReturnTopic, kafkaConsumerBuilder, logger);
            KafkaHelpers.DeleteTopic(requestObject.ReturnTopic, serviceSettings);

            return response;
        }
        public async Task<NetworkPlatformDatabaseModel> CreateAsync(NetworkPlatformCRUDDataModel networkPlatform)
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var newNetworkPlatform = new NetworkPlatformDatabaseModel() { OrganizationID = organizationId };
                    PropertyCopier.Populate(networkPlatform, newNetworkPlatform, new string[] { "OrganizationID" });
                    dbContext.NetworkPlatforms.Add(newNetworkPlatform);
                    await dbContext.SaveChangesAsync();
                    KafkaMessage.SubmitKafkaMessageAync(
                        new NetworkPlatformKafkaMessage()
                        {
                            Action = ActionEnum.create,
                            NetworkPlatformID = newNetworkPlatform.Id,
                            NetworkPlatform = newNetworkPlatform,
                            OrganizationID = organizationId

                        },
                        logger,
                        kafkaProducer);
                    await transaction.CommitAsync();
                    await distributedCaching.SetCachingAsync(networkPlatform, newNetworkPlatform.Id.ToString());
                    await distributedCaching.RemoveAsync(cachingListKey);
                    await dbContext.Entry(newNetworkPlatform).Reference(x => x.Credential).LoadAsync();
                    await dbContext.Entry(newNetworkPlatform).Reference(x => x.Organization).LoadAsync();

                    return newNetworkPlatform;
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
        public async Task<List<NetworkPlatformDatabaseModel>> GetListAsync()
        {
            try
            {
                List<NetworkPlatformDatabaseModel> networkPlatformList;
                var cachedList = await distributedCaching.GetStringAsync(cachingListKey);
                if (cachedList == null)
                {
                    networkPlatformList = await dbContext.NetworkPlatforms.Include(x => x.Credential).Include(x => x.Organization)
                        .Where(x => x.OrganizationID == organizationId).ToListAsync();
                    await distributedCaching.SetCachingAsync(networkPlatformList, cachingListKey);
                }
                else
                {
                    networkPlatformList = JsonConvert.DeserializeObject<List<NetworkPlatformDatabaseModel>>(cachedList) ?? new List<NetworkPlatformDatabaseModel>();
                }
                return networkPlatformList;
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
        public async Task<NetworkPlatformDatabaseModel> GetByIDAsync(Guid id)
        {
            try
            {
                var networkPlatform = await dbContext.NetworkPlatforms.SingleAsync(x => x.Id == id && x.OrganizationID == organizationId);
                await dbContext.Entry(networkPlatform).Reference(x => x.Credential).LoadAsync();
                await dbContext.Entry(networkPlatform).Reference(x => x.Organization).LoadAsync();
                return networkPlatform;
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
        public async Task<NetworkPlatformDatabaseModel> UpdateAsync(Guid id, NetworkPlatformCRUDDataModel networkPlatform)
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    NetworkPlatformDatabaseModel updateNetworkPlatform = await dbContext.NetworkPlatforms.SingleAsync(x => x.Id == id && x.OrganizationID == organizationId);
                    await dbContext.Entry(updateNetworkPlatform).Reference(x => x.Credential).LoadAsync();
                    await dbContext.Entry(updateNetworkPlatform).Reference(x => x.Organization).LoadAsync();
                    PropertyCopier.Populate(networkPlatform, updateNetworkPlatform, new string[] { "OrganizationID" });
                    updateNetworkPlatform.OrganizationID = organizationId;
                    await dbContext.SaveChangesAsync();
                    KafkaMessage.SubmitKafkaMessageAync(
                        new NetworkPlatformKafkaMessage()
                        {
                            Action = ActionEnum.update,
                            NetworkPlatformID = id,
                            NetworkPlatform = updateNetworkPlatform,
                            OrganizationID = organizationId
                        },
                        logger,
                        kafkaProducer);
                    await transaction.CommitAsync();
                    await distributedCaching.SetCachingAsync(updateNetworkPlatform, id.ToString());
                    await distributedCaching.RemoveAsync(cachingListKey);
                    await dbContext.Entry(updateNetworkPlatform).Reference(x => x.Credential).LoadAsync();
                    await dbContext.Entry(updateNetworkPlatform).Reference(x => x.Organization).LoadAsync();
                    return updateNetworkPlatform;
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
                    var networkPlatformItem = await dbContext.NetworkPlatforms.SingleAsync(x => x.Id == id && x.OrganizationID == organizationId);
                    dbContext.NetworkPlatforms.Remove(networkPlatformItem);
                    await dbContext.SaveChangesAsync();
                    KafkaMessage.SubmitKafkaMessageAync(
                        new NetworkPlatformKafkaMessage()
                        {
                            Action = ActionEnum.update,
                            NetworkPlatformID = id,
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
        public async Task<bool> NetworkPlatformIdExistsAsync(Guid id) => await dbContext.NetworkPlatforms.AnyAsync(e => e.Id == id && e.OrganizationID == organizationId);

        public async Task<bool> CredentialsIdExistsAsync(Guid id) => await dbContext.Credentials.AnyAsync(e => e.Id == id && e.OrganizationID == organizationId);
    }
}
