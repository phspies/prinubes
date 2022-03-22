using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.DatabaseModels.Factories;
using Prinubes.Common.Datamodels;

namespace Prinubes.Identity.Datamodels
{
    public class PrinubesIdentityDBContext : DbContext, IPrinubesDBContext
    {
        public DbSet<OrganizationDatabaseModel>? Organizations { get; set; }
        public DbSet<UserDatabaseModel>? Users { get; set; }
        public DbSet<GroupDatabaseModel>? Groups { get; set; }
        public DbSet<RoleDatabaseModel>? Roles { get; set; }
        public DbSet<RoutePathDatabaseModel>? RoutePaths { get; set; }
        //public DbSet<TaggingCRUDDataModel>? Tags { get; set; }
        public DbSet<CredentialDatabaseModel>? Credentials { get; set; }


        public PrinubesIdentityDBContext(DbContextOptions<PrinubesIdentityDBContext> options) : base(options)
        {
        }
        public bool Exists()
        {
            return (Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator)?.Exists() ?? false;
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
            // Map entities to tables  
            ServiceModelBuilderFactory.BuildOnModelCreating(ref modelBuilder);
        }
    }
}