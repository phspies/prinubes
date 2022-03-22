using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.DatabaseModels.ManyToMany;
using Prinubes.Common.Helpers;
using System.Reflection;

namespace Prinubes.Common.DatabaseModels.EntityTypes
{
    public class ComputePlatformConfiguration : IEntityTypeConfiguration<ComputePlatformDatabaseModel>
    {
        string assembly;
        public ComputePlatformConfiguration(string _assembly)
        {
            assembly = _assembly;
        }
        public void Configure(EntityTypeBuilder<ComputePlatformDatabaseModel> builder)
        {
            builder.ToTable("computeplatforms");

            builder.HasKey(ug => ug.Id).HasName("pk_computeplatforms");

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
        }
    }
}
