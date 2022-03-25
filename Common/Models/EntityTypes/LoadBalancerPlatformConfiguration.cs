using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prinubes.Common.Helpers;

namespace Prinubes.Common.DatabaseModels.EntityTypes
{
    public class LoadBalancerPlatformConfiguration : IEntityTypeConfiguration<LoadBalancerPlatformDatabaseModel>
    {
        string assembly;
        public LoadBalancerPlatformConfiguration(string _assembly)
        {
            assembly = _assembly;
        }
        public void Configure(EntityTypeBuilder<LoadBalancerPlatformDatabaseModel> builder)
        {
            builder.ToTable("loadbalancerplatforms");

            builder.HasKey(ug => ug.Id).HasName("pk_loadbalancerplatforms");

            builder.Property(ug => ug.Id).HasColumnType("BINARY(16)").HasDefaultValueSql("(UUID_TO_BIN(UUID()))");
            builder.Property(ug => ug.OrganizationID).HasColumnType("BINARY(16)");
            builder.Property(ug => ug.CredentialID).HasColumnType("BINARY(16)");

            if (assembly == "Identity")
            {

            }
            if (assembly == "Platform")
            {
                builder.Property(ug => ug.RowVersion).IsRowVersion();
                builder.Property(ug => ug.UpdateTimeStamp).ValueGeneratedOnAddOrUpdate();
                builder.Property(ug => ug.CreateTimeStamp).ValueGeneratedOnAdd();
            }
            builder.Property(e => e.Tags).HasJsonConversion();


            //builder.HasMany(p => p.Tags).WithMany(p => p.LoadBalancerPlatforms).UsingEntity<LoadBalancerPlatformsToTags>(
            //  j => j
            //      .HasOne(pt => pt.Tag)
            //      .WithMany(t => t.LoadBalancerPlatformTags).OnDelete(DeleteBehavior.Cascade)
            //      .HasForeignKey(pt => pt.LoadBalancerID).OnDelete(DeleteBehavior.Cascade),
            //  j => j
            //      .HasOne(pt => pt.LoadBalancerPlatform)
            //      .WithMany(p => p.LoadBalancerPlatformTags).OnDelete(DeleteBehavior.Cascade)
            //      .HasForeignKey(pt => pt.TagID).OnDelete(DeleteBehavior.Cascade),
            //  j =>
            //  {
            //      j.Property(pt => pt.LinkDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            //      j.HasKey(t => new { t.LoadBalancerID, t.TagID });
            //  });

        }
    }
}
