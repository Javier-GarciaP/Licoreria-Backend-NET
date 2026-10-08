using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StaffChatClienteMesa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cliente",
                table: "sesiones_mesa",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notas",
                table: "sesiones_mesa",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "chat_mensajes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AutorId = table.Column<Guid>(type: "uuid", nullable: false),
                    AutorNombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Rol = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Mensaje = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_mensajes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chat_mensajes_CreatedAt",
                table: "chat_mensajes",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chat_mensajes");

            migrationBuilder.DropColumn(
                name: "Cliente",
                table: "sesiones_mesa");

            migrationBuilder.DropColumn(
                name: "Notas",
                table: "sesiones_mesa");
        }
    }
}
