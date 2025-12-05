using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace icone_backend.Migrations
{
    /// <inheritdoc />
    public partial class UserIdIngredientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "created_by_user_id",
                schema: "public",
                table: "ingredients",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                schema: "public",
                table: "ingredients");
        }
    }
}
