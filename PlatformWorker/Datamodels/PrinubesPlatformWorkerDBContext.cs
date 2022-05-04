using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.DatabaseModels.Factories;
using Prinubes.Common.Datamodels;

namespace Prinubes.PlatformWorker.Datamodels
{
    public class PrinubesPlatformWorkerDBContext : DbContext, IPrinubesDBContext
    {
        public DbSet<Common.DatabaseModels.OrganizationDatabaseModel> Organizations { get; set; }
        public DbSet<UserDatabaseModel> Users { get; set; }
        public DbSet<GroupDatabaseModel> Groups { get; set; }
        public DbSet<RoleDatabaseModel> Roles { get; set; }
        public DbSet<RoutePathDatabaseModel> RoutePaths { get; set; }
        public DbSet<CredentialDatabaseModel> Credentials { get; set; }
        public DbSet<ComputePlatformDatabaseModel> ComputePlatforms { get; set; }
        public DbSet<NetworkPlatformDatabaseModel> NetworkPlatforms { get; set; }
        public DbSet<LoadBalancerPlatformDatabaseModel> LoadBalancerPlatforms { get; set; }


        public PrinubesPlatformWorkerDBContext(DbContextOptions<PrinubesPlatformWorkerDBContext> options) : base(options)
        {
        }
        public bool Exists()
        {
            return (this.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists();
        }
        public void MigrateIfRequired()
        {
            if (Database.GetPendingMigrations().Count() > 0)
            {
                Database.MigrateAsync();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ServiceModelBuilderFactory.BuildOnModelCreating(ref modelBuilder);
        }
    }
}