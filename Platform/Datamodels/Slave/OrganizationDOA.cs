using Microsoft.EntityFrameworkCore;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Helpers;
using Prinubes.Platforms.Datamodels;

namespace Prinubes.Platforms.Datamodels
{
    public class OrganizationDOA : IDisposable
    {
        private PrinubesPlatformDBContext dbContext;
        private ILogger logger;

        public OrganizationDOA(IServiceProvider serviceProvider)
        {
            dbContext = serviceProvider.GetRequiredService<PrinubesPlatformDBContext>();
            logger = serviceProvider.GetRequiredService<ILogger<OrganizationDOA>>();
        }
        public async Task<Common.DatabaseModels.OrganizationDatabaseModel> CreateAsync(Common.DatabaseModels.OrganizationDatabaseModel user)
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    dbContext.Organizations.Add(user);
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return user;
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
        public async Task<List<Common.DatabaseModels.OrganizationDatabaseModel>> GetListAsync()
        {
            try
            {
                return await dbContext.Organizations.Include(x => x.Groups).ToListAsync();
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
                return await dbContext.Organizations.SingleAsync(x => x.Id == id);
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
        public async Task<Common.DatabaseModels.OrganizationDatabaseModel> UpdateAsync(System.Guid id, Common.DatabaseModels.OrganizationDatabaseModel user)
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    Common.DatabaseModels.OrganizationDatabaseModel updateOrganization = await dbContext.Organizations.SingleAsync((System.Linq.Expressions.Expression<Func<Common.DatabaseModels.OrganizationDatabaseModel, bool>>)(x => x.Id == id));
                    PropertyCopier.Populate(user, updateOrganization);
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return updateOrganization;
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
        public async Task DeleteAsync(System.Guid id)
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var userItem = await dbContext.Organizations.SingleAsync(x => x.Id == id);
                    dbContext.Organizations.Remove(userItem);
                    await dbContext.SaveChangesAsync();
                    transaction.Commit();
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
        public async Task<bool> OrganizationIdExistsAsync(System.Guid id) => await dbContext.Organizations.AnyAsync(e => e.Id == id);
    }
}
