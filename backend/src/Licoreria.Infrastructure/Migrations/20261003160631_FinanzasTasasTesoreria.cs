using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FinanzasTasasTesoreria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "movimientos_tesoreria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Motivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ReferenciaTipo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ReferenciaId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimientos_tesoreria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tasas_cambio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tasas_cambio", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "tasas_cambio",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Fecha", "IsDeleted", "LastModifiedAt", "Tipo", "UpdatedBy", "Valor" },
                values: new object[,]
                {
                    { new Guid("aaaa0000-0000-0000-0000-000000000001"), new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "BCV", null, 36.5000m },
                    { new Guid("aaaa0000-0000-0000-0000-000000000002"), new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Paralelo", null, 40.0000m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_tasas_cambio_Fecha_Tipo",
                table: "tasas_cambio",
                columns: new[] { "Fecha", "Tipo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "movimientos_tesoreria");

            migrationBuilder.DropTable(
                name: "tasas_cambio");
        }
    }
}
