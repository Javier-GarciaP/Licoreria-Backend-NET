using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Enums;
using Licoreria.Domain.Services;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>Reglas de <see cref="ServicioReportes"/> aisladas del repositorio con Moq.</summary>
public sealed class ServicioReportesTests
{
    private static readonly DateTime Ahora = new(2026, 10, 14, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IReportesRepository> _repo = new();
    private readonly Mock<IRelojSistema> _reloj = new();

    public ServicioReportesTests()
    {
        _reloj.SetupGet(r => r.UtcNow).Returns(Ahora);
    }

    private ServicioReportes CrearServicio()
        => new(_repo.Object, _reloj.Object, new EvaluadorSaludStock());

    // ---------- Validación del periodo (1.1) ----------

    [Fact]
    public async Task Ventas_SinPeriodo_UsaLosUltimos30Dias()
    {
        DateTime? desdeUsado = null;
        DateTime? hastaUsado = null;
        _repo.Setup(r => r.ObtenerVentasAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Callback<DateTime, DateTime, CancellationToken>((d, h, _) => { desdeUsado = d; hastaUsado = h; })
            .ReturnsAsync(VentasVacias());

        await CrearServicio().ObtenerVentasAsync();

        Assert.Equal(Ahora, hastaUsado);
        Assert.Equal(Ahora.AddDays(-30), desdeUsado);
    }

    [Fact]
    public async Task Ventas_FechasSinHora_IncluyenElDiaFinalCompletoYSeInterpretanEnUtc()
    {
        DateTime? desdeUsado = null;
        DateTime? hastaUsado = null;
        _repo.Setup(r => r.ObtenerVentasAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Callback<DateTime, DateTime, CancellationToken>((d, h, _) => { desdeUsado = d; hastaUsado = h; })
            .ReturnsAsync(VentasVacias());

        await CrearServicio().ObtenerVentasAsync(new DateTime(2026, 10, 1), new DateTime(2026, 10, 13));

        Assert.Equal(DateTimeKind.Utc, desdeUsado!.Value.Kind);
        Assert.Equal(new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc), desdeUsado);
        Assert.Equal(DateTimeKind.Utc, hastaUsado!.Value.Kind);
        Assert.Equal(new DateTime(2026, 10, 14, 0, 0, 0, DateTimeKind.Utc).AddTicks(-1), hastaUsado);
    }

    [Fact]
    public async Task Ventas_RangoInvertido_LanzaArgumentException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            CrearServicio().ObtenerVentasAsync(Ahora, Ahora.AddDays(-1)));

        Assert.Contains("fecha inicial", ex.Message);
        _repo.Verify(r => r.ObtenerVentasAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Ventas_RangoMayorA366Dias_LanzaArgumentException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            CrearServicio().ObtenerVentasAsync(Ahora.AddDays(-367), Ahora));

        Assert.Contains("366", ex.Message);
    }

    [Fact]
    public async Task Ventas_RangoDe366Dias_EsValido()
    {
        _repo.Setup(r => r.ObtenerVentasAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VentasVacias());

        var ventas = await CrearServicio().ObtenerVentasAsync(Ahora.AddDays(-366), Ahora);

        Assert.NotNull(ventas);
    }

