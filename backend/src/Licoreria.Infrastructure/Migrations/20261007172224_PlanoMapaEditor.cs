using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PlanoMapaEditor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Alto",
                table: "zonas",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Ancho",
                table: "zonas",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "zonas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PosX",
                table: "zonas",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PosY",
                table: "zonas",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AltoFondo",
                table: "planos",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AnchoFondo",
                table: "planos",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Piso",
                table: "planos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Rejilla",
                table: "planos",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "plano_elementos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Forma",
                table: "plano_elementos",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MesaId",
                table: "plano_elementos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Z",
                table: "plano_elementos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_plano_elementos_MesaId",
                table: "plano_elementos",
                column: "MesaId");

            migrationBuilder.AddForeignKey(
                name: "FK_plano_elementos_mesas_MesaId",
                table: "plano_elementos",
                column: "MesaId",
                principalTable: "mesas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_plano_elementos_mesas_MesaId",
                table: "plano_elementos");

            migrationBuilder.DropIndex(
                name: "IX_plano_elementos_MesaId",
                table: "plano_elementos");

            migrationBuilder.DropColumn(
                name: "Alto",
                table: "zonas");

            migrationBuilder.DropColumn(
                name: "Ancho",
                table: "zonas");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "zonas");

            migrationBuilder.DropColumn(
                name: "PosX",
                table: "zonas");

            migrationBuilder.DropColumn(
                name: "PosY",
                table: "zonas");

            migrationBuilder.DropColumn(
                name: "AltoFondo",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "AnchoFondo",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "Piso",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "Rejilla",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "plano_elementos");

            migrationBuilder.DropColumn(
                name: "Forma",
                table: "plano_elementos");

            migrationBuilder.DropColumn(
                name: "MesaId",
                table: "plano_elementos");

            migrationBuilder.DropColumn(
                name: "Z",
                table: "plano_elementos");
        }
    }
}
