using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prinubes.Common.DatabaseModels.ManyToMany;

namespace Prinubes.Common.DatabaseModels.EntityTypes
{
    public class GroupConfiguration : IEntityTypeConfiguration<GroupDatabaseModel>
    {
        string assembly;
        public GroupConfiguration(string _assembly)
        {
            assembly = _assembly;
        }
        public void Configure(EntityTypeBuilder<GroupDatabaseModel> builder)
        {
            builder.ToTable("groups");
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

            builder.HasMany(p => p.Users).WithMany(p => p.Groups).UsingEntity<GroupsToUsers>(
            j => j
                .HasOne(pt => pt.User)
                .WithMany(t => t.UsersGroups)
                .HasForeignKey(pt => pt.GroupID),
            j => j
                .HasOne(pt => pt.Group)
                .WithMany(p => p.GroupsUsers)
                .HasForeignKey(pt => pt.UserID),
            j =>
            {
                j.Property(pt => pt.LinkDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
                j.HasKey(t => new { t.GroupID, t.UserID });
            });
        }
    }
}
