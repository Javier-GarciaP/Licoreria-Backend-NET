using Licoreria.Application.Dtos;
using Licoreria.Application.Reportes;
using Licoreria.Domain.Enums;

namespace Licoreria.UnitTests;

/// <summary>Cada mapeador produce las columnas, filas y totales esperados, con una celda por columna.</summary>
public sealed class MapeadoresReporteTests
{
    private static readonly DateTime Desde = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Hasta = new(2026, 10, 13, 0, 0, 0, DateTimeKind.Utc);

    private static void AssertEstructura(IReadOnlyList<TablaReporte> tablas)
    {
        Assert.NotEmpty(tablas);
        foreach (var tabla in tablas)
        {
            Assert.NotEmpty(tabla.Columnas);
            Assert.All(tabla.Filas, fila => Assert.Equal(tabla.Columnas.Count, fila.Count));
            if (tabla.Totales is not null)
            {
                Assert.Equal(tabla.Columnas.Count, tabla.Totales.Count);
            }
        }
    }

    [Fact]
    public void Ventas_ProduceResumenPorDiaUsuarioYMetodoConTotales()
    {
        var reporte = new ReporteVentasDto(
            Desde, Hasta, 250m, 9125m, 10, 25m,
            [new VentaPorDiaDto(Desde, 100m, 4), new VentaPorDiaDto(Desde.AddDays(1), 150m, 6)],
            [new VentaPorUsuarioDto(Guid.NewGuid(), "Ana", 250m, 10)],
            [new VentaPorMetodoPagoDto(Guid.NewGuid(), "Efectivo", Moneda.USD, 150m, 6),
             new VentaPorMetodoPagoDto(Guid.NewGuid(), "Pago móvil", Moneda.BS, 3650m, 4)]);

        var tablas = MapeadoresReporte.Ventas(reporte);

        AssertEstructura(tablas);
        Assert.Equal(["Resumen", "Por día", "Por usuario", "Por método de pago"], tablas.Select(t => t.Titulo));
        var porDia = tablas[1];
        Assert.Equal(2, porDia.Filas.Count);
        Assert.Equal(Desde, porDia.Filas[0][0]);
        Assert.Equal(10, porDia.Totales![1]);
        Assert.Equal(250m, porDia.Totales[2]);
        Assert.Equal("BS", tablas[3].Filas[1][1]);
    }

    [Fact]
    public void Productos_SeparaElTopDeLosProductosSinMovimiento()
    {
        var top = new ProductoVendidoDto(Guid.NewGuid(), "RON", "Ron", "750 ml", 30m, 90m, 100m, 100m, 10m, 3m, EstadoRotacion.Calculada);
        var quieto = new ProductoVendidoDto(Guid.NewGuid(), "TEQ", "Tequila", "750 ml", 0m, 0m, 0m, 0m, 12m, 0m, EstadoRotacion.SinMovimiento);
        var reporte = new ReporteProductosDto(Desde, Hasta, OrdenProductos.Unidades, 10, 30m, 90m, [top], [quieto]);

        var tablas = MapeadoresReporte.Productos(reporte);

        AssertEstructura(tablas);
        Assert.Single(tablas[0].Filas);
        Assert.Equal("Calculada", tablas[0].Filas[0][9]);
        Assert.Equal(30m, tablas[0].Totales![3]);
        Assert.Equal("Sin movimiento", tablas[1].Filas.Single()[9]);
    }

    [Fact]
    public void Productos_RotacionNoCalculable_VaComoCeldaNulaConSuEstado()
    {
        var fila = new ProductoVendidoDto(Guid.NewGuid(), "RON", "Ron", "750 ml", 5m, 20m, 100m, 100m, 0m, null, EstadoRotacion.NoCalculable);

        var tabla = MapeadoresReporte.Productos(
            new ReporteProductosDto(Desde, Hasta, OrdenProductos.Unidades, 10, 5m, 20m, [fila], []))[0];

        Assert.Null(tabla.Filas[0][8]);
        Assert.Equal("No calculable", tabla.Filas[0][9]);
    }

    [Fact]
    public void Inventario_TotalizaCostoYValorDeVenta()
    {
        var reporte = new InventarioValorizadoDto(2, 15m, 40m, 80m, 1,
        [
            new InventarioValorizadoItemDto(Guid.NewGuid(), "A", "Ron", "750", 10m, 2m, 20m, 40m, false),
            new InventarioValorizadoItemDto(Guid.NewGuid(), "B", "Vodka", "750", 5m, 4m, 20m, 40m, true)
        ]);

        var tabla = MapeadoresReporte.Inventario(reporte).Single();

        AssertEstructura([tabla]);
        Assert.Equal(2, tabla.Filas.Count);
        Assert.Equal("Sí", tabla.Filas[1][7]);
        Assert.Equal(40m, tabla.Totales![5]);
        Assert.Equal(80m, tabla.Totales[6]);
        Assert.Equal(15m, tabla.Totales[3]);
    }

