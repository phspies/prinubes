using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Prinubes.Common.DatabaseModels.EntityTypes
{
    public class OrganizationConfiguration : IEntityTypeConfiguration<OrganizationDatabaseModel>
    {
        string assembly;
        public OrganizationConfiguration(string _assembly)
        {
            assembly = _assembly;
        }
        public void Configure(EntityTypeBuilder<OrganizationDatabaseModel> builder)
        {
            builder.ToTable("organizations");
            builder.HasIndex(u => u.Organization).HasDatabaseName("idx_OrganizationName");

            builder.Property(ug => ug.Id).HasColumnType("BINARY(16)").HasDefaultValueSql("(UUID_TO_BIN(UUID()))");
            builder.Property(ug => ug.Organization).HasColumnType("nvarchar(30)").IsRequired();


            if (assembly == "Identity")
            {
                builder.Property(ug => ug.RowVersion).IsRowVersion();
                builder.Property(ug => ug.UpdateTimeStamp).ValueGeneratedOnAddOrUpdate();
                builder.Property(ug => ug.CreateTimeStamp).ValueGeneratedOnAdd();
            }
            if (!assembly.Contains("Identity"))
            {
                builder.Property(ug => ug.UpdateTimeStamp).HasColumnType("timestamp(6)");
                builder.Property(ug => ug.CreateTimeStamp).HasColumnType("timestamp(6)");
            }

            if (!assembly.Contains("Worker"))
            {
            }
            builder.HasMany(c => c.Groups).WithOne(e => e.Organization).HasForeignKey(x => x.OrganizationID);

            builder.HasMany(c => c.ComputePlatforms).WithOne(x => x.Organization).HasForeignKey(x => x.OrganizationID);
            builder.HasMany(c => c.NetworkPlatforms).WithOne(x => x.Organization).HasForeignKey(x => x.OrganizationID);
            builder.HasMany(c => c.LoadBalancerPlatforms).WithOne(x => x.Organization).HasForeignKey(x => x.OrganizationID);
            builder.HasMany(ug => ug.Credentials).WithOne(x => x.Organization).HasForeignKey(x => x.OrganizationID);
        }
    }
}
