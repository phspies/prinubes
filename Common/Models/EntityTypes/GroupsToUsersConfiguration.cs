using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.DatabaseModels.ManyToMany;
using System.Reflection;

namespace Prinubes.Common.DatabaseModels.EntityTypes
{
    public class GroupsToUsersConfiguration : IEntityTypeConfiguration<GroupsToUsers>
    {
        string assembly;
        public GroupsToUsersConfiguration(string _assembly)
        {
            assembly = _assembly;
        }
        public void Configure(EntityTypeBuilder<GroupsToUsers> builder)
        {
            builder.ToTable("groups_to_users_mtm");

            builder.Property(ug => ug.UserID).HasColumnType("BINARY(16)");
            builder.Property(ug => ug.GroupID).HasColumnType("BINARY(16)");

            if (assembly == "Identity")
            {

            }
        }
    }
}
