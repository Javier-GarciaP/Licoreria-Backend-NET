using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecepcionesCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recepciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdenCompraId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_recepciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recepciones_ordenes_compra_OrdenCompraId",
                        column: x => x.OrdenCompraId,
                        principalTable: "ordenes_compra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_recepciones_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "recepcion_detalles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecepcionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdenCompraDetalleId = table.Column<Guid>(type: "uuid", nullable: false),
                    VarianteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cantidad = table.Column<decimal>(type: "numeric(14,3)", precision: 14, scale: 3, nullable: false),
                    CostoUnitarioUSD = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recepcion_detalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recepcion_detalles_orden_compra_detalles_OrdenCompraDetalle~",
                        column: x => x.OrdenCompraDetalleId,
                        principalTable: "orden_compra_detalles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_recepcion_detalles_producto_variantes_VarianteId",
                        column: x => x.VarianteId,
                        principalTable: "producto_variantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_recepcion_detalles_recepciones_RecepcionId",
                        column: x => x.RecepcionId,
                        principalTable: "recepciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recepcion_detalles_OrdenCompraDetalleId",
                table: "recepcion_detalles",
                column: "OrdenCompraDetalleId");

            migrationBuilder.CreateIndex(
                name: "IX_recepcion_detalles_RecepcionId",
                table: "recepcion_detalles",
                column: "RecepcionId");

            migrationBuilder.CreateIndex(
                name: "IX_recepcion_detalles_VarianteId",
                table: "recepcion_detalles",
                column: "VarianteId");

            migrationBuilder.CreateIndex(
                name: "IX_recepciones_Fecha",
                table: "recepciones",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_recepciones_OrdenCompraId",
                table: "recepciones",
                column: "OrdenCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_recepciones_UsuarioId",
                table: "recepciones",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recepcion_detalles");

            migrationBuilder.DropTable(
                name: "recepciones");
        }
    }
}
