using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace icone_backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCalcSolids : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "MilkSolidsPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "NonFatSolidsPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OtherSolidsPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MilkSolidsPct",
                schema: "public",
                table: "ingredients");

            migrationBuilder.DropColumn(
                name: "NonFatSolidsPct",
                schema: "public",
                table: "ingredients");

            migrationBuilder.DropColumn(
                name: "OtherSolidsPct",
                schema: "public",
                table: "ingredients");
        }
    }
}
