using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InicialPostgreSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Marcas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marcas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnidadesMedida",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Abreviatura = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesMedida", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreCompleto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Rol = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Sku = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CodigoBarras = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PrecioCompraUSD = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioVentaUSD = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Stock = table.Column<int>(type: "integer", nullable: false),
                    StockMinimo = table.Column<int>(type: "integer", nullable: false),
                    StockMaximo = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ImagenUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CategoriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    MarcaId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnidadMedidaId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Productos_Marcas_MarcaId",
                        column: x => x.MarcaId,
                        principalTable: "Marcas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Productos_UnidadesMedida_UnidadMedidaId",
                        column: x => x.UnidadMedidaId,
                        principalTable: "UnidadesMedida",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TasaCambio = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalUSD = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalBS = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MetodoPago = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ventas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesVenta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VentaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitarioUSD = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesVenta_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesVenta_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                table: "Marcas",
                columns: new[] { "Id", "Activo", "CreatedAt", "Descripcion", "IsDeleted", "LastModifiedAt", "Nombre" },
                values: new object[,]
                {
                    { new Guid("55555555-5555-5555-5555-555555555551"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), "Ron venezolano", false, null, "Cacique" },
                    { new Guid("55555555-5555-5555-5555-555555555552"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), "Whisky escocés", false, null, "Old Parr" },
                    { new Guid("55555555-5555-5555-5555-555555555553"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), "Bebidas gaseosas", false, null, "Coca-Cola" },
                    { new Guid("55555555-5555-5555-5555-555555555554"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), "Bebidas energizantes", false, null, "Red Bull" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), "Snacks y pasabocas", false, null, "Frito-Lay" }
                });

            migrationBuilder.InsertData(
                table: "UnidadesMedida",
                columns: new[] { "Id", "Abreviatura", "CreatedAt", "IsDeleted", "LastModifiedAt", "Nombre" },
                values: new object[,]
                {
                    { new Guid("66666666-6666-6666-6666-666666666661"), "BOT", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Botella" },
                    { new Guid("66666666-6666-6666-6666-666666666662"), "UND", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Unidad" },
                    { new Guid("66666666-6666-6666-6666-666666666663"), "TOB", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Tobo" },
                    { new Guid("66666666-6666-6666-6666-666666666664"), "PLA", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Plato" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Activo", "CreatedAt", "Email", "IsDeleted", "LastModifiedAt", "NombreCompleto", "PasswordHash", "Rol" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), "admin@licoreria.com", false, null, "Administrador Principal", "admin123_hash", "Administrador" },
                    { new Guid("20000000-0000-0000-0000-000000000002"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), "cajero1@licoreria.com", false, null, "Cajero Turno Mañana", "cajero123_hash", "Cajero" }
                });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "Activo", "CategoriaId", "CodigoBarras", "CreatedAt", "ImagenUrl", "IsDeleted", "LastModifiedAt", "MarcaId", "Nombre", "PrecioCompraUSD", "PrecioVentaUSD", "Sku", "Stock", "StockMaximo", "StockMinimo", "UnidadMedidaId" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), true, new Guid("11111111-1111-1111-1111-111111111111"), "759100100101", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, new Guid("55555555-5555-5555-5555-555555555551"), "Ron Cacique Añejo 0.75L", 8.50m, 12.00m, "LIC-RON-0001", 30, 60, 5, new Guid("66666666-6666-6666-6666-666666666661") },
                    { new Guid("10000000-0000-0000-0000-000000000002"), true, new Guid("11111111-1111-1111-1111-111111111111"), "500028100202", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, new Guid("55555555-5555-5555-5555-555555555552"), "Whisky Old Parr 12 Años 0.75L", 28.00m, 38.00m, "LIC-WHI-0002", 12, 24, 3, new Guid("66666666-6666-6666-6666-666666666661") },
                    { new Guid("10000000-0000-0000-0000-000000000003"), true, new Guid("22222222-2222-2222-2222-222222222222"), "759100200303", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, new Guid("55555555-5555-5555-5555-555555555553"), "Coca-Cola 2 Litros", 1.60m, 2.50m, "GAS-COC-0003", 50, 100, 10, new Guid("66666666-6666-6666-6666-666666666661") },
                    { new Guid("10000000-0000-0000-0000-000000000004"), true, new Guid("33333333-3333-3333-3333-333333333333"), "900249010001", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, new Guid("55555555-5555-5555-5555-555555555554"), "Red Bull 250ml", 1.80m, 3.00m, "ENE-RED-0004", 40, 80, 8, new Guid("66666666-6666-6666-6666-666666666662") },
                    { new Guid("10000000-0000-0000-0000-000000000005"), true, new Guid("44444444-4444-4444-4444-444444444444"), "759100400505", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, new Guid("55555555-5555-5555-5555-555555555555"), "Doritos Queso Atrevido 150g", 1.20m, 2.00m, "PAS-DOR-0005", 25, 50, 5, new Guid("66666666-6666-6666-6666-666666666662") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesVenta_ProductoId",
                table: "DetallesVenta",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesVenta_VentaId",
                table: "DetallesVenta",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaId",
                table: "Productos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_MarcaId",
                table: "Productos",
                column: "MarcaId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Sku",
                table: "Productos",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_UnidadMedidaId",
                table: "Productos",
                column: "UnidadMedidaId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_UsuarioId",
                table: "Ventas",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesVenta");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Ventas");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Marcas");

            migrationBuilder.DropTable(
                name: "UnidadesMedida");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
