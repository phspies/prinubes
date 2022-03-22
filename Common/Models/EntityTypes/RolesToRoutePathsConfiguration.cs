using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.DatabaseModels.ManyToMany;
using System.Reflection;

namespace Prinubes.Common.DatabaseModels.EntityTypes
{
    public class RolesToRoutePathsConfiguration : IEntityTypeConfiguration<RolesToRoutePaths>
    {
        string assembly;
        public RolesToRoutePathsConfiguration(string _assembly)
        {
            assembly = _assembly;
        }
        public void Configure(EntityTypeBuilder<RolesToRoutePaths> builder)
        {
            builder.ToTable("roles_to_paths_mtm");

            builder.Property(ug => ug.RoutePathID).HasColumnType("BINARY(16)");
            builder.Property(ug => ug.RoleID).HasColumnType("BINARY(16)");


            if (assembly == "Identity")
            {

            }
            if (assembly == "Platform")
            {
            }
        }
    }
}
