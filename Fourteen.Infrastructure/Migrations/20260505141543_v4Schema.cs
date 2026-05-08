using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fourteen.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class v4Schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "domains",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    verification_status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    verification_token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    verified_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_domains", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "findings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    scan_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    severity = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    raw_data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    explanation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    recommendation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_findings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    gender = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    gender_probability = table.Column<double>(type: "float", nullable: false),
                    sample_size = table.Column<int>(type: "int", nullable: false),
                    age = table.Column<int>(type: "int", nullable: false),
                    age_group = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    country_id = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    country_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    country_probability = table.Column<double>(type: "float", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_revoked = table.Column<bool>(type: "bit", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    requested_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    started_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    failure_reason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    provider_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    username = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    avatar_url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_domains_created_at",
                table: "domains",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_domains_verification_status",
                table: "domains",
                column: "verification_status");

            migrationBuilder.CreateIndex(
                name: "IX_findings_created_at",
                table: "findings",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_findings_scan_id",
                table: "findings",
                column: "scan_id");

            migrationBuilder.CreateIndex(
                name: "IX_findings_severity",
                table: "findings",
                column: "severity");

            migrationBuilder.CreateIndex(
                name: "IX_findings_type",
                table: "findings",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_profiles_age",
                table: "profiles",
                column: "age");

            migrationBuilder.CreateIndex(
                name: "IX_profiles_age_group_created_at",
                table: "profiles",
                columns: new[] { "age_group", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_profiles_country_id_created_at",
                table: "profiles",
                columns: new[] { "country_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_profiles_created_at",
                table: "profiles",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_profiles_gender_created_at",
                table: "profiles",
                columns: new[] { "gender", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_profiles_gender_probability",
                table: "profiles",
                column: "gender_probability");

            migrationBuilder.CreateIndex(
                name: "IX_profiles_name",
                table: "profiles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_created_at",
                table: "refresh_tokens",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_expires_at",
                table: "refresh_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_is_revoked",
                table: "refresh_tokens",
                column: "is_revoked");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_scans_created_at",
                table: "scans",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_scans_domain_id",
                table: "scans",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "IX_scans_status",
                table: "scans",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_users_created_at",
                table: "users",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_is_active",
                table: "users",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_users_provider_id",
                table: "users",
                column: "provider_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "domains");

            migrationBuilder.DropTable(
                name: "findings");

            migrationBuilder.DropTable(
                name: "profiles");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "scans");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
