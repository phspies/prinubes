using Microsoft.EntityFrameworkCore;
using Prinubes.Common.DatabaseModels.EntityTypes;
using System.Reflection;

namespace Prinubes.Common.DatabaseModels.Factories
{
    public class ServiceModelBuilderFactory
    {
        static public void BuildOnModelCreating(ref ModelBuilder modelBuilder)
        {
            var assembly = Assembly.GetCallingAssembly().GetName().Name;
            modelBuilder.ApplyConfiguration(new OrganizationConfiguration(assembly));

            modelBuilder.ApplyConfiguration(new UserConfiguration(assembly));
            modelBuilder.ApplyConfiguration(new GroupConfiguration(assembly));
            modelBuilder.ApplyConfiguration(new GroupsToUsersConfiguration(assembly));
            modelBuilder.ApplyConfiguration(new RoleConfiguration(assembly));
            modelBuilder.ApplyConfiguration(new RolesToRoutePathsConfiguration(assembly));
            modelBuilder.ApplyConfiguration(new RoutePathConfiguration(assembly));

            modelBuilder.ApplyConfiguration(new ComputePlatformConfiguration(assembly));
            modelBuilder.ApplyConfiguration(new CredentialConfiguration(assembly));
            modelBuilder.ApplyConfiguration(new NetworkPlatformConfiguration(assembly));
            modelBuilder.ApplyConfiguration(new LoadBalancerPlatformConfiguration(assembly));

        }
    }
}
