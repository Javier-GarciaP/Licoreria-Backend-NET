using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PresentacionesBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) La variante base guarda stock y es la que se ordena.
            migrationBuilder.AddColumn<bool>(
                name: "EsBase",
                table: "producto_variantes",
                nullable: false,
                defaultValue: false);

            // 2) Las recetas pasan de "por producto" a "por variante vendida".
            migrationBuilder.AddColumn<Guid>(
                name: "VarianteVendidaId",
                table: "recetas",
                nullable: true);

            // 3) Backfill: cada receta existente se asigna a la primera variante de su producto.
            migrationBuilder.Sql(
                "UPDATE recetas SET \"VarianteVendidaId\" = sub.\"Id\" FROM (" +
                "SELECT DISTINCT ON (pv.\"ProductoId\") pv.\"Id\", pv.\"ProductoId\" " +
                "FROM producto_variantes pv ORDER BY pv.\"ProductoId\", pv.\"CreatedAt\" ASC" +
                ") AS sub WHERE recetas.\"ProductoId\" = sub.\"ProductoId\"");

            // 4) Quitar la relación por producto.
            migrationBuilder.DropForeignKey("FK_recetas_productos_ProductoId", "recetas");
            migrationBuilder.DropIndex("IX_recetas_ProductoId_VarianteInsumoId", "recetas");
            migrationBuilder.DropColumn("ProductoId", "recetas");

            // 5) La variante vendida es obligatoria y con FK.
            migrationBuilder.AlterColumn<Guid>(
                name: "VarianteVendidaId",
                table: "recetas",
                nullable: false,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_recetas_producto_variantes_VarianteVendidaId",
                table: "recetas",
                column: "VarianteVendidaId",
                principalTable: "producto_variantes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                "IX_recetas_VarianteVendidaId_VarianteInsumoId",
                "recetas",
                new[] { "VarianteVendidaId", "VarianteInsumoId" },
                unique: true);

            // 6) Backfill EsBase: la primera variante de cada producto de Barra Simple es base.
            migrationBuilder.Sql(
                "UPDATE producto_variantes SET \"EsBase\" = true WHERE \"Id\" IN (" +
                "SELECT DISTINCT ON (pv.\"ProductoId\") pv.\"Id\" " +
                "FROM producto_variantes pv " +
                "INNER JOIN productos p ON p.\"Id\" = pv.\"ProductoId\" " +
                "WHERE p.\"AreaDestino\" = 'Barra' AND p.\"Tipo\" = 'Simple' AND p.\"IsDeleted\" = false " +
                "ORDER BY pv.\"ProductoId\", pv.\"CreatedAt\" ASC" +
                ")");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_recetas_VarianteVendidaId_VarianteInsumoId", "recetas");
            migrationBuilder.DropForeignKey("FK_recetas_producto_variantes_VarianteVendidaId", "recetas");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductoId",
                table: "recetas",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE recetas SET \"ProductoId\" = sub.\"ProductoId\" FROM (" +
                "SELECT DISTINCT ON (pv.\"ProductoId\") pv.\"Id\", pv.\"ProductoId\" " +
                "FROM producto_variantes pv ORDER BY pv.\"ProductoId\", pv.\"CreatedAt\" ASC" +
                ") AS sub WHERE recetas.\"VarianteVendidaId\" = sub.\"Id\"");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductoId",
                table: "recetas",
                nullable: false,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_recetas_productos_ProductoId",
                table: "recetas",
                column: "ProductoId",
                principalTable: "productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                "IX_recetas_ProductoId_VarianteInsumoId",
                "recetas",
                new[] { "ProductoId", "VarianteInsumoId" },
                unique: true);

            migrationBuilder.DropColumn("VarianteVendidaId", "recetas");
            migrationBuilder.DropColumn("EsBase", "producto_variantes");
        }
    }
}
