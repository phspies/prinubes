using Microsoft.EntityFrameworkCore;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Helpers;

namespace Prinubes.Platforms.Datamodels
{
    public class UserDOA : IDisposable
    {
        private PrinubesPlatformDBContext dbContext;
        private ILogger logger;

        public UserDOA(IServiceProvider serviceProvider)
        {
            dbContext = serviceProvider.GetRequiredService<PrinubesPlatformDBContext>();
            logger = serviceProvider.GetRequiredService<ILogger<UserDOA>>();
        }
        public async Task<UserDatabaseModel> CreateAsync(UserDatabaseModel user)
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    dbContext.Users.Add(user);
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
        public async Task<List<UserDatabaseModel>> GetListAsync()
        {
            try
            {
                return await dbContext.Users.Include(x => x.Groups).ToListAsync();
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
                return await dbContext.Users.SingleAsync(x => x.Id == id);
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
                return await dbContext.Users.SingleAsync(x => x.EmailAddress == emailAddress);
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
        public async Task<UserDatabaseModel> UpdateAsync(Guid id, UserDatabaseModel user)
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    UserDatabaseModel updateUser = await dbContext.Users.SingleAsync(x => x.Id == id);
                    PropertyCopier.Populate(user, updateUser);
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
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
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var userItem = await dbContext.Users.SingleAsync(x => x.Id == id);
                    dbContext.Users.Remove(userItem);
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
        public async Task<bool> UserIdExistsAsync(Guid id) => await dbContext.Users.AnyAsync(e => e.Id == id);
        public async Task<bool> UserEmailAddressExistsAsync(string emailAddress) => await dbContext.Users.AnyAsync(e => e.EmailAddress == emailAddress);


    }
}
