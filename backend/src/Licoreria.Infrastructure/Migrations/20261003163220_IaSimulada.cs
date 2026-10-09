using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IaSimulada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_generaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Entrada = table.Column<string>(type: "jsonb", nullable: false),
                    Salida = table.Column<string>(type: "jsonb", nullable: true),
                    Modelo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Costo = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    Aprobado = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_generaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "planos_generados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AiGeneracionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Resultado = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planos_generados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_planos_generados_ai_generaciones_AiGeneracionId",
                        column: x => x.AiGeneracionId,
                        principalTable: "ai_generaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_planos_generados_AiGeneracionId",
                table: "planos_generados",
                column: "AiGeneracionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "planos_generados");

            migrationBuilder.DropTable(
                name: "ai_generaciones");
        }
    }
}
