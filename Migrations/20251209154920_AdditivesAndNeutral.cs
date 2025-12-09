using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace icone_backend.Migrations
{
    /// <inheritdoc />
    public partial class AdditivesAndNeutral : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "additives",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Scope = table.Column<int>(type: "integer", nullable: false),
                    IsReadOnly = table.Column<bool>(type: "boolean", nullable: false),
                    MaxDoseGL = table.Column<double>(type: "double precision", nullable: true),
                    Usage = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Scores_Stabilization = table.Column<double>(type: "double precision", nullable: true),
                    Scores_Emulsifying = table.Column<double>(type: "double precision", nullable: true),
                    Scores_LowPhResistance = table.Column<double>(type: "double precision", nullable: true),
                    Scores_Creaminess = table.Column<double>(type: "double precision", nullable: true),
                    Scores_Viscosity = table.Column<double>(type: "double precision", nullable: true),
                    Scores_Body = table.Column<double>(type: "double precision", nullable: true),
                    Scores_Elasticity = table.Column<double>(type: "double precision", nullable: true),
                    Scores_Crystallization = table.Column<double>(type: "double precision", nullable: true),
                    IncompatibleWithJson = table.Column<string>(type: "text", nullable: true),
                    CompatibleWithJson = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_additives", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "neutrals",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    GelatoType = table.Column<string>(type: "text", nullable: false),
                    Method = table.Column<string>(type: "text", nullable: false),
                    RecommendedDoseGPerKg = table.Column<double>(type: "double precision", nullable: false),
                    TotalDosagePerLiter = table.Column<double>(type: "double precision", nullable: false),
                    Scope = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    ComponentsJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_neutrals", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "additives",
                schema: "public");

            migrationBuilder.DropTable(
                name: "neutrals",
                schema: "public");
        }
    }
}