    [Fact]
    public async Task Propinas_RangoInvertido_LanzaArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            CrearServicio().ObtenerPropinasAsync(Ahora, Ahora.AddDays(-1)));
    }

    [Fact]
    public async Task Compras_RangoInvertido_LanzaArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            CrearServicio().ObtenerComprasAsync(Ahora, Ahora.AddDays(-1)));
    }

    [Fact]
    public async Task Heatmap_RangoInvertido_LanzaArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            CrearServicio().ObtenerHeatmapAsync(Ahora, Ahora.AddDays(-1)));
    }

    [Fact]
    public async Task MermasVsVentas_RangoInvertido_LanzaArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            CrearServicio().ObtenerMermasVsVentasAsync(Ahora, Ahora.AddDays(-1)));
    }

    // ---------- Métodos existentes (1.2) ----------

    [Fact]
    public async Task Ventas_DelegaEnElRepositorioConElRangoIndicado()
    {
        var desde = Ahora.AddDays(-7);
        var esperado = VentasVacias() with { TotalUSD = 250m, Cantidad = 10, TicketPromedioUSD = 25m };
        _repo.Setup(r => r.ObtenerVentasAsync(desde, Ahora, It.IsAny<CancellationToken>())).ReturnsAsync(esperado);

        var ventas = await CrearServicio().ObtenerVentasAsync(desde, Ahora);

        Assert.Same(esperado, ventas);
    }

    [Fact]
    public async Task Propinas_FiltraPorUsuario()
    {
        var usuario = Guid.NewGuid();
        var esperado = new ReportePropinasDto(Ahora.AddDays(-1), Ahora, 5m, []);
        _repo.Setup(r => r.ObtenerPropinasAsync(Ahora.AddDays(-1), Ahora, usuario, It.IsAny<CancellationToken>()))
            .ReturnsAsync(esperado);

        var propinas = await CrearServicio().ObtenerPropinasAsync(Ahora.AddDays(-1), Ahora, usuario);

        Assert.Same(esperado, propinas);
    }

    [Fact]
    public async Task Compras_DelegaEnElRepositorio()
    {
        var esperado = new ReporteComprasDto(Ahora.AddDays(-1), Ahora, 2, 100m, 40m, 60m, 1);
        _repo.Setup(r => r.ObtenerComprasAsync(Ahora.AddDays(-1), Ahora, It.IsAny<CancellationToken>())).ReturnsAsync(esperado);

        Assert.Same(esperado, await CrearServicio().ObtenerComprasAsync(Ahora.AddDays(-1), Ahora));
    }

    [Fact]
    public async Task Heatmap_DelegaEnElRepositorio()
    {
        var esperado = new ReporteHeatmapDto(Ahora.AddDays(-1), Ahora, []);
        _repo.Setup(r => r.ObtenerHeatmapAsync(Ahora.AddDays(-1), Ahora, It.IsAny<CancellationToken>())).ReturnsAsync(esperado);

        Assert.Same(esperado, await CrearServicio().ObtenerHeatmapAsync(Ahora.AddDays(-1), Ahora));
    }

    [Fact]
    public async Task InventarioValorizado_DelegaEnElRepositorio()
    {
        var esperado = new InventarioValorizadoDto(0, 0m, 0m, 0m, 0, []);
        _repo.Setup(r => r.ObtenerInventarioValorizadoAsync(It.IsAny<CancellationToken>())).ReturnsAsync(esperado);

        Assert.Same(esperado, await CrearServicio().ObtenerInventarioValorizadoAsync());
    }

    [Fact]
    public async Task DiagnosticoInventario_ClasificaYCuentaPorEstado()
    {
        _repo.Setup(r => r.ObtenerStockSaludAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[]
        {
            new StockSaludCrudoDto(Guid.NewGuid(), "A", "Ron", "750", 0m, 5m, 20m),
            new StockSaludCrudoDto(Guid.NewGuid(), "B", "Vodka", "750", 5m, 5m, 20m),
            new StockSaludCrudoDto(Guid.NewGuid(), "C", "Whisky", "750", 15m, 5m, 20m)
        });

        var diag = await CrearServicio().ObtenerDiagnosticoInventarioAsync();

        Assert.Equal(3, diag.Total);
        Assert.Equal(1, diag.SinStock);
        Assert.Equal(1, diag.RiesgoCritico);
        Assert.Equal(1, diag.Optimo);
        Assert.Equal(["Optimo", "RiesgoCritico", "SinStock"], diag.Items.Select(i => i.Estado));
    }

    [Fact]
    public async Task MermasVsVentas_CalculaElPorcentajeSobreVentas()
    {
        var mermas = new[]
        {
            new ComparativoMermaDto(MotivoMerma.Danado, 2, 3m, 10m),
            new ComparativoMermaDto(MotivoMerma.Vencido, 1, 1m, 5m)
        };
        _repo.Setup(r => r.ObtenerMermasVsVentasAsync(Ahora.AddDays(-7), Ahora, It.IsAny<CancellationToken>()))
            .ReturnsAsync((200m, (IReadOnlyList<ComparativoMermaDto>)mermas));

        var reporte = await CrearServicio().ObtenerMermasVsVentasAsync(Ahora.AddDays(-7), Ahora);

        Assert.Equal(15m, reporte.MermaValorizadaUSD);
        Assert.Equal(7.5m, reporte.Porcentaje);
    }

    [Fact]
    public async Task MermasVsVentas_SinVentas_PorcentajeCeroSinDividirEntreCero()
    {
        _repo.Setup(r => r.ObtenerMermasVsVentasAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((0m, (IReadOnlyList<ComparativoMermaDto>)[new ComparativoMermaDto(MotivoMerma.Danado, 1, 1m, 4m)]));

        var reporte = await CrearServicio().ObtenerMermasVsVentasAsync(Ahora.AddDays(-7), Ahora);

        Assert.Equal(0m, reporte.Porcentaje);
    }

    [Fact]
    public async Task Dashboard_UsaInicioDeDiaYDeMesUtc()
    {
        var esperado = new DashboardDto(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, false, 0, 0, 0, Ahora);
        _repo.Setup(r => r.ObtenerDashboardAsync(
                new DateTime(2026, 10, 14, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                Ahora,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(esperado);

        Assert.Same(esperado, await CrearServicio().ObtenerDashboardAsync());
    }

    // ---------- Top productos y rotación (2.2) ----------

    private static ProductoVendidoCrudoDto Fila(string nombre, decimal unidades, decimal ingreso, decimal existencia)
        => new(Guid.NewGuid(), nombre.ToUpperInvariant(), nombre, "750 ml", unidades, ingreso, existencia);

    private void DevolverProductos(params ProductoVendidoCrudoDto[] filas)
        => _repo.Setup(r => r.ObtenerProductosVendidosAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(filas);

    [Fact]
    public async Task Productos_OrdenPorUnidades_LimitaYOrdenaDescendente()
    {
        DevolverProductos(
            Fila("Ron", 30m, 90m, 10m),
            Fila("Vodka", 50m, 100m, 10m),
            Fila("Whisky", 10m, 400m, 10m),
            Fila("Gin", 20m, 60m, 10m));

        var reporte = await CrearServicio().ObtenerProductosAsync(limite: 2, orden: OrdenProductos.Unidades);

        Assert.Equal(["Vodka", "Ron"], reporte.Top.Select(p => p.ProductoNombre));
        Assert.Equal(110m, reporte.TotalUnidades);
        Assert.Equal(650m, reporte.TotalIngresoUSD);
    }

    [Fact]
    public async Task Productos_OrdenPorIngresos_PriorizaElMayorIngreso()
    {
        DevolverProductos(
            Fila("Ron", 30m, 90m, 10m),
            Fila("Whisky", 10m, 400m, 10m));

        var reporte = await CrearServicio().ObtenerProductosAsync(orden: OrdenProductos.Ingresos);

        Assert.Equal("Whisky", reporte.Top[0].ProductoNombre);
    }

    [Fact]
    public async Task Productos_CalculaElPorcentajeDelTotal()
    {
        DevolverProductos(Fila("Ron", 25m, 50m, 5m), Fila("Vodka", 75m, 150m, 5m));

        var reporte = await CrearServicio().ObtenerProductosAsync();

        Assert.Equal(75m, reporte.Top[0].PorcentajeUnidades);
        Assert.Equal(25m, reporte.Top[1].PorcentajeIngreso);
    }

    [Fact]
    public async Task Productos_Cortesia_CuentaUnidadesPeroNoIngreso()
    {
        DevolverProductos(Fila("Ron", 4m, 0m, 8m));

        var reporte = await CrearServicio().ObtenerProductosAsync();

        Assert.Equal(4m, reporte.Top[0].Unidades);
        Assert.Equal(0m, reporte.Top[0].IngresoUSD);
        Assert.Equal(0m, reporte.Top[0].PorcentajeIngreso);
    }

    [Fact]
    public async Task Productos_Rotacion_UnidadesEntreExistencia()
    {
        DevolverProductos(Fila("Ron", 30m, 90m, 10m));

        var producto = (await CrearServicio().ObtenerProductosAsync()).Top.Single();

        Assert.Equal(3m, producto.Rotacion);
        Assert.Equal(EstadoRotacion.Calculada, producto.EstadoRotacion);
    }

    [Fact]
    public async Task Productos_VendidoConExistenciaCero_RotacionNoCalculable()
    {
        DevolverProductos(Fila("Ron", 30m, 90m, 0m));

        var producto = (await CrearServicio().ObtenerProductosAsync()).Top.Single();

        Assert.Null(producto.Rotacion);
        Assert.Equal(EstadoRotacion.NoCalculable, producto.EstadoRotacion);
    }

    [Fact]
    public async Task Productos_ConExistenciaYSinVentas_AparecenSinMovimientoConRotacionCero()
    {
        DevolverProductos(Fila("Ron", 30m, 90m, 10m), Fila("Tequila", 0m, 0m, 12m), Fila("Agotado", 0m, 0m, 0m));

        var reporte = await CrearServicio().ObtenerProductosAsync();

        var sinMovimiento = Assert.Single(reporte.SinMovimiento);
        Assert.Equal("Tequila", sinMovimiento.ProductoNombre);
        Assert.Equal(0m, sinMovimiento.Rotacion);
        Assert.Equal(EstadoRotacion.SinMovimiento, sinMovimiento.EstadoRotacion);
        Assert.DoesNotContain(reporte.Top, p => p.ProductoNombre == "Tequila");
    }

    [Fact]
    public async Task Productos_SinVentas_NoDivideEntreCero()
    {
        DevolverProductos();

        var reporte = await CrearServicio().ObtenerProductosAsync();

        Assert.Empty(reporte.Top);
        Assert.Equal(0m, reporte.TotalUnidades);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task Productos_LimiteFueraDeRango_LanzaArgumentException(int limite)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => CrearServicio().ObtenerProductosAsync(limite: limite));
    }

    [Fact]
    public async Task Productos_RangoInvertido_LanzaArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            CrearServicio().ObtenerProductosAsync(Ahora, Ahora.AddDays(-1)));
    }

    // ---------- Ticket promedio y métodos de pago (2.4) ----------

    [Fact]
    public async Task Ventas_PagosMixtosUsdYBs_ConservaCadaMetodoConSuMonedaOriginal()
    {
        var efectivo = new VentaPorMetodoPagoDto(Guid.NewGuid(), "Efectivo", Moneda.USD, 150m, 6);
        var pagoMovil = new VentaPorMetodoPagoDto(Guid.NewGuid(), "Pago móvil", Moneda.BS, 3650m, 4);
        var esperado = VentasVacias() with
        {
            TotalUSD = 250m,
            Cantidad = 10,
            TicketPromedioUSD = 25m,
            PorMetodoPago = [efectivo, pagoMovil]
        };
        _repo.Setup(r => r.ObtenerVentasAsync(Ahora.AddDays(-7), Ahora, It.IsAny<CancellationToken>())).ReturnsAsync(esperado);

        var ventas = await CrearServicio().ObtenerVentasAsync(Ahora.AddDays(-7), Ahora);

        Assert.Equal(25m, ventas.TicketPromedioUSD);
        Assert.Equal(10, ventas.Cantidad);
        Assert.Equal([Moneda.USD, Moneda.BS], ventas.PorMetodoPago.Select(m => m.Moneda));
        Assert.Equal(3650m, ventas.PorMetodoPago.Single(m => m.MetodoPagoNombre == "Pago móvil").Monto);
    }

    // ---------- Ocupación y reservas (3.2) ----------

    private static OcupacionZonaCrudaDto Zona(string nombre, int mesas, int ocupadas, int sesiones = 0, int abiertas = 0, decimal minutos = 0m)
        => new(Guid.NewGuid(), nombre, mesas, ocupadas, sesiones, abiertas, minutos);

    [Fact]
    public async Task Ocupacion_CincoDeVeinteMesas_Es25PorCiento()
    {
        _repo.Setup(r => r.ObtenerOcupacionAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), Ahora, It.IsAny<CancellationToken>()))
            .ReturnsAsync([Zona("Salón", 12, 3), Zona("VIP", 8, 2)]);

        var reporte = await CrearServicio().ObtenerOcupacionAsync();

        Assert.Equal(20, reporte.MesasActivas);
        Assert.Equal(5, reporte.MesasOcupadas);
        Assert.Equal(25m, reporte.PorcentajeOcupacion);
        Assert.Equal(25m, reporte.PorZona.Single(z => z.Zona == "Salón").PorcentajeOcupacion);
        Assert.Equal(25m, reporte.PorZona.Single(z => z.Zona == "VIP").PorcentajeOcupacion);
    }

    [Fact]
    public async Task Ocupacion_CalculaPermanenciaPromedioYSesionesPorMesa()
    {
        _repo.Setup(r => r.ObtenerOcupacionAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), Ahora, It.IsAny<CancellationToken>()))
            .ReturnsAsync([Zona("Salón", 4, 1, sesiones: 8, abiertas: 1, minutos: 960m)]);

        var zona = (await CrearServicio().ObtenerOcupacionAsync()).PorZona.Single();

        Assert.Equal(120m, zona.PermanenciaPromedioMinutos);
        Assert.Equal(2m, zona.SesionesPorMesa);
        Assert.Equal(1, zona.SesionesAbiertas);
    }

    [Fact]
    public async Task Ocupacion_SinMesasNiSesiones_NoDivideEntreCero()
    {
        _repo.Setup(r => r.ObtenerOcupacionAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([Zona("Vacía", 0, 0)]);

        var reporte = await CrearServicio().ObtenerOcupacionAsync();

        Assert.Equal(0m, reporte.PorcentajeOcupacion);
        Assert.Null(reporte.PermanenciaPromedioMinutos);
        Assert.Null(reporte.SesionesPorMesa);
        Assert.Null(reporte.PorZona.Single().SesionesPorMesa);
    }

    private void DevolverReservas(params ReservasPorEstadoDto[] filas)
        => _repo.Setup(r => r.ObtenerReservasPorEstadoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(filas);

    [Fact]
    public async Task Reservas_OchoAsistieronYDosNo_TasaDeNoAsistenciaDel20PorCiento()
    {
        DevolverReservas(
            new ReservasPorEstadoDto(EstadoReserva.Asistio, 8),
            new ReservasPorEstadoDto(EstadoReserva.NoAsistio, 2));

        var reporte = await CrearServicio().ObtenerReservasAsistenciaAsync();

        Assert.Equal(20m, reporte.TasaNoAsistencia);
        Assert.Equal(10, reporte.Total);
    }

    [Fact]
    public async Task Reservas_LasCanceladasSeCuentanAparteYNoEntranEnLaTasa()
    {
        DevolverReservas(
            new ReservasPorEstadoDto(EstadoReserva.Asistio, 8),
            new ReservasPorEstadoDto(EstadoReserva.NoAsistio, 2),
            new ReservasPorEstadoDto(EstadoReserva.Cancelada, 5));

        var reporte = await CrearServicio().ObtenerReservasAsistenciaAsync();

        Assert.Equal(5, reporte.Canceladas);
        Assert.Equal(20m, reporte.TasaNoAsistencia);
        Assert.Equal(15, reporte.Total);
    }

    [Fact]
    public async Task Reservas_SinReservasQueDebieranConcretarse_TasaNula()
    {
        DevolverReservas(new ReservasPorEstadoDto(EstadoReserva.Cancelada, 3));

        var reporte = await CrearServicio().ObtenerReservasAsistenciaAsync();

        Assert.Null(reporte.TasaNoAsistencia);
    }

    // ---------- Caja y mermas (3.3) ----------

    private static TurnoCajaDto Turno(decimal esperado, decimal contado)
        => new(Guid.NewGuid(), Guid.NewGuid(), "Cajero", Ahora.AddHours(-8), Ahora.AddHours(-1), esperado, contado, contado - esperado);

    [Fact]
    public async Task Caja_ResumeTotalPromedioYMayorEnValorAbsoluto()
    {
        _repo.Setup(r => r.ObtenerTurnosCerradosAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([Turno(100m, 95m), Turno(100m, 101m), Turno(50m, 50m)]);

        var reporte = await CrearServicio().ObtenerCajaAsync();

        Assert.Equal(3, reporte.Turnos);
        Assert.Equal(-4m, reporte.DescuadreTotal);
        Assert.Equal(-1.33m, reporte.DescuadrePromedio);
        Assert.Equal(-5m, reporte.MayorDescuadre);
    }

    [Fact]
    public async Task Caja_SinTurnosCerrados_ResumenEnCero()
    {
        _repo.Setup(r => r.ObtenerTurnosCerradosAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var reporte = await CrearServicio().ObtenerCajaAsync();

        Assert.Equal(0, reporte.Turnos);
        Assert.Equal(0m, reporte.DescuadrePromedio);
        Assert.Equal(0m, reporte.MayorDescuadre);
    }

    [Fact]
    public async Task MermasVsVentas_CadaMotivoTraeSuProporcionFrenteALasVentas()
    {
        _repo.Setup(r => r.ObtenerMermasVsVentasAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((200m, (IReadOnlyList<ComparativoMermaDto>)
            [
                new ComparativoMermaDto(MotivoMerma.Danado, 2, 3m, 10m),
                new ComparativoMermaDto(MotivoMerma.Vencido, 1, 1m, 5m)
            ]));

        var reporte = await CrearServicio().ObtenerMermasVsVentasAsync();

        Assert.Equal(5m, reporte.PorMotivo.Single(m => m.Motivo == MotivoMerma.Danado).PorcentajeVentas);
        Assert.Equal(2.5m, reporte.PorMotivo.Single(m => m.Motivo == MotivoMerma.Vencido).PorcentajeVentas);
    }

    // ---------- Desempeño del mesonero (4.4) ----------

    private static MesoneroCrudoDto Actividad(
        string nombre,
        decimal ventas,
        int cuentas = 1,
        int totales = 10,
        int cancelados = 0,
        int conTiempo = 0,
        double segundos = 0d)
        => new(Guid.NewGuid(), nombre, 2, cuentas, totales, cancelados, totales - cancelados, ventas, conTiempo, segundos);

    private void DevolverActividad(params MesoneroCrudoDto[] filas)
        => _repo.Setup(r => r.ObtenerActividadMesonerosAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(filas);

    [Fact]
    public async Task Mesoneros_TicketPorCuenta_VentasEntreCuentasAtendidas()
    {
        DevolverActividad(Actividad("Ana", 150m, cuentas: 4));

        var ana = (await CrearServicio().ObtenerMesonerosAsync()).Mesoneros.Single();

        Assert.Equal(37.5m, ana.TicketPorCuentaUSD);
        Assert.Equal(150m, ana.VentasGeneradasUSD);
    }

    [Fact]
    public async Task Mesoneros_TasaDeCancelacion_CanceladosEntreTotales()
    {
        DevolverActividad(Actividad("Ana", 80m, totales: 20, cancelados: 3));

        var ana = (await CrearServicio().ObtenerMesonerosAsync()).Mesoneros.Single();

        Assert.Equal(15m, ana.TasaCancelacion);
        Assert.Equal(3, ana.ItemsCancelados);
        Assert.Equal(17, ana.ItemsServidos);
    }

    [Fact]
    public async Task Mesoneros_TiempoDeServicio_PromediaSoloLosItemsConMarca()
    {
        // 3 ítems con ServidoEn suman 540 s; los ítems sin la marca no entran en el promedio.
        DevolverActividad(Actividad("Ana", 80m, totales: 10, conTiempo: 3, segundos: 540d));

        var ana = (await CrearServicio().ObtenerMesonerosAsync()).Mesoneros.Single();

        Assert.Equal(180m, ana.TiempoServicioPromedioSegundos);
    }

    [Fact]
    public async Task Mesoneros_SinItemsServidosConMarca_TiempoNuloEnLugarDeCero()
    {
        DevolverActividad(Actividad("Ana", 80m, conTiempo: 0));

        var ana = (await CrearServicio().ObtenerMesonerosAsync()).Mesoneros.Single();

        Assert.Null(ana.TiempoServicioPromedioSegundos);
    }

    [Fact]
    public async Task Mesoneros_SinCuentasNiItems_NoDivideEntreCero()
    {
        DevolverActividad(Actividad("Ana", 0m, cuentas: 0, totales: 0));

        var ana = (await CrearServicio().ObtenerMesonerosAsync()).Mesoneros.Single();

        Assert.Equal(0m, ana.TicketPorCuentaUSD);
        Assert.Equal(0m, ana.TasaCancelacion);
    }

    [Fact]
    public async Task Mesoneros_SeOrdenanPorVentasGeneradasYSumanElTotal()
    {
        DevolverActividad(Actividad("Luis", 40m), Actividad("Ana", 100m), Actividad("Sin asignar", 10m));

        var reporte = await CrearServicio().ObtenerMesonerosAsync();

        Assert.Equal(["Ana", "Luis", "Sin asignar"], reporte.Mesoneros.Select(m => m.Nombre));
        Assert.Equal(150m, reporte.TotalVentasUSD);
    }

    private static ReporteVentasDto VentasVacias()
        => new(Ahora.AddDays(-30), Ahora, 0m, 0m, 0, 0m, [], [], []);
}
