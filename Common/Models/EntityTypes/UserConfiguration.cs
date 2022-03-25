using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Prinubes.Common.DatabaseModels.EntityTypes
{
    public class UserConfiguration : IEntityTypeConfiguration<UserDatabaseModel>
    {
        string assembly;
        public UserConfiguration(string _assembly)
        {
            assembly = _assembly;
        }
        public void Configure(EntityTypeBuilder<UserDatabaseModel> builder)
        {
            builder.ToTable("users");
            builder.Property(ug => ug.Id).HasColumnType("BINARY(16)").HasDefaultValueSql("(UUID_TO_BIN(UUID()))");
            builder.Property(ug => ug.EmailAddress).HasColumnType("nvarchar(50)").IsRequired();
            builder.Property(ug => ug.Firstname).HasColumnType("nvarchar(30)").IsRequired();
            builder.Property(ug => ug.Lastname).HasColumnType("nvarchar(30)").IsRequired();


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
        }
    }
}
