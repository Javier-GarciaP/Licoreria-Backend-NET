using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ComprasProveedores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "proveedores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Rif = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Contacto = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Direccion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DiasCredito = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proveedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ordenes_compra",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProveedorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    TotalUSD = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordenes_compra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ordenes_compra_proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "orden_compra_detalles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdenCompraId = table.Column<Guid>(type: "uuid", nullable: false),
                    VarianteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cantidad = table.Column<decimal>(type: "numeric(14,3)", precision: 14, scale: 3, nullable: false),
                    CostoUnitarioUSD = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CantidadRecibida = table.Column<decimal>(type: "numeric(14,3)", precision: 14, scale: 3, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orden_compra_detalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_orden_compra_detalles_ordenes_compra_OrdenCompraId",
                        column: x => x.OrdenCompraId,
                        principalTable: "ordenes_compra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_orden_compra_detalles_producto_variantes_VarianteId",
                        column: x => x.VarianteId,
                        principalTable: "producto_variantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_orden_compra_detalles_OrdenCompraId",
                table: "orden_compra_detalles",
                column: "OrdenCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_orden_compra_detalles_VarianteId",
                table: "orden_compra_detalles",
                column: "VarianteId");

            migrationBuilder.CreateIndex(
                name: "IX_ordenes_compra_Fecha",
                table: "ordenes_compra",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_ordenes_compra_Numero",
                table: "ordenes_compra",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ordenes_compra_ProveedorId",
                table: "ordenes_compra",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_proveedores_Rif",
                table: "proveedores",
                column: "Rif");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "orden_compra_detalles");

            migrationBuilder.DropTable(
                name: "ordenes_compra");

            migrationBuilder.DropTable(
                name: "proveedores");
        }
    }
}
