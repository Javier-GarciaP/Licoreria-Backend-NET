using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CroquisPlanos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsPlantilla",
                table: "planos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FondoAjuste",
                table: "planos",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "contener");

            migrationBuilder.AddColumn<string>(
                name: "FondoImagenUrl",
                table: "planos",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FondoOpacidad",
                table: "planos",
                type: "numeric(3,2)",
                precision: 3,
                scale: 2,
                nullable: false,
                defaultValue: 0.5m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EsPlantilla",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "FondoAjuste",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "FondoImagenUrl",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "FondoOpacidad",
                table: "planos");
        }
    }
}
