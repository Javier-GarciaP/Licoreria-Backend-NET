using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CatalogoVariantes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_productos_unidades_medida_UnidadMedidaId",
                table: "productos");

            migrationBuilder.DropIndex(
                name: "IX_productos_Sku",
                table: "productos");

            migrationBuilder.DropIndex(
                name: "IX_productos_UnidadMedidaId",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "CodigoBarras",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "PrecioCompraUSD",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "PrecioVentaUSD",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "Stock",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "StockMaximo",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "StockMinimo",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "UnidadMedidaId",
                table: "productos");

            migrationBuilder.RenameColumn(
                name: "Sku",
                table: "productos",
                newName: "Tipo");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "productos",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<Guid>(
                name: "MarcaId",
                table: "productos",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<decimal>(
                name: "GradoAlcoholico",
                table: "productos",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ImpuestoId",
                table: "productos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "categorias",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "CategoriaPadreId",
                table: "categorias",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "impuestos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Porcentaje = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_impuestos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "listas_precio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EsPredeterminada = table.Column<bool>(type: "boolean", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_listas_precio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "producto_variantes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PrecioCompraUSD = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioVentaUSD = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    UnidadMedidaId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producto_variantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_producto_variantes_productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_producto_variantes_unidades_medida_UnidadMedidaId",
                        column: x => x.UnidadMedidaId,
                        principalTable: "unidades_medida",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "codigos_barras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VarianteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Principal = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_codigos_barras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_codigos_barras_producto_variantes_VarianteId",
                        column: x => x.VarianteId,
                        principalTable: "producto_variantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "precios_producto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VarianteId = table.Column<Guid>(type: "uuid", nullable: false),
                    ListaPrecioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Moneda = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Precio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_precios_producto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_precios_producto_listas_precio_ListaPrecioId",
                        column: x => x.ListaPrecioId,
                        principalTable: "listas_precio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_precios_producto_producto_variantes_VarianteId",
                        column: x => x.VarianteId,
                        principalTable: "producto_variantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recetas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    VarianteInsumoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cantidad = table.Column<decimal>(type: "numeric(14,3)", precision: 14, scale: 3, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recetas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recetas_producto_variantes_VarianteInsumoId",
                        column: x => x.VarianteInsumoId,
                        principalTable: "producto_variantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_recetas_productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "categorias",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "Activo", "CategoriaPadreId" },
                values: new object[] { true, null });

            migrationBuilder.UpdateData(
                table: "categorias",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "Activo", "CategoriaPadreId" },
                values: new object[] { true, null });

            migrationBuilder.UpdateData(
                table: "categorias",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "Activo", "CategoriaPadreId" },
                values: new object[] { true, null });

            migrationBuilder.UpdateData(
                table: "categorias",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "Activo", "CategoriaPadreId" },
                values: new object[] { true, null });

            migrationBuilder.InsertData(
                table: "impuestos",
                columns: new[] { "Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Nombre", "Porcentaje", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("77777777-7777-7777-7777-777777777771"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "IVA", 16.00m, null },
                    { new Guid("77777777-7777-7777-7777-777777777772"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "IGTF", 3.00m, null }
                });

            migrationBuilder.InsertData(
                table: "listas_precio",
                columns: new[] { "Id", "Activo", "CreatedAt", "CreatedBy", "Descripcion", "EsPredeterminada", "IsDeleted", "LastModifiedAt", "Nombre", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("88888888-8888-8888-8888-888888888881"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, "Precio de venta al público.", true, false, null, "Detal", null },
                    { new Guid("88888888-8888-8888-8888-888888888882"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, "Precio para compras al mayor.", false, false, null, "Mayorista", null }
                });

            migrationBuilder.InsertData(
                table: "producto_variantes",
                columns: new[] { "Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Nombre", "PrecioCompraUSD", "PrecioVentaUSD", "ProductoId", "Sku", "UnidadMedidaId", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("30000000-0000-0000-0000-000000000001"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "Botella 0.75L", 8.50m, 12.00m, new Guid("10000000-0000-0000-0000-000000000001"), "LIC-RON-0001", new Guid("66666666-6666-6666-6666-666666666661"), null },
                    { new Guid("30000000-0000-0000-0000-000000000002"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "Botella 0.75L", 28.00m, 38.00m, new Guid("10000000-0000-0000-0000-000000000002"), "LIC-WHI-0002", new Guid("66666666-6666-6666-6666-666666666661"), null },
                    { new Guid("30000000-0000-0000-0000-000000000003"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "Botella 2L", 1.60m, 2.50m, new Guid("10000000-0000-0000-0000-000000000003"), "GAS-COC-0003", new Guid("66666666-6666-6666-6666-666666666661"), null },
                    { new Guid("30000000-0000-0000-0000-000000000004"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "Lata 250ml", 1.80m, 3.00m, new Guid("10000000-0000-0000-0000-000000000004"), "ENE-RED-0004", new Guid("66666666-6666-6666-6666-666666666662"), null },
                    { new Guid("30000000-0000-0000-0000-000000000005"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "Paquete 150g", 1.20m, 2.00m, new Guid("10000000-0000-0000-0000-000000000005"), "PAS-DOR-0005", new Guid("66666666-6666-6666-6666-666666666662"), null }
                });

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "Descripcion", "GradoAlcoholico", "ImpuestoId", "Nombre", "Tipo" },
                values: new object[] { "Ron añejo venezolano.", 40.00m, new Guid("77777777-7777-7777-7777-777777777771"), "Ron Cacique Añejo", "Simple" });

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "Descripcion", "GradoAlcoholico", "ImpuestoId", "Nombre", "Tipo" },
                values: new object[] { "Whisky escocés de 12 años.", 40.00m, new Guid("77777777-7777-7777-7777-777777777771"), "Whisky Old Parr 12 Años", "Simple" });

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "Descripcion", "GradoAlcoholico", "ImpuestoId", "Nombre", "Tipo" },
                values: new object[] { "Refresco de cola.", null, new Guid("77777777-7777-7777-7777-777777777771"), "Coca-Cola", "Simple" });

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "Descripcion", "GradoAlcoholico", "ImpuestoId", "Nombre", "Tipo" },
                values: new object[] { "Bebida energizante.", null, new Guid("77777777-7777-7777-7777-777777777771"), "Red Bull", "Simple" });

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "Descripcion", "GradoAlcoholico", "ImpuestoId", "Nombre", "Tipo" },
                values: new object[] { "Snack de maíz sabor queso.", null, new Guid("77777777-7777-7777-7777-777777777771"), "Doritos Queso Atrevido", "Simple" });

            migrationBuilder.InsertData(
                table: "codigos_barras",
                columns: new[] { "Id", "Codigo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Principal", "UpdatedBy", "VarianteId" },
                values: new object[,]
                {
                    { new Guid("40000000-0000-0000-0000-000000000001"), "759100100101", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, true, null, new Guid("30000000-0000-0000-0000-000000000001") },
                    { new Guid("40000000-0000-0000-0000-000000000002"), "500028100202", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, true, null, new Guid("30000000-0000-0000-0000-000000000002") },
                    { new Guid("40000000-0000-0000-0000-000000000003"), "759100200303", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, true, null, new Guid("30000000-0000-0000-0000-000000000003") },
                    { new Guid("40000000-0000-0000-0000-000000000004"), "900249010001", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, true, null, new Guid("30000000-0000-0000-0000-000000000004") },
                    { new Guid("40000000-0000-0000-0000-000000000005"), "759100400505", new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, true, null, new Guid("30000000-0000-0000-0000-000000000005") }
                });

            migrationBuilder.InsertData(
                table: "precios_producto",
                columns: new[] { "Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "ListaPrecioId", "Moneda", "Precio", "UpdatedBy", "VarianteId" },
                values: new object[,]
                {
                    { new Guid("90000000-0000-0000-0000-000000000001"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, new Guid("88888888-8888-8888-8888-888888888881"), "USD", 12.00m, null, new Guid("30000000-0000-0000-0000-000000000001") },
                    { new Guid("90000000-0000-0000-0000-000000000002"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, new Guid("88888888-8888-8888-8888-888888888881"), "USD", 38.00m, null, new Guid("30000000-0000-0000-0000-000000000002") },
                    { new Guid("90000000-0000-0000-0000-000000000003"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, new Guid("88888888-8888-8888-8888-888888888881"), "USD", 2.50m, null, new Guid("30000000-0000-0000-0000-000000000003") },
                    { new Guid("90000000-0000-0000-0000-000000000004"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, new Guid("88888888-8888-8888-8888-888888888881"), "USD", 3.00m, null, new Guid("30000000-0000-0000-0000-000000000004") },
                    { new Guid("90000000-0000-0000-0000-000000000005"), true, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, new Guid("88888888-8888-8888-8888-888888888881"), "USD", 2.00m, null, new Guid("30000000-0000-0000-0000-000000000005") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_productos_ImpuestoId",
                table: "productos",
                column: "ImpuestoId");

            migrationBuilder.CreateIndex(
                name: "IX_categorias_CategoriaPadreId",
                table: "categorias",
                column: "CategoriaPadreId");

            migrationBuilder.CreateIndex(
                name: "IX_codigos_barras_Codigo",
                table: "codigos_barras",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_codigos_barras_VarianteId",
                table: "codigos_barras",
                column: "VarianteId");

            migrationBuilder.CreateIndex(
                name: "IX_precios_producto_ListaPrecioId",
                table: "precios_producto",
                column: "ListaPrecioId");

            migrationBuilder.CreateIndex(
                name: "IX_precios_producto_VarianteId_ListaPrecioId_Moneda",
                table: "precios_producto",
                columns: new[] { "VarianteId", "ListaPrecioId", "Moneda" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_producto_variantes_ProductoId",
                table: "producto_variantes",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_producto_variantes_Sku",
                table: "producto_variantes",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_producto_variantes_UnidadMedidaId",
                table: "producto_variantes",
                column: "UnidadMedidaId");

            migrationBuilder.CreateIndex(
                name: "IX_recetas_ProductoId_VarianteInsumoId",
                table: "recetas",
                columns: new[] { "ProductoId", "VarianteInsumoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recetas_VarianteInsumoId",
                table: "recetas",
                column: "VarianteInsumoId");

            migrationBuilder.AddForeignKey(
                name: "FK_categorias_categorias_CategoriaPadreId",
                table: "categorias",
                column: "CategoriaPadreId",
                principalTable: "categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_productos_impuestos_ImpuestoId",
                table: "productos",
                column: "ImpuestoId",
                principalTable: "impuestos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_categorias_categorias_CategoriaPadreId",
                table: "categorias");

            migrationBuilder.DropForeignKey(
                name: "FK_productos_impuestos_ImpuestoId",
                table: "productos");

            migrationBuilder.DropTable(
                name: "codigos_barras");

            migrationBuilder.DropTable(
                name: "impuestos");

            migrationBuilder.DropTable(
                name: "precios_producto");

            migrationBuilder.DropTable(
                name: "recetas");

            migrationBuilder.DropTable(
                name: "listas_precio");

            migrationBuilder.DropTable(
                name: "producto_variantes");

            migrationBuilder.DropIndex(
                name: "IX_productos_ImpuestoId",
                table: "productos");

            migrationBuilder.DropIndex(
                name: "IX_categorias_CategoriaPadreId",
                table: "categorias");

            migrationBuilder.DropColumn(
                name: "GradoAlcoholico",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "ImpuestoId",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "categorias");

            migrationBuilder.DropColumn(
                name: "CategoriaPadreId",
                table: "categorias");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "productos",
                newName: "Sku");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "productos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<Guid>(
                name: "MarcaId",
                table: "productos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoBarras",
                table: "productos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioCompraUSD",
                table: "productos",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioVentaUSD",
                table: "productos",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "productos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockMaximo",
                table: "productos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockMinimo",
                table: "productos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "UnidadMedidaId",
                table: "productos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                columns: new[] { "CodigoBarras", "Descripcion", "Nombre", "PrecioCompraUSD", "PrecioVentaUSD", "Sku", "Stock", "StockMaximo", "StockMinimo", "UnidadMedidaId" },
                values: new object[] { "759100100101", "Ron añejo venezolano de 0.75 litros.", "Ron Cacique Añejo 0.75L", 8.50m, 12.00m, "LIC-RON-0001", 30, 60, 5, new Guid("66666666-6666-6666-6666-666666666661") });

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                columns: new[] { "CodigoBarras", "Descripcion", "Nombre", "PrecioCompraUSD", "PrecioVentaUSD", "Sku", "Stock", "StockMaximo", "StockMinimo", "UnidadMedidaId" },
                values: new object[] { "500028100202", "Whisky escocés de 12 años, 0.75 litros.", "Whisky Old Parr 12 Años 0.75L", 28.00m, 38.00m, "LIC-WHI-0002", 12, 24, 3, new Guid("66666666-6666-6666-6666-666666666661") });

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                columns: new[] { "CodigoBarras", "Descripcion", "Nombre", "PrecioCompraUSD", "PrecioVentaUSD", "Sku", "Stock", "StockMaximo", "StockMinimo", "UnidadMedidaId" },
                values: new object[] { "759100200303", "Refresco de cola de 2 litros.", "Coca-Cola 2 Litros", 1.60m, 2.50m, "GAS-COC-0003", 50, 100, 10, new Guid("66666666-6666-6666-6666-666666666661") });

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                columns: new[] { "CodigoBarras", "Descripcion", "Nombre", "PrecioCompraUSD", "PrecioVentaUSD", "Sku", "Stock", "StockMaximo", "StockMinimo", "UnidadMedidaId" },
                values: new object[] { "900249010001", "Bebida energizante de 250 ml.", "Red Bull 250ml", 1.80m, 3.00m, "ENE-RED-0004", 40, 80, 8, new Guid("66666666-6666-6666-6666-666666666662") });

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                columns: new[] { "CodigoBarras", "Descripcion", "Nombre", "PrecioCompraUSD", "PrecioVentaUSD", "Sku", "Stock", "StockMaximo", "StockMinimo", "UnidadMedidaId" },
                values: new object[] { "759100400505", "Snack de maíz sabor queso, 150 g.", "Doritos Queso Atrevido 150g", 1.20m, 2.00m, "PAS-DOR-0005", 25, 50, 5, new Guid("66666666-6666-6666-6666-666666666662") });

            migrationBuilder.CreateIndex(
                name: "IX_productos_Sku",
                table: "productos",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_productos_UnidadMedidaId",
                table: "productos",
                column: "UnidadMedidaId");

            migrationBuilder.AddForeignKey(
                name: "FK_productos_unidades_medida_UnidadMedidaId",
                table: "productos",
                column: "UnidadMedidaId",
                principalTable: "unidades_medida",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
