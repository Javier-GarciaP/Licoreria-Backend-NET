using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PresentacionesYPorciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "envases_abiertos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PorcionVarianteId = table.Column<Guid>(type: "uuid", nullable: false),
                    PresentacionVarianteId = table.Column<Guid>(type: "uuid", nullable: false),
                    PorcionesIniciales = table.Column<int>(type: "integer", nullable: false),
                    PorcionesRestantes = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AbiertoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AbiertoPorUsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    CerradoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MotivoDescarte = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_envases_abiertos", x => x.Id);
                    table.CheckConstraint("ck_envase_iniciales_positivas", "\"PorcionesIniciales\" >= 1");
                    table.CheckConstraint("ck_envase_restantes_no_negativas", "\"PorcionesRestantes\" >= 0");
                    table.CheckConstraint("ck_envase_restantes_no_superan_iniciales", "\"PorcionesRestantes\" <= \"PorcionesIniciales\"");
                    table.ForeignKey(
                        name: "FK_envases_abiertos_producto_variantes_PorcionVarianteId",
                        column: x => x.PorcionVarianteId,
                        principalTable: "producto_variantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_envases_abiertos_producto_variantes_PresentacionVarianteId",
                        column: x => x.PresentacionVarianteId,
                        principalTable: "producto_variantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "porcion_origenes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PorcionVarianteId = table.Column<Guid>(type: "uuid", nullable: false),
                    PresentacionVarianteId = table.Column<Guid>(type: "uuid", nullable: false),
                    PorcionesPorUnidad = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_porcion_origenes", x => x.Id);
                    table.CheckConstraint("ck_porcion_origen_porciones_por_unidad", "\"PorcionesPorUnidad\" >= 1");
                    table.ForeignKey(
                        name: "FK_porcion_origenes_producto_variantes_PorcionVarianteId",
                        column: x => x.PorcionVarianteId,
                        principalTable: "producto_variantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_porcion_origenes_producto_variantes_PresentacionVarianteId",
                        column: x => x.PresentacionVarianteId,
                        principalTable: "producto_variantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_porcion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VarianteId = table.Column<Guid>(type: "uuid", nullable: false),
                    PorcionesReservadas = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_porcion", x => x.Id);
                    table.CheckConstraint("ck_stock_porcion_reservadas_no_negativas", "\"PorcionesReservadas\" >= 0");
                    table.ForeignKey(
                        name: "FK_stock_porcion_producto_variantes_VarianteId",
                        column: x => x.VarianteId,
                        principalTable: "producto_variantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_envases_abiertos_PresentacionVarianteId",
                table: "envases_abiertos",
                column: "PresentacionVarianteId");

            migrationBuilder.CreateIndex(
                name: "ix_envases_porcion_estado_fecha",
                table: "envases_abiertos",
                columns: new[] { "PorcionVarianteId", "Estado", "AbiertoEn" });

            migrationBuilder.CreateIndex(
                name: "IX_porcion_origenes_PresentacionVarianteId",
                table: "porcion_origenes",
                column: "PresentacionVarianteId");

            migrationBuilder.CreateIndex(
                name: "ux_porcion_origen_par",
                table: "porcion_origenes",
                columns: new[] { "PorcionVarianteId", "PresentacionVarianteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_stock_porcion_variante",
                table: "stock_porcion",
                column: "VarianteId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "envases_abiertos");

            migrationBuilder.DropTable(
                name: "porcion_origenes");

            migrationBuilder.DropTable(
                name: "stock_porcion");
        }
    }
}
