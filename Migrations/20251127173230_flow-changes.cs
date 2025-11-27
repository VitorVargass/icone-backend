using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace icone_backend.Migrations
{
    /// <inheritdoc />
    public partial class flowchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_users_company_id",
                schema: "public",
                table: "users",
                column: "company_id");

            migrationBuilder.AddForeignKey(
                name: "FK_users_companies_company_id",
                schema: "public",
                table: "users",
                column: "company_id",
                principalSchema: "public",
                principalTable: "companies",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_companies_company_id",
                schema: "public",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_company_id",
                schema: "public",
                table: "users");
        }
    }
}
