﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Prinubes.Platforms.Datamodels;

#nullable disable

namespace Prinubes.Platform.Migrations
{
    [DbContext(typeof(PrinubesPlatformDBContext))]
    [Migration("20220518160032_platformchanges")]
    partial class platformchanges
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.ComputePlatformDatabaseModel", b =>
                {
                    b.Property<byte[]>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("BINARY(16)")
                        .HasDefaultValueSql("(UUID_TO_BIN(UUID()))");

                    b.Property<int>("AvailabilityZone")
                        .HasColumnType("int");

                    b.Property<string>("ClustersMoid")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("CreateTimeStamp")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)");

                    b.Property<byte[]>("CredentialID")
                        .IsRequired()
                        .HasColumnType("BINARY(16)");

                    b.Property<string>("DatacenterMoid")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<bool?>("Enabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("FolderMoid")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<byte[]>("OrganizationID")
                        .IsRequired()
                        .HasColumnType("BINARY(16)");

                    b.Property<string>("Platform")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("PlatformType")
                        .HasColumnType("int");

                    b.Property<string>("ResourcepoolMoid")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime?>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp(6)");

                    b.Property<string>("Tags")
                        .HasColumnType("JSON");

                    b.Property<DateTime>("UpdateTimeStamp")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime(6)");

                    b.Property<string>("UrlEndpoint")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<bool?>("VertifySSLCert")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("message")
                        .HasColumnType("longtext");

                    b.Property<int?>("state")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .HasName("pk_computeplatforms");

                    b.HasIndex("CredentialID");

                    b.HasIndex("OrganizationID");

                    b.ToTable("computeplatforms", (string)null);
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.CredentialDatabaseModel", b =>
                {
                    b.Property<byte[]>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("BINARY(16)")
                        .HasDefaultValueSql("(UUID_TO_BIN(UUID()))");

                    b.Property<DateTime>("CreateTimeStamp")
                        .HasColumnType("timestamp(6)");

                    b.Property<string>("Credential")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("EncryptedKey")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("EncryptedPassword")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<byte[]>("OrganizationID")
                        .IsRequired()
                        .HasColumnType("BINARY(16)");

                    b.Property<byte[]>("RowVersion")
                        .HasColumnType("longblob");

                    b.Property<DateTime>("UpdateTimeStamp")
                        .HasColumnType("timestamp(6)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id")
                        .HasName("pk_credentials");

                    b.HasIndex("OrganizationID");

                    b.ToTable("credentials", (string)null);
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.GroupDatabaseModel", b =>
                {
                    b.Property<byte[]>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("BINARY(16)")
                        .HasDefaultValueSql("(UUID_TO_BIN(UUID()))");

                    b.Property<DateTime>("CreateTimeStamp")
                        .HasColumnType("timestamp(6)");

                    b.Property<string>("Group")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<byte[]>("OrganizationID")
                        .IsRequired()
                        .HasColumnType("BINARY(16)");

                    b.Property<byte[]>("RowVersion")
                        .HasColumnType("longblob");

                    b.Property<DateTime>("UpdateTimeStamp")
                        .HasColumnType("timestamp(6)");

                    b.HasKey("Id");

                    b.HasIndex("OrganizationID");

                    b.ToTable("groups", (string)null);
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.LoadBalancerPlatformDatabaseModel", b =>
                {
                    b.Property<byte[]>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("BINARY(16)")
                        .HasDefaultValueSql("(UUID_TO_BIN(UUID()))");

                    b.Property<int>("AvailabilityZone")
                        .HasColumnType("int");

                    b.Property<string>("CloudPlatformMoid")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("CreateTimeStamp")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)");

                    b.Property<byte[]>("CredentialID")
                        .IsRequired()
                        .HasColumnType("BINARY(16)");

                    b.Property<bool?>("Enabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<byte[]>("OrganizationID")
                        .IsRequired()
                        .HasColumnType("BINARY(16)");

                    b.Property<string>("Platform")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("PlatformType")
                        .HasColumnType("int");

                    b.Property<DateTime?>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp(6)");

                    b.Property<string>("Tags")
                        .HasColumnType("JSON");

                    b.Property<DateTime>("UpdateTimeStamp")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime(6)");

                    b.Property<string>("UrlEndpoint")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("VertifySSLCert")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("message")
                        .HasColumnType("longtext");

                    b.Property<int?>("state")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .HasName("pk_loadbalancerplatforms");

                    b.HasIndex("CredentialID");

                    b.HasIndex("OrganizationID");

                    b.ToTable("loadbalancerplatforms", (string)null);
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.ManyToMany.GroupsToUsers", b =>
                {
                    b.Property<byte[]>("GroupID")
                        .HasColumnType("BINARY(16)");

                    b.Property<byte[]>("UserID")
                        .HasColumnType("BINARY(16)");

                    b.Property<DateTime>("LinkDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                    b.HasKey("GroupID", "UserID");

                    b.HasIndex("UserID");

                    b.ToTable("groups_to_users_mtm", (string)null);
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.ManyToMany.RolesToRoutePaths", b =>
                {
                    b.Property<byte[]>("RoutePathID")
                        .HasColumnType("BINARY(16)");

                    b.Property<byte[]>("RoleID")
                        .HasColumnType("BINARY(16)");

                    b.Property<DateTime>("LinkDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                    b.HasKey("RoutePathID", "RoleID");

                    b.HasIndex("RoleID");

                    b.ToTable("roles_to_paths_mtm", (string)null);
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.NetworkPlatformDatabaseModel", b =>
                {
                    b.Property<byte[]>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("BINARY(16)")
                        .HasDefaultValueSql("(UUID_TO_BIN(UUID()))");

                    b.Property<int>("AvailabilityZone")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreateTimeStamp")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)");

                    b.Property<byte[]>("CredentialID")
                        .IsRequired()
                        .HasColumnType("BINARY(16)");

                    b.Property<bool?>("Enabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<byte[]>("OrganizationID")
                        .IsRequired()
                        .HasColumnType("BINARY(16)");

                    b.Property<string>("Platform")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("PlatformType")
                        .HasColumnType("int");

                    b.Property<DateTime?>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp(6)");

                    b.Property<string>("Tags")
                        .HasColumnType("JSON");

                    b.Property<DateTime>("UpdateTimeStamp")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime(6)");

                    b.Property<string>("UrlEndpoint")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("VertifySSLCert")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("message")
                        .HasColumnType("longtext");

                    b.Property<int?>("state")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .HasName("pk_networkplatforms");

                    b.HasIndex("CredentialID");

                    b.HasIndex("OrganizationID");

                    b.ToTable("networkplatforms", (string)null);
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.OrganizationDatabaseModel", b =>
                {
                    b.Property<byte[]>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("BINARY(16)")
                        .HasDefaultValueSql("(UUID_TO_BIN(UUID()))");

                    b.Property<DateTime>("CreateTimeStamp")
                        .HasColumnType("timestamp(6)");

                    b.Property<string>("Organization")
                        .IsRequired()
                        .HasColumnType("nvarchar(30)");

                    b.Property<byte[]>("RowVersion")
                        .HasColumnType("longblob");

                    b.Property<DateTime>("UpdateTimeStamp")
                        .HasColumnType("timestamp(6)");

                    b.HasKey("Id");

                    b.HasIndex("Organization")
                        .HasDatabaseName("idx_OrganizationName");

                    b.ToTable("organizations", (string)null);
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.RoleDatabaseModel", b =>
                {
                    b.Property<byte[]>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("BINARY(16)")
                        .HasDefaultValueSql("(UUID_TO_BIN(UUID()))");

                    b.Property<DateTime>("CreateTimeStamp")
                        .HasColumnType("timestamp(6)");

                    b.Property<byte[]>("OrganizationID")
                        .IsRequired()
                        .HasColumnType("BINARY(16)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<byte[]>("RowVersion")
                        .HasColumnType("longblob");

                    b.Property<DateTime>("UpdateTimeStamp")
                        .HasColumnType("timestamp(6)");

                    b.HasKey("Id");

                    b.ToTable("roles", (string)null);
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.RoutePathDatabaseModel", b =>
                {
                    b.Property<byte[]>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("BINARY(16)")
                        .HasDefaultValueSql("(UUID_TO_BIN(UUID()))");

                    b.Property<string>("Controller")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("FriendlyName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("MethodName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("MicroService")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("RoutePathUnique")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("RouteTemplate")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("RouteTemplate")
                        .HasDatabaseName("idx_RouteTemplate");

                    b.ToTable("routepaths", (string)null);
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.UserDatabaseModel", b =>
                {
                    b.Property<byte[]>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("BINARY(16)")
                        .HasDefaultValueSql("(UUID_TO_BIN(UUID()))");

                    b.Property<DateTime>("CreateTimeStamp")
                        .HasColumnType("timestamp(6)");

                    b.Property<string>("EmailAddress")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Firstname")
                        .IsRequired()
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("Lastname")
                        .IsRequired()
                        .HasColumnType("nvarchar(30)");

                    b.Property<byte[]>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("longblob");

                    b.Property<byte[]>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("longblob");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.Property<byte[]>("RowVersion")
                        .HasColumnType("longblob");

                    b.Property<DateTime>("UpdateTimeStamp")
                        .HasColumnType("timestamp(6)");

                    b.HasKey("Id");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.ComputePlatformDatabaseModel", b =>
                {
                    b.HasOne("Prinubes.Common.DatabaseModels.CredentialDatabaseModel", "Credential")
                        .WithMany("ComputePlatforms")
                        .HasForeignKey("CredentialID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Prinubes.Common.DatabaseModels.OrganizationDatabaseModel", "Organization")
                        .WithMany("ComputePlatforms")
                        .HasForeignKey("OrganizationID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Credential");

                    b.Navigation("Organization");
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.CredentialDatabaseModel", b =>
                {
                    b.HasOne("Prinubes.Common.DatabaseModels.OrganizationDatabaseModel", "Organization")
                        .WithMany("Credentials")
                        .HasForeignKey("OrganizationID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Organization");
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.GroupDatabaseModel", b =>
                {
                    b.HasOne("Prinubes.Common.DatabaseModels.OrganizationDatabaseModel", "Organization")
                        .WithMany("Groups")
                        .HasForeignKey("OrganizationID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Organization");
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.LoadBalancerPlatformDatabaseModel", b =>
                {
                    b.HasOne("Prinubes.Common.DatabaseModels.CredentialDatabaseModel", "Credential")
                        .WithMany("LoadBalancerPlatforms")
                        .HasForeignKey("CredentialID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Prinubes.Common.DatabaseModels.OrganizationDatabaseModel", "Organization")
                        .WithMany("LoadBalancerPlatforms")
                        .HasForeignKey("OrganizationID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Credential");

                    b.Navigation("Organization");
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.ManyToMany.GroupsToUsers", b =>
                {
                    b.HasOne("Prinubes.Common.DatabaseModels.UserDatabaseModel", "User")
                        .WithMany("UsersGroups")
                        .HasForeignKey("GroupID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Prinubes.Common.DatabaseModels.GroupDatabaseModel", "Group")
                        .WithMany("GroupsUsers")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.ManyToMany.RolesToRoutePaths", b =>
                {
                    b.HasOne("Prinubes.Common.DatabaseModels.RoutePathDatabaseModel", "RoutePath")
                        .WithMany("RolesToPaths")
                        .HasForeignKey("RoleID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Prinubes.Common.DatabaseModels.RoleDatabaseModel", "Role")
                        .WithMany("RolesToPaths")
                        .HasForeignKey("RoutePathID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("RoutePath");
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.NetworkPlatformDatabaseModel", b =>
                {
                    b.HasOne("Prinubes.Common.DatabaseModels.CredentialDatabaseModel", "Credential")
                        .WithMany("NetworkPlatforms")
                        .HasForeignKey("CredentialID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Prinubes.Common.DatabaseModels.OrganizationDatabaseModel", "Organization")
                        .WithMany("NetworkPlatforms")
                        .HasForeignKey("OrganizationID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Credential");

                    b.Navigation("Organization");
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.CredentialDatabaseModel", b =>
                {
                    b.Navigation("ComputePlatforms");

                    b.Navigation("LoadBalancerPlatforms");

                    b.Navigation("NetworkPlatforms");
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.GroupDatabaseModel", b =>
                {
                    b.Navigation("GroupsUsers");
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.OrganizationDatabaseModel", b =>
                {
                    b.Navigation("ComputePlatforms");

                    b.Navigation("Credentials");

                    b.Navigation("Groups");

                    b.Navigation("LoadBalancerPlatforms");

                    b.Navigation("NetworkPlatforms");
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.RoleDatabaseModel", b =>
                {
                    b.Navigation("RolesToPaths");
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.RoutePathDatabaseModel", b =>
                {
                    b.Navigation("RolesToPaths");
                });

            modelBuilder.Entity("Prinubes.Common.DatabaseModels.UserDatabaseModel", b =>
                {
                    b.Navigation("UsersGroups");
                });
#pragma warning restore 612, 618
        }
    }
}
