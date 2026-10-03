using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CajaSesiones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "denominaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Moneda = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_denominaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sesiones_caja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FondoInicial = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoEsperado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoContado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Descuadre = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_sesiones_caja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sesiones_caja_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "arqueos_denominacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SesionCajaId = table.Column<Guid>(type: "uuid", nullable: false),
                    DenominacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_arqueos_denominacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_arqueos_denominacion_denominaciones_DenominacionId",
                        column: x => x.DenominacionId,
                        principalTable: "denominaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_arqueos_denominacion_sesiones_caja_SesionCajaId",
                        column: x => x.SesionCajaId,
                        principalTable: "sesiones_caja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "movimientos_caja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SesionCajaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Motivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimientos_caja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_movimientos_caja_sesiones_caja_SesionCajaId",
                        column: x => x.SesionCajaId,
                        principalTable: "sesiones_caja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "denominaciones",
                columns: new[] { "Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Moneda", "Tipo", "UpdatedBy", "Valor" },
                values: new object[,]
                {
                    { new Guid("cccc0000-0000-0000-0000-000000000001"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "USD", "Billete", null, 1m },
                    { new Guid("cccc0000-0000-0000-0000-000000000002"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "USD", "Billete", null, 5m },
                    { new Guid("cccc0000-0000-0000-0000-000000000003"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "USD", "Billete", null, 10m },
                    { new Guid("cccc0000-0000-0000-0000-000000000004"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "USD", "Billete", null, 20m },
                    { new Guid("cccc0000-0000-0000-0000-000000000005"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "USD", "Billete", null, 50m },
                    { new Guid("cccc0000-0000-0000-0000-000000000006"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "USD", "Billete", null, 100m },
                    { new Guid("cccc0000-0000-0000-0000-000000000007"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "BS", "Billete", null, 20m },
                    { new Guid("cccc0000-0000-0000-0000-000000000008"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "BS", "Billete", null, 50m },
                    { new Guid("cccc0000-0000-0000-0000-000000000009"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "BS", "Billete", null, 100m },
                    { new Guid("cccc0000-0000-0000-0000-000000000010"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "BS", "Billete", null, 500m },
                    { new Guid("cccc0000-0000-0000-0000-000000000011"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "BS", "Billete", null, 1000m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_arqueos_denominacion_DenominacionId",
                table: "arqueos_denominacion",
                column: "DenominacionId");

            migrationBuilder.CreateIndex(
                name: "IX_arqueos_denominacion_SesionCajaId",
                table: "arqueos_denominacion",
                column: "SesionCajaId");

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_caja_SesionCajaId",
                table: "movimientos_caja",
                column: "SesionCajaId");

            migrationBuilder.CreateIndex(
                name: "IX_sesiones_caja_UsuarioId",
                table: "sesiones_caja",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "arqueos_denominacion");

            migrationBuilder.DropTable(
                name: "movimientos_caja");

            migrationBuilder.DropTable(
                name: "denominaciones");

            migrationBuilder.DropTable(
                name: "sesiones_caja");
        }
    }
}
