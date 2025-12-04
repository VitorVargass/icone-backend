using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace icone_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddIngredient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ingredients",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    WaterPct = table.Column<double>(type: "double precision", nullable: false),
                    FatPct = table.Column<double>(type: "double precision", nullable: false),
                    ProteinPct = table.Column<double>(type: "double precision", nullable: false),
                    SugarPct = table.Column<double>(type: "double precision", nullable: false),
                    LactosePct = table.Column<double>(type: "double precision", nullable: false),
                    FiberPct = table.Column<double>(type: "double precision", nullable: false),
                    CarbsPct = table.Column<double>(type: "double precision", nullable: false),
                    AlcoholPct = table.Column<double>(type: "double precision", nullable: false),
                    TotalSolidsPct = table.Column<double>(type: "double precision", nullable: false),
                    Pac = table.Column<double>(type: "double precision", nullable: false),
                    Pod = table.Column<double>(type: "double precision", nullable: false),
                    KcalPer100g = table.Column<double>(type: "double precision", nullable: false),
                    SodiumMg = table.Column<double>(type: "double precision", nullable: false),
                    PotassiumMg = table.Column<double>(type: "double precision", nullable: false),
                    CholesterolMg = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ingredients", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ingredients",
                schema: "public");
        }
    }
}
