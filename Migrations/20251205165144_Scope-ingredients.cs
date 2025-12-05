using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace icone_backend.Migrations
{
    /// <inheritdoc />
    public partial class Scopeingredients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                schema: "public",
                table: "ingredients",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FatMonounsaturatedPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FatSaturatedPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FatTransPct",
                schema: "public",
                table: "ingredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Scope",
                schema: "public",
                table: "ingredients",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyId",
                schema: "public",
                table: "ingredients");

            migrationBuilder.DropColumn(
                name: "FatMonounsaturatedPct",
                schema: "public",
                table: "ingredients");

            migrationBuilder.DropColumn(
                name: "FatSaturatedPct",
                schema: "public",
                table: "ingredients");

            migrationBuilder.DropColumn(
                name: "FatTransPct",
                schema: "public",
                table: "ingredients");

            migrationBuilder.DropColumn(
                name: "Scope",
                schema: "public",
                table: "ingredients");
        }
    }
}
