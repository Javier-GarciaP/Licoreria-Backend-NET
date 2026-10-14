using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AuditoriaTotal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "audit_log",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Cambio");

            // Las filas previas eran todas registros manuales: se marcan como Evento.
            migrationBuilder.Sql("UPDATE audit_log SET \"Tipo\" = 'Evento';");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_Accion",
                table: "audit_log",
                column: "Accion");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_Entidad_EntidadId",
                table: "audit_log",
                columns: new[] { "Entidad", "EntidadId" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_Tipo",
                table: "audit_log",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_UsuarioId_CreatedAt",
                table: "audit_log",
                columns: new[] { "UsuarioId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_audit_log_Accion",
                table: "audit_log");

            migrationBuilder.DropIndex(
                name: "IX_audit_log_Entidad_EntidadId",
                table: "audit_log");

            migrationBuilder.DropIndex(
                name: "IX_audit_log_Tipo",
                table: "audit_log");

            migrationBuilder.DropIndex(
                name: "IX_audit_log_UsuarioId_CreatedAt",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "audit_log");
        }
    }
}
