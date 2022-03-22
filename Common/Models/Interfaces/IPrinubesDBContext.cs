using Microsoft.EntityFrameworkCore;
using Prinubes.Common.DatabaseModels;

namespace Prinubes.Common.Datamodels
{
    public interface IPrinubesDBContext
    {
        DbSet<DatabaseModels.OrganizationDatabaseModel> Organizations { get; set; }
        DbSet<CredentialDatabaseModel> Credentials { get; set; }
        DbSet<UserDatabaseModel> Users { get; set; }
        DbSet<RoutePathDatabaseModel> RoutePaths { get; set; }
        public void MigrateIfRequired();
    }
}