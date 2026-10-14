using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licoreria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestriccionesConcurrencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Verificación previa (3.2): si los datos ya violan una restricción nueva, la migración
            // se detiene con un mensaje claro en lugar de fallar a mitad con un error de índice.
            migrationBuilder.Sql(@"
DO $$
DECLARE
    n integer;
BEGIN
    SELECT COUNT(*) INTO n FROM (
        SELECT ""Numero"" FROM comprobantes_fiscales GROUP BY ""Numero"" HAVING COUNT(*) > 1) d;
    IF n > 0 THEN
        RAISE EXCEPTION 'No se puede aplicar RestriccionesConcurrencia: % número(s) de comprobante duplicado(s). Corrija comprobantes_fiscales.""Numero"" y reintente.', n;
    END IF;

    SELECT COUNT(*) INTO n FROM sesiones_caja WHERE ""Estado"" = 'Abierta' AND ""IsDeleted"" = false;
    IF n > 1 THEN
        RAISE EXCEPTION 'No se puede aplicar RestriccionesConcurrencia: hay % sesiones de caja abiertas. Cierre todas menos una (sesiones_caja) y reintente.', n;
    END IF;

    SELECT COUNT(*) INTO n FROM (
        SELECT ""MesaId"" FROM sesiones_mesa
        WHERE ""MesaId"" IS NOT NULL AND ""CerradaEn"" IS NULL AND ""IsDeleted"" = false
        GROUP BY ""MesaId"" HAVING COUNT(*) > 1) d;
    IF n > 0 THEN
        RAISE EXCEPTION 'No se puede aplicar RestriccionesConcurrencia: % mesa(s) con más de una sesión abierta (sesiones_mesa). Cierre las repetidas y reintente.', n;
    END IF;

    SELECT COUNT(*) INTO n FROM (
        SELECT ""CuentaId"" FROM ventas
        WHERE ""CuentaId"" IS NOT NULL AND ""Estado"" <> 'Anulada' AND ""IsDeleted"" = false
        GROUP BY ""CuentaId"" HAVING COUNT(*) > 1) d;
    IF n > 0 THEN
        RAISE EXCEPTION 'No se puede aplicar RestriccionesConcurrencia: % cuenta(s) con más de una venta vigente (ventas). Anule las repetidas y reintente.', n;
    END IF;

    SELECT COUNT(*) INTO n FROM stock_producto
    WHERE ""Cantidad"" < 0 OR ""CantidadReservada"" < 0 OR ""CantidadReservada"" > ""Cantidad"";
    IF n > 0 THEN
        RAISE EXCEPTION 'No se puede aplicar RestriccionesConcurrencia: % fila(s) de stock_producto con existencia negativa o reserva inválida. Corrija con un ajuste de inventario y reintente.', n;
    END IF;
END $$;");

            migrationBuilder.CreateSequence(
                name: "comprobante_numero_seq");

            // La secuencia continúa después del mayor número emitido (formato F-00000001).
            migrationBuilder.Sql(@"
DO $$
DECLARE
    maximo bigint;
BEGIN
    SELECT COALESCE(MAX(NULLIF(regexp_replace(""Numero"", '\D', '', 'g'), '')::bigint), 0)
    INTO maximo FROM comprobantes_fiscales;
    IF maximo > 0 THEN
        PERFORM setval('comprobante_numero_seq', maximo, true);
    END IF;
END $$;");

            migrationBuilder.CreateIndex(
                name: "ux_ventas_por_cuenta",
                table: "ventas",
                column: "CuentaId",
                unique: true,
                filter: "\"CuentaId\" IS NOT NULL AND \"Estado\" <> 'Anulada' AND \"IsDeleted\" = false");

            migrationBuilder.AddCheckConstraint(
                name: "ck_stock_cantidad_no_negativa",
                table: "stock_producto",
                sql: "\"Cantidad\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_stock_reservada_no_negativa",
                table: "stock_producto",
                sql: "\"CantidadReservada\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_stock_reservada_no_supera_cantidad",
                table: "stock_producto",
                sql: "\"CantidadReservada\" <= \"Cantidad\"");

            migrationBuilder.CreateIndex(
                name: "ux_sesiones_mesa_abierta_por_mesa",
                table: "sesiones_mesa",
                column: "MesaId",
                unique: true,
                filter: "\"MesaId\" IS NOT NULL AND \"CerradaEn\" IS NULL AND \"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "ux_sesiones_caja_abierta",
                table: "sesiones_caja",
                column: "Estado",
                unique: true,
                filter: "\"Estado\" = 'Abierta' AND \"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "ux_comprobantes_numero",
                table: "comprobantes_fiscales",
                column: "Numero",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_ventas_por_cuenta",
                table: "ventas");

            migrationBuilder.DropCheckConstraint(
                name: "ck_stock_cantidad_no_negativa",
                table: "stock_producto");

            migrationBuilder.DropCheckConstraint(
                name: "ck_stock_reservada_no_negativa",
                table: "stock_producto");

            migrationBuilder.DropCheckConstraint(
                name: "ck_stock_reservada_no_supera_cantidad",
                table: "stock_producto");

            migrationBuilder.DropIndex(
                name: "ux_sesiones_mesa_abierta_por_mesa",
                table: "sesiones_mesa");

            migrationBuilder.DropIndex(
                name: "ux_sesiones_caja_abierta",
                table: "sesiones_caja");

            migrationBuilder.DropIndex(
                name: "ux_comprobantes_numero",
                table: "comprobantes_fiscales");

            migrationBuilder.DropSequence(
                name: "comprobante_numero_seq");
        }
    }
}
