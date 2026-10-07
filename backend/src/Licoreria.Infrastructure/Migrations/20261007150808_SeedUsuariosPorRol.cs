using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedUsuariosPorRol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Limpia filas previas con los mismos correos (p. ej. creadas a mano)
            // para evitar el choque con el índice único de Email antes de sembrar.
            migrationBuilder.Sql(
                "DELETE FROM usuarios WHERE \"Email\" IN " +
                "('mesero1@licoreria.com','barra1@licoreria.com','cocina1@licoreria.com','host1@licoreria.com','editor1@licoreria.com');");

            migrationBuilder.InsertData(
                table: "usuarios",
                columns: new[] { "Id", "Activo", "CreatedAt", "CreatedBy", "Email", "IsDeleted", "LastModifiedAt", "NombreCompleto", "PasswordHash", "Rol", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000003"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, "mesero1@licoreria.com", false, null, "Mesero Salón", "100000.AUk/0TjrGP/TROe9YBkEHg==.7/s6R8N/f1Yyq3JSRqES6ve1sn9mwRVO6ojw46ltKWs=", "Mesero", null },
                    { new Guid("20000000-0000-0000-0000-000000000004"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, "barra1@licoreria.com", false, null, "Barra Principal", "100000.yNO0fMCBb8nJcrdcPZbrXQ==.yWAcS5bMk5TWWmOyxmfMECn/wmG06aV59aV48P+UFAM=", "Barra", null },
                    { new Guid("20000000-0000-0000-0000-000000000005"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, "cocina1@licoreria.com", false, null, "Cocina Principal", "100000.tJ3DjqIW4P1+kBI6IYS3/A==./OrON+2Far9+3lLaeY7sqPpvB76AQTbBFyd33n3G9zM=", "Cocina", null },
                    { new Guid("20000000-0000-0000-0000-000000000006"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, "host1@licoreria.com", false, null, "Host Recepción", "100000.QDkQ0DimXrzSqKCOZpwjZg==.5a4nNjO9hVwfdaQNMbNoS3yL781iE33Djwnq9LL5VAU=", "Host", null },
                    { new Guid("20000000-0000-0000-0000-000000000007"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, "editor1@licoreria.com", false, null, "Editor de Contenido", "100000.5IgpLdoJsYDx+QHJJZnghQ==.+FAcpwzA3baeUGj/MCPeJzFfJ3ikLCw+iw8jWGhIWH0=", "EditorContenido", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "usuarios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "usuarios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "usuarios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "usuarios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "usuarios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000007"));
        }
    }
}
