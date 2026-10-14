using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VentaSesionCajaTurno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SesionCajaId",
                table: "ventas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CuentasDesalojadas",
                table: "sesiones_caja",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "VentasDelTurnoUSD",
                table: "sesiones_caja",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_ventas_SesionCajaId",
                table: "ventas",
                column: "SesionCajaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ventas_SesionCajaId",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "SesionCajaId",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "CuentasDesalojadas",
                table: "sesiones_caja");

            migrationBuilder.DropColumn(
                name: "VentasDelTurnoUSD",
                table: "sesiones_caja");
        }
    }
}
