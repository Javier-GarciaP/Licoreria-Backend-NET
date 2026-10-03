using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CuentasComandas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sesiones_mesa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreMesa = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    MesaId = table.Column<Guid>(type: "uuid", nullable: true),
                    AbiertaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CerradaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sesiones_mesa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cuentas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SesionMesaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAbonado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuentas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cuentas_sesiones_mesa_SesionMesaId",
                        column: x => x.SesionMesaId,
                        principalTable: "sesiones_mesa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "abonos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CuentaId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetodoPagoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_abonos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_abonos_cuentas_CuentaId",
                        column: x => x.CuentaId,
                        principalTable: "cuentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_abonos_metodos_pago_MetodoPagoId",
                        column: x => x.MetodoPagoId,
                        principalTable: "metodos_pago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "comandas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CuentaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Area = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comandas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_comandas_cuentas_CuentaId",
                        column: x => x.CuentaId,
                        principalTable: "cuentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comanda_detalles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ComandaId = table.Column<Guid>(type: "uuid", nullable: false),
                    VarianteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cantidad = table.Column<decimal>(type: "numeric(14,3)", precision: 14, scale: 3, nullable: false),
                    PrecioUnitarioUSD = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AreaDestino = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EsCortesia = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comanda_detalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_comanda_detalles_comandas_ComandaId",
                        column: x => x.ComandaId,
                        principalTable: "comandas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_comanda_detalles_producto_variantes_VarianteId",
                        column: x => x.VarianteId,
                        principalTable: "producto_variantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_abonos_CuentaId",
                table: "abonos",
                column: "CuentaId");

            migrationBuilder.CreateIndex(
                name: "IX_abonos_MetodoPagoId",
                table: "abonos",
                column: "MetodoPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_comanda_detalles_ComandaId",
                table: "comanda_detalles",
                column: "ComandaId");

            migrationBuilder.CreateIndex(
                name: "IX_comanda_detalles_VarianteId",
                table: "comanda_detalles",
                column: "VarianteId");

            migrationBuilder.CreateIndex(
                name: "IX_comandas_CuentaId",
                table: "comandas",
                column: "CuentaId");

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_SesionMesaId",
                table: "cuentas",
                column: "SesionMesaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "abonos");

            migrationBuilder.DropTable(
                name: "comanda_detalles");

            migrationBuilder.DropTable(
                name: "comandas");

            migrationBuilder.DropTable(
                name: "cuentas");

            migrationBuilder.DropTable(
                name: "sesiones_mesa");
        }
    }
}
