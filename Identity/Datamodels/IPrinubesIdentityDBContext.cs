using Microsoft.EntityFrameworkCore;
using Prinubes.Common.DatabaseModels;

namespace Prinubes.Identity.Datamodels
{
    public interface IPrinubesIdentityDBContext
    {
        DbSet<GroupDatabaseModel> Groups { get; set; }
        DbSet<Common.DatabaseModels.OrganizationDatabaseModel> Organizations { get; set; }
        DbSet<RoleDatabaseModel> Roles { get; set; }
        DbSet<RoutePathDatabaseModel> RoutePaths { get; set; }
        DbSet<UserDatabaseModel> Users { get; set; }
    }
}