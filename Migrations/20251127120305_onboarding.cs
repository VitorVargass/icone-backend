using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace icone_backend.Migrations
{
    /// <inheritdoc />
    public partial class Onboarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "onboarding_step",
                schema: "public",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "website",
                schema: "public",
                table: "companies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "onboarding_step",
                schema: "public",
                table: "users");

            migrationBuilder.AlterColumn<string>(
                name: "website",
                schema: "public",
                table: "companies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }
    }
}