    [Fact]
    public void Compras_ProduceUnaFilaConElResumenDelPeriodo()
    {
        var tabla = MapeadoresReporte.Compras(new ReporteComprasDto(Desde, Hasta, 3, 500m, 200m, 300m, 2)).Single();

        AssertEstructura([tabla]);
        Assert.Equal([3, 500m, 200m, 300m, 2], tabla.Filas.Single());
    }

    [Fact]
    public void Propinas_TotalizaPropinasYVentas()
    {
        var reporte = new ReportePropinasDto(Desde, Hasta, 12m,
            [new PropinaPorUsuarioDto(Guid.NewGuid(), "Ana", 4, 7m), new PropinaPorUsuarioDto(Guid.NewGuid(), "Luis", 2, 5m)]);

        var tabla = MapeadoresReporte.Propinas(reporte).Single();

        AssertEstructura([tabla]);
        Assert.Equal(6, tabla.Totales![1]);
        Assert.Equal(12m, tabla.Totales[2]);
    }

    [Fact]
    public void Mesoneros_UnaFilaPorMesoneroConTodosSusIndicadores()
    {
        var reporte = new ReporteMesonerosDto(Desde, Hasta, 130m,
        [
            new MesoneroDto(Guid.NewGuid(), "Ana", 5, 18, 2, 4, 100m, 25m, 10m, 180m),
            new MesoneroDto(null, "Sin asignar", 1, 3, 0, 1, 30m, 30m, 0m, null)
        ]);

        var tabla = MapeadoresReporte.Mesoneros(reporte).Single();

        AssertEstructura([tabla]);
        Assert.Equal(2, tabla.Filas.Count);
        Assert.Equal("Ana", tabla.Filas[0][0]);
        Assert.Equal(180m, tabla.Filas[0][8]);
        Assert.Null(tabla.Filas[1][8]);
        Assert.Equal(130m, tabla.Totales![5]);
    }

    [Fact]
    public void Caja_ResumenYTurnosConDescuadreTotal()
    {
        var t1 = new TurnoCajaDto(Guid.NewGuid(), Guid.NewGuid(), "Ana", Desde, Desde.AddHours(8), 100m, 95m, -5m);
        var reporte = new ReporteCajaDto(Desde, Hasta, 1, -5m, -5m, -5m, [t1]);

        var tablas = MapeadoresReporte.Caja(reporte);

        AssertEstructura(tablas);
        Assert.Equal(-5m, tablas[0].Filas.Single()[1]);
        Assert.Equal(-5m, tablas[1].Totales![5]);
        Assert.Equal(95m, tablas[1].Filas[0][4]);
    }

    [Fact]
    public void Ocupacion_ResumenYZonas()
    {
        var zona = new OcupacionZonaDto(Guid.NewGuid(), "VIP", 8, 2, 25m, 10, 1, 1.25m, 90m);
        var reporte = new ReporteOcupacionDto(Desde, Hasta, Hasta, 8, 2, 25m, 10, 1, 1.25m, 90m, [zona]);

        var tablas = MapeadoresReporte.Ocupacion(reporte);

        AssertEstructura(tablas);
        Assert.Equal(25m, tablas[0].Filas.Single()[2]);
        Assert.Equal("VIP", tablas[1].Filas.Single()[0]);
    }

    [Fact]
    public void MermasVsVentas_ResumenYMotivosConTotal()
    {
        var reporte = new ReporteMermasVsVentasDto(Desde, Hasta, 200m, 15m, 7.5m,
        [
            new ComparativoMermaDto(MotivoMerma.Danado, 2, 3m, 10m, 5m),
            new ComparativoMermaDto(MotivoMerma.Vencido, 1, 1m, 5m, 2.5m)
        ]);

        var tablas = MapeadoresReporte.MermasVsVentas(reporte);

        AssertEstructura(tablas);
        Assert.Equal("Danado", tablas[1].Filas[0][0]);
        Assert.Equal(15m, tablas[1].Totales![3]);
        Assert.Equal(3, tablas[1].Totales![1]);
    }

    [Fact]
    public void Auditoria_UnaFilaPorRegistro_ConSistemaCuandoNoHayUsuario()
    {
        var registros = new[]
        {
            new AuditLogDto(Guid.NewGuid(), null, null, "login", "Sesion", null, "{}", "127.0.0.1", Desde, "Acceso")
        };

        var tabla = MapeadoresReporte.Auditoria(registros).Single();

        AssertEstructura([tabla]);
        Assert.Equal("Sistema", tabla.Filas.Single()[1]);
        Assert.Equal(Desde, tabla.Filas.Single()[0]);
    }
}
