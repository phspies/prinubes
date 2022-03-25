using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prinubes.Identity.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "organizations",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "BINARY(16)", nullable: false, defaultValueSql: "(UUID_TO_BIN(UUID()))"),
                    Organization = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    CreateTimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdateTimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "BINARY(16)", nullable: false, defaultValueSql: "(UUID_TO_BIN(UUID()))"),
                    RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    CreateTimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdateTimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    Role = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationID = table.Column<byte[]>(type: "BINARY(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "routepaths",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "BINARY(16)", nullable: false, defaultValueSql: "(UUID_TO_BIN(UUID()))"),
                    MicroService = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Controller = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MethodName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FriendlyName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RouteTemplate = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoutePathUnique = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routepaths", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "BINARY(16)", nullable: false, defaultValueSql: "(UUID_TO_BIN(UUID()))"),
                    PasswordHash = table.Column<byte[]>(type: "longblob", nullable: false),
                    PasswordSalt = table.Column<byte[]>(type: "longblob", nullable: false),
                    RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    CreateTimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdateTimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    EmailAddress = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Firstname = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    Lastname = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "credentials",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "BINARY(16)", nullable: false, defaultValueSql: "(UUID_TO_BIN(UUID()))"),
                    Credential = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationID = table.Column<byte[]>(type: "BINARY(16)", nullable: false),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EncryptedPassword = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EncryptedKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    CreateTimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdateTimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_credentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_credentials_organizations_OrganizationID",
                        column: x => x.OrganizationID,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "groups",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "BINARY(16)", nullable: false, defaultValueSql: "(UUID_TO_BIN(UUID()))"),
                    Group = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationID = table.Column<byte[]>(type: "BINARY(16)", nullable: false),
                    RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    CreateTimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdateTimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_groups_organizations_OrganizationID",
                        column: x => x.OrganizationID,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "roles_to_paths_mtm",
                columns: table => new
                {
                    RoleID = table.Column<byte[]>(type: "BINARY(16)", nullable: false),
                    RoutePathID = table.Column<byte[]>(type: "BINARY(16)", nullable: false),
                    LinkDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles_to_paths_mtm", x => new { x.RoutePathID, x.RoleID });
                    table.ForeignKey(
                        name: "FK_roles_to_paths_mtm_roles_RoutePathID",
                        column: x => x.RoutePathID,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_roles_to_paths_mtm_routepaths_RoleID",
                        column: x => x.RoleID,
                        principalTable: "routepaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "computeplatforms",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "BINARY(16)", nullable: false, defaultValueSql: "(UUID_TO_BIN(UUID()))"),
                    RowVersion = table.Column<byte[]>(type: "longblob", nullable: true),
                    CreateTimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdateTimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Platform = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationID = table.Column<byte[]>(type: "BINARY(16)", nullable: false),
                    PlatformType = table.Column<int>(type: "int", nullable: true),
                    CredentialID = table.Column<byte[]>(type: "BINARY(16)", nullable: true),
                    Location = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UrlEndpoint = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VertifySSLCert = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    CredentialsId = table.Column<byte[]>(type: "BINARY(16)", nullable: true),
                    DatacenterMoid = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClustersMoid = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FolderMoid = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResourcepoolMoid = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_computeplatforms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_computeplatforms_credentials_CredentialID",
                        column: x => x.CredentialID,
                        principalTable: "credentials",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_computeplatforms_credentials_CredentialsId",
                        column: x => x.CredentialsId,
                        principalTable: "credentials",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_computeplatforms_organizations_OrganizationID",
                        column: x => x.OrganizationID,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "loadbalancerplatforms",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "BINARY(16)", nullable: false, defaultValueSql: "(UUID_TO_BIN(UUID()))"),
                    RowVersion = table.Column<byte[]>(type: "longblob", nullable: true),
                    CreateTimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdateTimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Platform = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationID = table.Column<byte[]>(type: "BINARY(16)", nullable: false),
                    PlatformType = table.Column<int>(type: "int", nullable: false),
                    CredentialID = table.Column<byte[]>(type: "BINARY(16)", nullable: false),
                    Location = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UrlEndpoint = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VertifySSLCert = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CloudPlatformMoid = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_loadbalancerplatforms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_loadbalancerplatforms_credentials_CredentialID",
                        column: x => x.CredentialID,
                        principalTable: "credentials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_loadbalancerplatforms_organizations_OrganizationID",
                        column: x => x.OrganizationID,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "networkplatforms",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "BINARY(16)", nullable: false, defaultValueSql: "(UUID_TO_BIN(UUID()))"),
                    RowVersion = table.Column<byte[]>(type: "longblob", nullable: true),
                    CreateTimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdateTimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Platform = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationID = table.Column<byte[]>(type: "BINARY(16)", nullable: false),
                    PlatformType = table.Column<int>(type: "int", nullable: false),
                    CredentialID = table.Column<byte[]>(type: "BINARY(16)", nullable: false),
                    CredentialsId = table.Column<byte[]>(type: "BINARY(16)", nullable: false),
                    Location = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UrlEndpoint = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VertifySSLCert = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_networkplatforms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_networkplatforms_credentials_CredentialID",
                        column: x => x.CredentialID,
                        principalTable: "credentials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_networkplatforms_credentials_CredentialsId",
                        column: x => x.CredentialsId,
                        principalTable: "credentials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_networkplatforms_organizations_OrganizationID",
                        column: x => x.OrganizationID,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "groups_to_users_mtm",
                columns: table => new
                {
                    GroupID = table.Column<byte[]>(type: "BINARY(16)", nullable: false),
                    UserID = table.Column<byte[]>(type: "BINARY(16)", nullable: false),
                    LinkDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_groups_to_users_mtm", x => new { x.GroupID, x.UserID });
                    table.ForeignKey(
                        name: "FK_groups_to_users_mtm_groups_UserID",
                        column: x => x.UserID,
                        principalTable: "groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_groups_to_users_mtm_users_GroupID",
                        column: x => x.GroupID,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "BINARY(16)", nullable: false, defaultValueSql: "(UUID_TO_BIN(UUID()))"),
                    OrganizationID = table.Column<byte[]>(type: "BINARY(16)", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    ComputePlatformDatabaseModelId = table.Column<byte[]>(type: "BINARY(16)", nullable: true),
                    LoadBalancerPlatformDatabaseModelId = table.Column<byte[]>(type: "BINARY(16)", nullable: true),
                    NetworkPlatformDatabaseModelId = table.Column<byte[]>(type: "BINARY(16)", nullable: true),
                    RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    CreateTimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdateTimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tags_computeplatforms_ComputePlatformDatabaseModelId",
                        column: x => x.ComputePlatformDatabaseModelId,
                        principalTable: "computeplatforms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tags_loadbalancerplatforms_LoadBalancerPlatformDatabaseModel~",
                        column: x => x.LoadBalancerPlatformDatabaseModelId,
                        principalTable: "loadbalancerplatforms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tags_networkplatforms_NetworkPlatformDatabaseModelId",
                        column: x => x.NetworkPlatformDatabaseModelId,
                        principalTable: "networkplatforms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tags_organizations_OrganizationID",
                        column: x => x.OrganizationID,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_computeplatforms_CredentialID",
                table: "computeplatforms",
                column: "CredentialID");

            migrationBuilder.CreateIndex(
                name: "IX_computeplatforms_CredentialsId",
                table: "computeplatforms",
                column: "CredentialsId");

            migrationBuilder.CreateIndex(
                name: "IX_computeplatforms_OrganizationID",
                table: "computeplatforms",
                column: "OrganizationID");

            migrationBuilder.CreateIndex(
                name: "IX_credentials_OrganizationID",
                table: "credentials",
                column: "OrganizationID");

            migrationBuilder.CreateIndex(
                name: "IX_groups_OrganizationID",
                table: "groups",
                column: "OrganizationID");

            migrationBuilder.CreateIndex(
                name: "IX_groups_to_users_mtm_UserID",
                table: "groups_to_users_mtm",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_loadbalancerplatforms_CredentialID",
                table: "loadbalancerplatforms",
                column: "CredentialID");

            migrationBuilder.CreateIndex(
                name: "IX_loadbalancerplatforms_OrganizationID",
                table: "loadbalancerplatforms",
                column: "OrganizationID");

            migrationBuilder.CreateIndex(
                name: "IX_networkplatforms_CredentialID",
                table: "networkplatforms",
                column: "CredentialID");

            migrationBuilder.CreateIndex(
                name: "IX_networkplatforms_CredentialsId",
                table: "networkplatforms",
                column: "CredentialsId");

            migrationBuilder.CreateIndex(
                name: "IX_networkplatforms_OrganizationID",
                table: "networkplatforms",
                column: "OrganizationID");

            migrationBuilder.CreateIndex(
                name: "idx_OrganizationName",
                table: "organizations",
                column: "Organization");

            migrationBuilder.CreateIndex(
                name: "IX_roles_to_paths_mtm_RoleID",
                table: "roles_to_paths_mtm",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "idx_RouteTemplate",
                table: "routepaths",
                column: "RouteTemplate");

            migrationBuilder.CreateIndex(
                name: "IX_tags_ComputePlatformDatabaseModelId",
                table: "tags",
                column: "ComputePlatformDatabaseModelId");

            migrationBuilder.CreateIndex(
                name: "IX_tags_LoadBalancerPlatformDatabaseModelId",
                table: "tags",
                column: "LoadBalancerPlatformDatabaseModelId");

            migrationBuilder.CreateIndex(
                name: "IX_tags_NetworkPlatformDatabaseModelId",
                table: "tags",
                column: "NetworkPlatformDatabaseModelId");

            migrationBuilder.CreateIndex(
                name: "IX_tags_OrganizationID",
                table: "tags",
                column: "OrganizationID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "groups_to_users_mtm");

            migrationBuilder.DropTable(
                name: "roles_to_paths_mtm");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "groups");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "routepaths");

            migrationBuilder.DropTable(
                name: "computeplatforms");

            migrationBuilder.DropTable(
                name: "loadbalancerplatforms");

            migrationBuilder.DropTable(
                name: "networkplatforms");

            migrationBuilder.DropTable(
                name: "credentials");

            migrationBuilder.DropTable(
                name: "organizations");
        }
    }
}
