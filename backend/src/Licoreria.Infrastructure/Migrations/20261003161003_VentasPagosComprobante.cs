using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VentasPagosComprobante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_detalles_venta_productos_ProductoId",
                table: "detalles_venta");

            migrationBuilder.DropColumn(
                name: "MetodoPago",
                table: "ventas");

            migrationBuilder.RenameColumn(
                name: "ProductoId",
                table: "detalles_venta",
                newName: "VarianteId");

            migrationBuilder.RenameIndex(
                name: "IX_detalles_venta_ProductoId",
                table: "detalles_venta",
                newName: "IX_detalles_venta_VarianteId");

            migrationBuilder.AddColumn<Guid>(
                name: "CuentaId",
                table: "ventas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DescuentoUSD",
                table: "ventas",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "ventas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SubtotalUSD",
                table: "ventas",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "Cantidad",
                table: "detalles_venta",
                type: "numeric(14,3)",
                precision: 14,
                scale: 3,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<decimal>(
                name: "DescuentoUSD",
                table: "detalles_venta",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "EsCortesia",
                table: "detalles_venta",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "comprobantes_fiscales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VentaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NumeroControl = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalUSD = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalBS = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comprobantes_fiscales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_comprobantes_fiscales_ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "devoluciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VentaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MontoUSD = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ReintegrarInventario = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devoluciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_devoluciones_ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "metodos_pago",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metodos_pago", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "devolucion_detalles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DevolucionId = table.Column<Guid>(type: "uuid", nullable: false),
                    VarianteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cantidad = table.Column<decimal>(type: "numeric(14,3)", precision: 14, scale: 3, nullable: false),
                    PrecioUnitarioUSD = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devolucion_detalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_devolucion_detalles_devoluciones_DevolucionId",
                        column: x => x.DevolucionId,
                        principalTable: "devoluciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_devolucion_detalles_producto_variantes_VarianteId",
                        column: x => x.VarianteId,
                        principalTable: "producto_variantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pagos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VentaId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetodoPagoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Propina = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pagos_metodos_pago_MetodoPagoId",
                        column: x => x.MetodoPagoId,
                        principalTable: "metodos_pago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pagos_ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "metodos_pago",
                columns: new[] { "Id", "Activo", "Codigo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Nombre", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("bbbb0000-0000-0000-0000-000000000001"), true, "EfectivoUSD", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "Efectivo en dólares", null },
                    { new Guid("bbbb0000-0000-0000-0000-000000000002"), true, "EfectivoBS", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "Efectivo en bolívares", null },
                    { new Guid("bbbb0000-0000-0000-0000-000000000003"), true, "PagoMovil", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "Pago móvil", null },
                    { new Guid("bbbb0000-0000-0000-0000-000000000004"), true, "Zelle", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "Zelle", null },
                    { new Guid("bbbb0000-0000-0000-0000-000000000005"), true, "Punto", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "Punto de venta", null },
                    { new Guid("bbbb0000-0000-0000-0000-000000000006"), true, "Credito", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "Crédito", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ventas_Fecha",
                table: "ventas",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_comprobantes_fiscales_VentaId",
                table: "comprobantes_fiscales",
                column: "VentaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_devolucion_detalles_DevolucionId",
                table: "devolucion_detalles",
                column: "DevolucionId");

            migrationBuilder.CreateIndex(
                name: "IX_devolucion_detalles_VarianteId",
                table: "devolucion_detalles",
                column: "VarianteId");

            migrationBuilder.CreateIndex(
                name: "IX_devoluciones_VentaId",
                table: "devoluciones",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_metodos_pago_Codigo",
                table: "metodos_pago",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pagos_MetodoPagoId",
                table: "pagos",
                column: "MetodoPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_VentaId",
                table: "pagos",
                column: "VentaId");

            migrationBuilder.AddForeignKey(
                name: "FK_detalles_venta_producto_variantes_VarianteId",
                table: "detalles_venta",
                column: "VarianteId",
                principalTable: "producto_variantes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_detalles_venta_producto_variantes_VarianteId",
                table: "detalles_venta");

            migrationBuilder.DropTable(
                name: "comprobantes_fiscales");

            migrationBuilder.DropTable(
                name: "devolucion_detalles");

            migrationBuilder.DropTable(
                name: "pagos");

            migrationBuilder.DropTable(
                name: "devoluciones");

            migrationBuilder.DropTable(
                name: "metodos_pago");

            migrationBuilder.DropIndex(
                name: "IX_ventas_Fecha",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "CuentaId",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "DescuentoUSD",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "SubtotalUSD",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "DescuentoUSD",
                table: "detalles_venta");

            migrationBuilder.DropColumn(
                name: "EsCortesia",
                table: "detalles_venta");

            migrationBuilder.RenameColumn(
                name: "VarianteId",
                table: "detalles_venta",
                newName: "ProductoId");

            migrationBuilder.RenameIndex(
                name: "IX_detalles_venta_VarianteId",
                table: "detalles_venta",
                newName: "IX_detalles_venta_ProductoId");

            migrationBuilder.AddColumn<string>(
                name: "MetodoPago",
                table: "ventas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Cantidad",
                table: "detalles_venta",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(14,3)",
                oldPrecision: 14,
                oldScale: 3);

            migrationBuilder.AddForeignKey(
                name: "FK_detalles_venta_productos_ProductoId",
                table: "detalles_venta",
                column: "ProductoId",
                principalTable: "productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
