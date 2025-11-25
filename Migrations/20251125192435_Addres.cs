using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace icone_backend.Migrations
{
    /// <inheritdoc />
    public partial class Addres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "city",
                schema: "public",
                table: "companies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "country_code",
                schema: "public",
                table: "companies",
                type: "character varying(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "line1",
                schema: "public",
                table: "companies",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "line2",
                schema: "public",
                table: "companies",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "postal_code",
                schema: "public",
                table: "companies",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "state_region",
                schema: "public",
                table: "companies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "city",
                schema: "public",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "country_code",
                schema: "public",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "line1",
                schema: "public",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "line2",
                schema: "public",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "postal_code",
                schema: "public",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "state_region",
                schema: "public",
                table: "companies");
        }
    }
}
