using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace icone_backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangeIngredient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "additives",
                schema: "public");

            migrationBuilder.AlterColumn<double>(
                name: "WaterPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "TotalSolidsPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "SugarPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "SodiumMg",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "ProteinPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "PotassiumMg",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "Pod",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "Pac",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "OtherSolidsPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "NonFatSolidsPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "MilkSolidsPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "LactosePct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "KcalPer100g",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "FiberPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "FatTransPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "FatSaturatedPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "FatPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "FatMonounsaturatedPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "CholesterolMg",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "CarbsPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "AlcoholPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AddColumn<string>(
                name: "CompatibleWithJson",
                schema: "public",
                table: "ingredients",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "public",
                table: "ingredients",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "public",
                table: "ingredients",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncompatibleWithJson",
                schema: "public",
                table: "ingredients",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MaxDoseGL",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Scores_Body",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Scores_Creaminess",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Scores_Crystallization",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Scores_Elasticity",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Scores_Emulsifying",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Scores_LowPhResistance",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Scores_Stabilization",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Scores_Viscosity",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "public",
                table: "ingredients",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Usage",
                schema: "public",
                table: "ingredients",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompatibleWithJson",
                schema: "public",
                table: "ingredients");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "public",
                table: "ingredients");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "public",
                table: "ingredients");

            migrationBuilder.DropColumn(
                name: "IncompatibleWithJson",
                schema: "public",
                table: "ingredients");

            migrationBuilder.DropColumn(
                name: "MaxDoseGL",
                schema: "public",
                table: "ingredients");

            migrationBuilder.DropColumn(
                name: "Scores_Body",
                schema: "public",
                table: "ingredients");

            migrationBuilder.DropColumn(
                name: "Scores_Creaminess",
                schema: "public",
                table: "ingredients");

            migrationBuilder.DropColumn(
                name: "Scores_Crystallization",
                schema: "public",
                table: "ingredients");

            migrationBuilder.DropColumn(
                name: "Scores_Elasticity",
                schema: "public",
                table: "ingredients");

            migrationBuilder.DropColumn(
                name: "Scores_Emulsifying",
                schema: "public",
                table: "ingredients");

            migrationBuilder.DropColumn(
                name: "Scores_LowPhResistance",
                schema: "public",
                table: "ingredients");

            migrationBuilder.DropColumn(
                name: "Scores_Stabilization",
                schema: "public",
                table: "ingredients");

            migrationBuilder.DropColumn(
                name: "Scores_Viscosity",
                schema: "public",
                table: "ingredients");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "public",
                table: "ingredients");

            migrationBuilder.DropColumn(
                name: "Usage",
                schema: "public",
                table: "ingredients");

            migrationBuilder.AlterColumn<double>(
                name: "WaterPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "TotalSolidsPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "SugarPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "SodiumMg",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "ProteinPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "PotassiumMg",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Pod",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Pac",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "OtherSolidsPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "NonFatSolidsPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "MilkSolidsPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "LactosePct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "KcalPer100g",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "FiberPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "FatTransPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "FatSaturatedPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "FatPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "FatMonounsaturatedPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "CholesterolMg",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "CarbsPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "AlcoholPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "additives",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Category = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompatibleWithJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IncompatibleWithJson = table.Column<string>(type: "text", nullable: true),
                    MaxDoseGL = table.Column<double>(type: "double precision", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Scope = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Usage = table.Column<int>(type: "integer", nullable: false),
                    Scores_Body = table.Column<double>(type: "double precision", nullable: true),
                    Scores_Creaminess = table.Column<double>(type: "double precision", nullable: true),
                    Scores_Crystallization = table.Column<double>(type: "double precision", nullable: true),
                    Scores_Elasticity = table.Column<double>(type: "double precision", nullable: true),
                    Scores_Emulsifying = table.Column<double>(type: "double precision", nullable: true),
                    Scores_LowPhResistance = table.Column<double>(type: "double precision", nullable: true),
                    Scores_Stabilization = table.Column<double>(type: "double precision", nullable: true),
                    Scores_Viscosity = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_additives", x => x.Id);
                });
        }
    }
}
