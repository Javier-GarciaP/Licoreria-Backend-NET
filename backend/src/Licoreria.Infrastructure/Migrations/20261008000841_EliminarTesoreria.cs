using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EliminarTesoreria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "movimientos_tesoreria");

            migrationBuilder.DropTable(
                name: "precios_producto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "movimientos_tesoreria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Moneda = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Motivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ReferenciaId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenciaTipo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimientos_tesoreria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "precios_producto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListaPrecioId = table.Column<Guid>(type: "uuid", nullable: false),
                    VarianteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Moneda = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Precio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_precios_producto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_precios_producto_listas_precio_ListaPrecioId",
                        column: x => x.ListaPrecioId,
                        principalTable: "listas_precio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_precios_producto_producto_variantes_VarianteId",
                        column: x => x.VarianteId,
                        principalTable: "producto_variantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "precios_producto",
                columns: new[] { "Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "ListaPrecioId", "Moneda", "Precio", "UpdatedBy", "VarianteId" },
                values: new object[,]
                {
                    { new Guid("90000000-0000-0000-0000-000000000001"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, new Guid("88888888-8888-8888-8888-888888888881"), "USD", 12.00m, null, new Guid("30000000-0000-0000-0000-000000000001") },
                    { new Guid("90000000-0000-0000-0000-000000000002"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, new Guid("88888888-8888-8888-8888-888888888881"), "USD", 38.00m, null, new Guid("30000000-0000-0000-0000-000000000002") },
                    { new Guid("90000000-0000-0000-0000-000000000003"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, new Guid("88888888-8888-8888-8888-888888888881"), "USD", 2.50m, null, new Guid("30000000-0000-0000-0000-000000000003") },
                    { new Guid("90000000-0000-0000-0000-000000000004"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, new Guid("88888888-8888-8888-8888-888888888881"), "USD", 3.00m, null, new Guid("30000000-0000-0000-0000-000000000004") },
                    { new Guid("90000000-0000-0000-0000-000000000005"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, new Guid("88888888-8888-8888-8888-888888888881"), "USD", 2.00m, null, new Guid("30000000-0000-0000-0000-000000000005") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_precios_producto_ListaPrecioId",
                table: "precios_producto",
                column: "ListaPrecioId");

            migrationBuilder.CreateIndex(
                name: "IX_precios_producto_VarianteId_ListaPrecioId_Moneda",
                table: "precios_producto",
                columns: new[] { "VarianteId", "ListaPrecioId", "Moneda" },
                unique: true);
        }
    }
}
