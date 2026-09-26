using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdaptacionBaseEntityGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.AlterColumn<Guid>(
                name: "UsuarioId",
                table: "Ventas",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Ventas",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Ventas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Ventas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Ventas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Usuarios",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Usuarios",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoriaId",
                table: "Productos",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Productos",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Productos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Productos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "VentaId",
                table: "DetallesVenta",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductoId",
                table: "DetallesVenta",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "DetallesVenta",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "DetallesVenta",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DetallesVenta",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "DetallesVenta",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Categorias",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Categorias",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Categorias",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Categorias",
                type: "datetime2",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "CreatedAt", "Descripcion", "IsDeleted", "LastModifiedAt", "Nombre" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), "Rones, Whiskeys, Anises y Vodka", false, null, "Licores" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), "Refrescos y bebidas carbonatadas", false, null, "Gaseosas" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), "Bebidas para rendimiento y energía", false, null, "Energizantes" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), "Snacks, papitas y frutos secos", false, null, "Pasabocas" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Activo", "CreatedAt", "Email", "IsDeleted", "LastModifiedAt", "NombreCompleto", "PasswordHash", "Rol" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), "admin@licoreria.com", false, null, "Administrador Principal", "admin123_hash", "Admin" },
                    { new Guid("20000000-0000-0000-0000-000000000002"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), "cajero1@licoreria.com", false, null, "Cajero Turno Mañana", "cajero123_hash", "Cajero" }
                });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "Activo", "CategoriaId", "CodigoBarras", "CreatedAt", "ImagenUrl", "IsDeleted", "LastModifiedAt", "Nombre", "PrecioCompraUSD", "PrecioVentaUSD", "Stock", "StockMinimo" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), true, new Guid("11111111-1111-1111-1111-111111111111"), "759100100101", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "Ron Cacique Añejo 0.75L", 8.50m, 12.00m, 30, 5 },
                    { new Guid("10000000-0000-0000-0000-000000000002"), true, new Guid("11111111-1111-1111-1111-111111111111"), "500028100202", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "Whisky Old Parr 12 Años 0.75L", 28.00m, 38.00m, 12, 3 },
                    { new Guid("10000000-0000-0000-0000-000000000003"), true, new Guid("22222222-2222-2222-2222-222222222222"), "759100200303", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "Coca-Cola 2 Litros", 1.60m, 2.50m, 50, 10 },
                    { new Guid("10000000-0000-0000-0000-000000000004"), true, new Guid("33333333-3333-3333-3333-333333333333"), "900249010001", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "Red Bull 250ml", 1.80m, 3.00m, 40, 8 },
                    { new Guid("10000000-0000-0000-0000-000000000005"), true, new Guid("44444444-4444-4444-4444-444444444444"), "759100400505", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "Doritos Queso Atrevido 150g", 1.20m, 2.00m, 25, 5 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "DetallesVenta");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DetallesVenta");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "DetallesVenta");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Categorias");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Ventas",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Ventas",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Usuarios",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "CategoriaId",
                table: "Productos",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Productos",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "VentaId",
                table: "DetallesVenta",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "ProductoId",
                table: "DetallesVenta",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "DetallesVenta",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Categorias",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, "Rones, Whiskeys, Anises y Vodka", "Licores" },
                    { 2, "Refrescos y bebidas carbonatadas", "Gaseosas" },
                    { 3, "Bebidas para rendimiento y energía", "Energizantes" },
                    { 4, "Snacks, papitas y frutos secos", "Pasabocas" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Activo", "Email", "NombreCompleto", "PasswordHash", "Rol" },
                values: new object[,]
                {
                    { 1, true, "admin@licoreria.com", "Administrador Principal", "admin123_hash", "Admin" },
                    { 2, true, "cajero1@licoreria.com", "Cajero Turno Mañana", "cajero123_hash", "Cajero" }
                });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "Activo", "CategoriaId", "CodigoBarras", "ImagenUrl", "Nombre", "PrecioCompraUSD", "PrecioVentaUSD", "Stock", "StockMinimo" },
                values: new object[,]
                {
                    { 1, true, 1, "759100100101", null, "Ron Cacique Añejo 0.75L", 8.50m, 12.00m, 30, 5 },
                    { 2, true, 1, "500028100202", null, "Whisky Old Parr 12 Años 0.75L", 28.00m, 38.00m, 12, 3 },
                    { 3, true, 2, "759100200303", null, "Coca-Cola 2 Litros", 1.60m, 2.50m, 50, 10 },
                    { 4, true, 3, "900249010001", null, "Red Bull 250ml", 1.80m, 3.00m, 40, 8 },
                    { 5, true, 4, "759100400505", null, "Doritos Queso Atrevido 150g", 1.20m, 2.00m, 25, 5 }
                });
        }
    }
}
