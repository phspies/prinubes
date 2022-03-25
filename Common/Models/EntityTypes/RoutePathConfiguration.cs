using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Prinubes.Common.DatabaseModels.EntityTypes
{
    public class RoutePathConfiguration : IEntityTypeConfiguration<RoutePathDatabaseModel>
    {
        string assembly;
        public RoutePathConfiguration(string _assembly)
        {
            assembly = _assembly;
        }
        public void Configure(EntityTypeBuilder<RoutePathDatabaseModel> builder)
        {
            builder.ToTable("routepaths");

            builder.HasIndex(u => u.RouteTemplate).HasDatabaseName("idx_RouteTemplate");

            builder.Property(ug => ug.Id).HasColumnType("BINARY(16)").HasDefaultValueSql("(UUID_TO_BIN(UUID()))");

            if (assembly == "Identity")
            {

            }
            if (assembly == "Platform")
            {

            }
        }
    }
}
