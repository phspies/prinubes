using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Prinubes.Common.DatabaseModels.EntityTypes
{
    public class CredentialConfiguration : IEntityTypeConfiguration<CredentialDatabaseModel>
    {
        string assembly;
        public CredentialConfiguration(string _assembly)
        {
            assembly = _assembly;
        }
        public void Configure(EntityTypeBuilder<CredentialDatabaseModel> builder)
        {
            builder.ToTable("credentials");

            builder.HasKey(ug => ug.Id).HasName("pk_credentials");

            builder.Property(ug => ug.Id).HasColumnType("BINARY(16)").HasDefaultValueSql("(UUID_TO_BIN(UUID()))");
            builder.Property(ug => ug.OrganizationID).HasColumnType("BINARY(16)");

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

            builder.HasMany(c => c.ComputePlatforms).WithOne(c => c.Credential).HasForeignKey(x => x.CredentialID);
            builder.HasMany(c => c.NetworkPlatforms).WithOne(c => c.Credential).HasForeignKey(x => x.CredentialID);
            builder.HasMany(c => c.LoadBalancerPlatforms).WithOne(c => c.Credential).HasForeignKey(x => x.CredentialID);
        }
    }
}
