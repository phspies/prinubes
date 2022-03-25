using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Helpers;
using Prinubes.Common.Kafka.Producer;

namespace Prinubes.Platforms.Datamodels
{
    public class CredentialDOA : IDisposable
    {
        private PrinubesPlatformDBContext dbContext;
        private ILogger logger;
        private IMessageProducer kafkaProducer;
        private IMapper mapper;
        private string cachingListKey = "credentialslist";
        private IDistributedCache distributedCaching;
        private OrganizationDatabaseModel organizationObject;

        public CredentialDOA(System.Guid _organizationId, IServiceProvider _serviceProvider)
        {
            dbContext = _serviceProvider.GetRequiredService<PrinubesPlatformDBContext>();
            logger = _serviceProvider.GetRequiredService<ILogger<CredentialDOA>>();
            kafkaProducer = _serviceProvider.GetRequiredService<IMessageProducer>();
            mapper = _serviceProvider.GetRequiredService<IMapper>();
            distributedCaching = _serviceProvider.GetRequiredService<IDistributedCache>();
            organizationObject = dbContext.Organizations.Single(x => x.Id == _organizationId);
        }
        public async Task<CredentialDatabaseModel> CreateAsync(CredentialDatabaseModel credential)
        {
            try
            {
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    dbContext.Credentials.Add(credential);
                    await transaction.CommitAsync();
                }
                await distributedCaching.SetCachingAsync(credential, credential.Id.ToString());
                await distributedCaching.RemoveAsync(cachingListKey);
                return credential;

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

        public async Task<List<CredentialDatabaseModel>> GetListAsync()
        {
            try
            {
                List<CredentialDatabaseModel> credentialList;
                var cachedList = await distributedCaching.GetStringAsync(cachingListKey);
                if (cachedList == null)
                {
                    credentialList = await dbContext.Credentials.Include(x => x.Organization).Where(x => x.OrganizationID == organizationObject.Id).ToListAsync();
                    await distributedCaching.SetCachingAsync(credentialList, cachingListKey);
                }
                else
                {
                    credentialList = JsonConvert.DeserializeObject<List<CredentialDatabaseModel>>(cachedList) ?? new List<CredentialDatabaseModel>();
                }
                return credentialList;
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
        public async Task<CredentialDatabaseModel> GetByID(System.Guid id)
        {
            try
            {
                CredentialDatabaseModel credential;
                var cachedCredential = await distributedCaching.GetStringAsync(id.ToString());
                if (cachedCredential == null)
                {
                    credential = await dbContext.Credentials.Include(b => b.Organization).SingleAsync(x => x.Id == id && x.OrganizationID == organizationObject.Id);
                    await distributedCaching.SetCachingAsync(credential, id.ToString());
                }
                else
                {
                    credential = JsonConvert.DeserializeObject<CredentialDatabaseModel>(cachedCredential) ?? new CredentialDatabaseModel();
                }
                return credential;
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
        public async Task<CredentialDatabaseModel> UpdateAsync(System.Guid id, CredentialCRUDDataModel credential)
        {
            try
            {
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    var updatedCredential = await dbContext.Credentials.SingleAsync(x => x.Id == id && x.OrganizationID == organizationObject.Id);
                    PropertyCopier.Populate(credential, updatedCredential);
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    await distributedCaching.SetCachingAsync(updatedCredential, id.ToString());
                    await distributedCaching.RemoveAsync(cachingListKey);
                    return updatedCredential;
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
                    var deleteCredential = dbContext.Credentials.Single(x => x.Id == id && x.OrganizationID == organizationObject.Id);
                    dbContext.Credentials.Remove(deleteCredential);
                    await dbContext.SaveChangesAsync();
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
            //    ((IDisposable)dbContext).Dispose();
        }

        public async Task<bool> CredentialExistsAsync(System.Guid id) => await dbContext.Credentials.AnyAsync(e => e.Id == id && e.OrganizationID == organizationObject.Id);

    }
}
