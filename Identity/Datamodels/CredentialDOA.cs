using AutoMapper;
using Prinubes.Common.Kafka;
using Prinubes.Common.Kafka.Producer;
using Microsoft.EntityFrameworkCore;
using Prinubes.Common.DatabaseModels;
using Prinubes.Identity.Helpers;
using Prinubes.Common.Helpers;
using Prinubes.Common.Models;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Prinubes.Identity.Datamodels
{
    public class CredentialDOA : IDisposable
    {
        private PrinubesIdentityDBContext dbContext;
        private ILogger logger;
        private IMessageProducer kafkaProducer;
        private IMapper mapper;
        private string cachingListKey = "credentialslist";
        private IDistributedCache distributedCaching;
        private Guid organizationId;

        public CredentialDOA(Guid _organizationId, IServiceProvider _serviceProvider)
        {
            dbContext = _serviceProvider.GetRequiredService<PrinubesIdentityDBContext>();
            logger = _serviceProvider.GetRequiredService<ILogger<UserDOA>>();
            kafkaProducer = _serviceProvider.GetRequiredService<IMessageProducer>();
            mapper = _serviceProvider.GetRequiredService<IMapper>();
            distributedCaching = _serviceProvider.GetRequiredService<IDistributedCache>();
            organizationId = _organizationId;
        }
        public async Task<CredentialDatabaseModel> CreateAsync(CredentialCRUDDataModel credential)
        {
            try
            {
                CredentialDatabaseModel updatedCredential = mapper.Map<CredentialCRUDDataModel, CredentialDatabaseModel>(credential);
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    string key;
                    updatedCredential.EncryptedPassword = CipherService.EncryptString(credential.Password, out key);
                    updatedCredential.EncryptedKey = key;
                    updatedCredential.OrganizationID = organizationId;
                    dbContext.Credentials.Add(updatedCredential);
                    await dbContext.SaveChangesAsync();
                    KafkaMessage.SubmitKafkaMessageAync(
                        new CredentialKafkaMessage()
                        {
                            Action = ActionEnum.create,
                            CredentialID = updatedCredential.Id,
                            Credential = mapper.Map<CredentialKafkaDataModel>(updatedCredential),
                            OrganizationID = organizationId

                        },
                        logger,
                        kafkaProducer); 
                    await transaction.CommitAsync();
                }
                await distributedCaching.SetCachingAsync(updatedCredential, updatedCredential.Id.ToString());
                await distributedCaching.RemoveAsync(cachingListKey);
                return updatedCredential;

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
                    credentialList = await dbContext.Credentials.Include(x => x.Organization).Where(x => x.OrganizationID == organizationId).ToListAsync();
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
        public async Task<CredentialDatabaseModel> GetByID(Guid id)
        {
            try
            {
                CredentialDatabaseModel credential;
                var cachedCredential = await distributedCaching.GetStringAsync(id.ToString());
                if (cachedCredential == null)
                {
                    credential = await dbContext.Credentials.Include(b => b.Organization).SingleAsync(x => x.Id == id && x.OrganizationID == organizationId);
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
                    var updatedCredential = await dbContext.Credentials.Include(x => x.Organization).SingleAsync(x => x.Id == id && x.OrganizationID == organizationId);
                    PropertyCopier.Populate(credential, updatedCredential);

                    if (credential.Password != null)
                    {
                        string key;
                        updatedCredential.EncryptedPassword = CipherService.EncryptString(credential.Password, out key);
                        updatedCredential.EncryptedKey = key;
                    }
                    updatedCredential.OrganizationID = organizationId;
                    await dbContext.SaveChangesAsync();
                    KafkaMessage.SubmitKafkaMessageAync(
                        new CredentialKafkaMessage()
                        {
                            Action = ActionEnum.update,
                            CredentialID = id,
                            Credential = mapper.Map<CredentialKafkaDataModel>(updatedCredential),
                            OrganizationID = organizationId,
                            RowVersion = credential.RowVersion,
                        },
                        logger,
                        kafkaProducer);
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
                    var tmpcredential = dbContext.Credentials.Single(x => x.Id == id && x.OrganizationID == organizationId);
                    dbContext.Credentials.Remove(tmpcredential);
                    await dbContext.SaveChangesAsync();
                    KafkaMessage.SubmitKafkaMessageAync(
                        new CredentialKafkaMessage()
                        {
                            Action = ActionEnum.delete,
                            CredentialID = id,
                            OrganizationID = organizationId,
                            RowVersion = tmpcredential.RowVersion
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
           // ((IDisposable)dbContext).Dispose();
        }

        public async Task<bool> CredentialExistsAsync(System.Guid id) => await dbContext.Credentials.AnyAsync(e => e.Id == id && e.OrganizationID == organizationId);

    }
}
