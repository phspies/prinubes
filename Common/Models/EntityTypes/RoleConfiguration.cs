using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prinubes.Common.DatabaseModels.ManyToMany;

namespace Prinubes.Common.DatabaseModels.EntityTypes
{
    public class RoleConfiguration : IEntityTypeConfiguration<RoleDatabaseModel>
    {
        string assembly;
        public RoleConfiguration(string _assembly)
        {
            assembly = _assembly;
        }
        public void Configure(EntityTypeBuilder<RoleDatabaseModel> builder)
        {
            builder.ToTable("roles");

            builder.Property(ug => ug.Id).HasColumnType("BINARY(16)").HasDefaultValueSql("(UUID_TO_BIN(UUID()))");
            builder.Property(ug => ug.OrganizationID).HasColumnType("BINARY(16)");

            if (assembly == "Identity")
            {
                builder.Property(ug => ug.RowVersion).IsRowVersion();
                builder.Property(ug => ug.UpdateTimeStamp).ValueGeneratedOnAddOrUpdate();
                builder.Property(ug => ug.CreateTimeStamp).ValueGeneratedOnAdd();
            }
            if (assembly == "Platform")
            {
                builder.Property(ug => ug.UpdateTimeStamp).HasColumnType("timestamp(6)");
                builder.Property(ug => ug.CreateTimeStamp).HasColumnType("timestamp(6)");
            }

            builder.HasMany(p => p.RoutePaths).WithMany(p => p.Roles).UsingEntity<RolesToRoutePaths>(
                j => j
                    .HasOne(pt => pt.RoutePath)
                    .WithMany(t => t.RolesToPaths)
                    .HasForeignKey(pt => pt.RoleID),
                j => j
                    .HasOne(pt => pt.Role)
                    .WithMany(p => p.RolesToPaths)
                    .HasForeignKey(pt => pt.RoutePathID),
                j =>
                {
                    j.Property(pt => pt.LinkDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
                    j.HasKey(t => new { t.RoutePathID, t.RoleID });
                });
        }
    }
}
