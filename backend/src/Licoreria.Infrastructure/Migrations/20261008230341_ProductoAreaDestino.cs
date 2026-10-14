using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductoAreaDestino : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AreaDestino",
                table: "productos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Barra");

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "AreaDestino",
                value: "Barra");

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                column: "AreaDestino",
                value: "Barra");

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "AreaDestino",
                value: "Barra");

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                column: "AreaDestino",
                value: "Barra");

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                column: "AreaDestino",
                value: "Barra");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AreaDestino",
                table: "productos");
        }
    }
}
