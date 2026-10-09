using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedPasswordDemo123 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                column: "PasswordHash",
                value: "100000.FAjfLZhxDdPKpSSBsWnxhA==.oEa34uWpndtwYkEbsIYWwybInWw7MjWldrQtjwI46xI=");

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                column: "PasswordHash",
                value: "100000./aCU6rte+XCUv0r4ENjLuw==.1WuFy7dAWv4p3TZOWu05sIRNYmAQy25tmq7a2FxGY50=");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                column: "PasswordHash",
                value: "100000.bGljb3JlcmlhLWFkbWluIQ==.YwgspkL29TkDaFw34dp94bJtoYuLiheB1jTHjHoI0/A=");

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                column: "PasswordHash",
                value: "100000.bGljb3JlcmlhLWNhamVybw==.vBfTiTnA2NkFbJLRMHRR/Cp00Fm6VwefpzHLwFJcVY0=");
        }
    }
}
