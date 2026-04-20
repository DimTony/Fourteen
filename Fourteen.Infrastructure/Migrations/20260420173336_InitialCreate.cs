using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fourteen.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    gender = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    gender_probability = table.Column<double>(type: "float", nullable: false),
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "profiles");
        }
    }
}
