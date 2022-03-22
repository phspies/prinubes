using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.DatabaseModels.ManyToMany;
using Prinubes.Common.Helpers;
using System.Reflection;

namespace Prinubes.Common.DatabaseModels.EntityTypes
{
    public class NetworkPlatformConfiguration : IEntityTypeConfiguration<NetworkPlatformDatabaseModel>
    {
        string assembly;
        public NetworkPlatformConfiguration(string _assembly)
        {
            assembly = _assembly;
        }
        public void Configure(EntityTypeBuilder<NetworkPlatformDatabaseModel> builder)
        {
            builder.ToTable("networkplatforms");

            builder.HasKey(ug => ug.Id).HasName("pk_networkplatforms");

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

            //builder.HasMany(p => p.Tags).WithMany(p => p.NetworkPlatforms).UsingEntity<NetworkPlatformsToTags>(
            // j => j
            //     .HasOne(pt => pt.Tag)
            //     .WithMany(t => t.NetworkPlatformTags).OnDelete(DeleteBehavior.Cascade)
            //     .HasForeignKey(pt => pt.NetworkPlatformID).OnDelete(DeleteBehavior.Cascade),
            // j => j
            //     .HasOne(pt => pt.NetworkPlatform)
            //     .WithMany(p => p.NetworkPlatformTags).OnDelete(DeleteBehavior.Cascade)
            //     .HasForeignKey(pt => pt.TagID).OnDelete(DeleteBehavior.Cascade),
            // j =>
            // {
            //     j.Property(pt => pt.LinkDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            //     j.HasKey(t => new { t.NetworkPlatformID, t.TagID });
            // }); ;

        }
    }
}
