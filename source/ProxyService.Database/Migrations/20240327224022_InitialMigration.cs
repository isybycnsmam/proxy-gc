using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProxyService.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "checking_methods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TestTarget = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDisabled = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checking_methods", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "checking_runs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Ignore = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Created = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checking_runs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "getting_methods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDisabled = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_getting_methods", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "proxies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Ip = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Port = table.Column<int>(type: "int", nullable: false),
                    CountryCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Anonymity = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LastChecked = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proxies", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "checking_sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Ignore = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Elapsed = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Created = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CheckingMethodId = table.Column<int>(type: "int", nullable: false),
                    CheckingRunId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checking_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_checking_sessions_checking_methods_CheckingMethodId",
                        column: x => x.CheckingMethodId,
                        principalTable: "checking_methods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_checking_sessions_checking_runs_CheckingRunId",
                        column: x => x.CheckingRunId,
                        principalTable: "checking_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "checking_results",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Result = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ResponseTime = table.Column<int>(type: "int", nullable: false),
                    Ignore = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProxyId = table.Column<int>(type: "int", nullable: true),
                    CheckingSessionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checking_results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_checking_results_checking_sessions_CheckingSessionId",
                        column: x => x.CheckingSessionId,
                        principalTable: "checking_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_checking_results_proxies_ProxyId",
                        column: x => x.ProxyId,
                        principalTable: "proxies",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "checking_methods",
                columns: new[] { "Id", "Description", "IsDisabled", "Name", "TestTarget" },
                values: new object[,]
                {
                    { 1, "Tcp", false, "Ping", null },
                    { 2, "Https", false, "Site", "https://wtfismyip.com/text" },
                    { 3, "Http", false, "Site", "http://ifconfig.io/ip" }
                });

            migrationBuilder.InsertData(
                table: "getting_methods",
                columns: new[] { "Id", "Description", "IsDisabled", "Name" },
                values: new object[,]
                {
                    { 1, "Downloads ~400 proxies from txt file", false, "TextSpysOne" },
                    { 2, "Downloads 500 ssl proxies from html page", false, "HttpsSpysOne" },
                    { 3, "Downloads ~700 proxies from 4 html sub-pages", false, "ProxyOrg" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_checking_results_CheckingSessionId",
                table: "checking_results",
                column: "CheckingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_checking_results_ProxyId",
                table: "checking_results",
                column: "ProxyId");

            migrationBuilder.CreateIndex(
                name: "IX_checking_sessions_CheckingMethodId",
                table: "checking_sessions",
                column: "CheckingMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_checking_sessions_CheckingRunId",
                table: "checking_sessions",
                column: "CheckingRunId");

            migrationBuilder.CreateIndex(
                name: "IX_getting_methods_Name",
                table: "getting_methods",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proxies_Ip_Port",
                table: "proxies",
                columns: new[] { "Ip", "Port" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "checking_results");

            migrationBuilder.DropTable(
                name: "getting_methods");

            migrationBuilder.DropTable(
                name: "checking_sessions");

            migrationBuilder.DropTable(
                name: "proxies");

            migrationBuilder.DropTable(
                name: "checking_methods");

            migrationBuilder.DropTable(
                name: "checking_runs");
        }
    }
}
