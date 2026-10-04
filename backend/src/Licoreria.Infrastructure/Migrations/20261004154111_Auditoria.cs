using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Auditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    Usuario = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Accion = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Entidad = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    EntidadId = table.Column<Guid>(type: "uuid", nullable: true),
                    Datos = table.Column<string>(type: "jsonb", nullable: true),
                    Ip = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_CreatedAt",
                table: "audit_log",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_Entidad",
                table: "audit_log",
                column: "Entidad");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log");
        }
    }
}
