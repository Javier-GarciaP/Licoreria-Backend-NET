using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModulosComplementarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClienteId",
                table: "ventas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VentaId",
                table: "cuentas_por_cobrar",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ventas_ClienteId",
                table: "ventas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "ux_cuentas_por_cobrar_por_venta",
                table: "cuentas_por_cobrar",
                column: "VentaId",
                unique: true,
                filter: "\"VentaId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ventas_clientes_ClienteId",
                table: "ventas",
                column: "ClienteId",
                principalTable: "clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ventas_clientes_ClienteId",
                table: "ventas");

            migrationBuilder.DropIndex(
                name: "IX_ventas_ClienteId",
                table: "ventas");

            migrationBuilder.DropIndex(
                name: "ux_cuentas_por_cobrar_por_venta",
                table: "cuentas_por_cobrar");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "VentaId",
                table: "cuentas_por_cobrar");
        }
    }
}
