using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>Disponibilidad, reserva, consumo y liberación de porciones en <see cref="ServicioKardex"/>, aislado con Moq.</summary>
public sealed class ServicioKardexPorcionesTests
{
    private static readonly DateTime Ahora = new(2026, 10, 14, 22, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IInventarioRepository> _inventario = new();
    private readonly Mock<IPorcionRepository> _porciones = new();
    private readonly Mock<IRelojSistema> _reloj = new();

    private readonly Producto _cola = new() { Nombre = "Coca-Cola" };
    private readonly ProductoVariante _litro = new() { Nombre = "Botella 1 L", EsBase = true };
    private readonly ProductoVariante _vaso = new() { Nombre = "Vaso" };
    private readonly ProductoVariante _presentacionSola = new() { Nombre = "Lata", EsBase = true };

    private readonly List<EnvaseAbierto> _abiertos = new();
    private readonly StockPorcion _stockVaso;
    private readonly List<MovimientoInventario> _movimientos = new();

    public ServicioKardexPorcionesTests()
    {
        _reloj.SetupGet(r => r.UtcNow).Returns(Ahora);
        _vaso.Producto = _cola;
        _stockVaso = new StockPorcion { VarianteId = _vaso.Id };

        _porciones.Setup(p => p.TieneOrigenesAsync(_vaso.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _porciones.Setup(p => p.TieneOrigenesAsync(It.Is<Guid>(id => id != _vaso.Id), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _porciones.Setup(p => p.ObtenerAbiertosAsync(_vaso.Id, It.IsAny<CancellationToken>())).ReturnsAsync(() => _abiertos.Where(e => e.Estado == EstadoEnvase.Abierto).ToList());
        _porciones.Setup(p => p.ObtenerStockPorcionAsync(_vaso.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_stockVaso);
        _porciones
            .Setup(p => p.ObtenerOrigenesAsync(_vaso.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PorcionOrigen { PorcionVarianteId = _vaso.Id, PresentacionVarianteId = _litro.Id, PorcionesPorUnidad = 4 }]);
        _inventario.Setup(i => i.ObtenerVarianteConRecetasAsync(_vaso.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_vaso);
        _inventario
            .Setup(i => i.AgregarMovimientoAsync(It.IsAny<MovimientoInventario>(), It.IsAny<CancellationToken>()))
            .Callback<MovimientoInventario, CancellationToken>((m, _) => _movimientos.Add(m))
            .Returns(Task.CompletedTask);
        ConBotellasCerradas(0m);
    }

    private ServicioKardex CrearServicio() => new(_inventario.Object, _porciones.Object, _reloj.Object);

    private void ConBotellasCerradas(decimal cantidad)
    {
        var stock = new StockProducto { VarianteId = _litro.Id };
        if (cantidad > 0) stock.AplicarMovimiento(cantidad);
        _inventario.Setup(i => i.ObtenerStockAsync(_litro.Id, It.IsAny<CancellationToken>())).ReturnsAsync(stock);
    }

    private EnvaseAbierto Abrir(int restantes, int minutosAntes = 0)
    {
        var envase = EnvaseAbierto.Destapar(_vaso.Id, _litro.Id, 8, null, Ahora.AddMinutes(-minutosAntes));
        if (restantes < 8) envase.Consumir(8 - restantes, Ahora);
        _abiertos.Add(envase);
        return envase;
    }

    private static KardexInsumo Vasos(Guid id, decimal n) => new(id, n);

    /* ===================== 4.1 Disponibilidad ===================== */

    [Fact]
    public async Task Verificar_ConPorcionesAbiertasYReservadas_UsaLaDiferencia()
    {
        Abrir(6);
        _stockVaso.Reservar(2);

        await CrearServicio().VerificarStockAsync([Vasos(_vaso.Id, 4)]);

        var ex = await Assert.ThrowsAsync<ExistenciaAgotadaException>(
            () => CrearServicio().VerificarStockAsync([Vasos(_vaso.Id, 5)]));
        Assert.Equal(4m, ex.Disponible);
    }

    [Fact]
    public async Task Verificar_SumaLasPorcionesDeVariosEnvasesAbiertos()
    {
        Abrir(2, minutosAntes: 30);
        Abrir(8);

        await CrearServicio().VerificarStockAsync([Vasos(_vaso.Id, 10)]);
    }

    [Fact]
    public async Task Verificar_UnEnvaseAgotadoNoCuenta()
    {
        var agotado = Abrir(1);
        agotado.Consumir(1, Ahora);

        await Assert.ThrowsAsync<ExistenciaAgotadaException>(
            () => CrearServicio().VerificarStockAsync([Vasos(_vaso.Id, 1)]));
    }

    [Fact]
    public async Task Verificar_PresentacionesYPorcionesJuntas_ValidaAmbas()
    {
        Abrir(8);
        var lata = new StockProducto { VarianteId = _presentacionSola.Id };
        lata.AplicarMovimiento(1m);
        _inventario
            .Setup(i => i.ObtenerStockDeVariantesAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([lata]);

        await CrearServicio().VerificarStockAsync([Vasos(_vaso.Id, 3), new KardexInsumo(_presentacionSola.Id, 1m)]);
        await Assert.ThrowsAsync<ExistenciaAgotadaException>(
            () => CrearServicio().VerificarStockAsync([Vasos(_vaso.Id, 3), new KardexInsumo(_presentacionSola.Id, 2m)]));
    }

    /* ===================== 4.2 Reservar y liberar ===================== */

    [Fact]
    public async Task Reservar_ConPorcionesAbiertas_LasCompromete()
    {
        Abrir(4);

        await CrearServicio().ReservarAsync([Vasos(_vaso.Id, 2)]);

        Assert.Equal(2, _stockVaso.PorcionesReservadas);
    }

    [Fact]
    public async Task Reservar_SinEnvaseAbiertoPeroConBotellasCerradas_LanzaPorcionesSinAbrir()
    {
        ConBotellasCerradas(3m);

        var ex = await Assert.ThrowsAsync<PorcionesSinAbrirException>(
            () => CrearServicio().ReservarAsync([Vasos(_vaso.Id, 1)]));

        Assert.Equal(3, ex.EnvasesCerrados);
        Assert.Equal("Coca-Cola · Vaso", ex.Producto);
        Assert.Equal(0, _stockVaso.PorcionesReservadas);
    }

    [Fact]
    public async Task Reservar_SinNadaQueServir_LanzaExistenciaAgotadaYNoSinAbrir()
    {
        ConBotellasCerradas(0m);

        var ex = await Assert.ThrowsAsync<ExistenciaAgotadaException>(
            () => CrearServicio().ReservarAsync([Vasos(_vaso.Id, 1)]));

        Assert.IsNotType<PorcionesSinAbrirException>(ex);
    }

    [Fact]
    public async Task Reservar_MasQueLasAbiertasConBotellasCerradas_PideDestaparOtra()
    {
        Abrir(2);
        ConBotellasCerradas(2m);

        await Assert.ThrowsAsync<PorcionesSinAbrirException>(
            () => CrearServicio().ReservarAsync([Vasos(_vaso.Id, 3)]));
    }

    [Fact]
    public async Task Reservar_NoPuedeTomarLoYaReservado()
    {
        Abrir(3);
        _stockVaso.Reservar(3);

        await Assert.ThrowsAnyAsync<ExistenciaAgotadaException>(
            () => CrearServicio().ReservarAsync([Vasos(_vaso.Id, 1)]));
    }

    [Fact]
    public async Task Reservar_UnaCantidadNoEntera_LanzaReglaDeNegocio()
    {
        Abrir(4);

        await Assert.ThrowsAsync<ReglaNegocioException>(() => CrearServicio().ReservarAsync([Vasos(_vaso.Id, 1.5m)]));
    }

    [Fact]
    public async Task Reservar_SiNoExisteLaFilaDeReservas_LaCrea()
    {
        Abrir(4);
        _porciones.Setup(p => p.ObtenerStockPorcionAsync(_vaso.Id, It.IsAny<CancellationToken>())).ReturnsAsync((StockPorcion?)null);
        StockPorcion? creada = null;
        _porciones
            .Setup(p => p.AgregarStockPorcionAsync(It.IsAny<StockPorcion>(), It.IsAny<CancellationToken>()))
            .Callback<StockPorcion, CancellationToken>((s, _) => creada = s)
            .Returns(Task.CompletedTask);

        await CrearServicio().ReservarAsync([Vasos(_vaso.Id, 2)]);

        Assert.Equal(2, creada!.PorcionesReservadas);
    }

    [Fact]
    public async Task Liberar_DevuelveLasPorcionesReservadas()
    {
        Abrir(4);
        _stockVaso.Reservar(3);

        await CrearServicio().LiberarReservaAsync([Vasos(_vaso.Id, 2)]);

        Assert.Equal(1, _stockVaso.PorcionesReservadas);
    }

    [Fact]
    public async Task Liberar_MasDeLoReservado_LiberaSoloLoQueHay()
    {
        _stockVaso.Reservar(1);

        await CrearServicio().LiberarReservaAsync([Vasos(_vaso.Id, 5)]);

        Assert.Equal(0, _stockVaso.PorcionesReservadas);
    }

    /* ===================== 4.3 Consumir ===================== */

    [Fact]
    public async Task Consumir_TresVasosConDosEnvases_AgotaElMasAntiguoYSigueConElSiguiente()
    {
        var antiguo = Abrir(2, minutosAntes: 60);
        var reciente = Abrir(8, minutosAntes: 10);
        _stockVaso.Reservar(3);

        var movimiento = await CrearServicio().ConsumirReservaAsync(
            _vaso.Id, TipoMovimientoInventario.Venta, 3m, "comanda", Guid.NewGuid(), "Salida por comanda servida");

        Assert.Equal(EstadoEnvase.Agotado, antiguo.Estado);
        Assert.Equal(0, antiguo.PorcionesRestantes);
        Assert.Equal(7, reciente.PorcionesRestantes);
        Assert.Equal(EstadoEnvase.Abierto, reciente.Estado);
        Assert.Equal(0, _stockVaso.PorcionesReservadas);
        Assert.Equal(-3m, movimiento.Cantidad);
        Assert.Equal(TipoMovimientoInventario.Venta, movimiento.Tipo);
        Assert.Equal(_vaso.Id, Assert.Single(_movimientos).VarianteId);
    }

    [Fact]
    public async Task Consumir_LaUltimaPorcion_DejaElEnvaseAgotado()
    {
        var envase = Abrir(1);
        _stockVaso.Reservar(1);

        await CrearServicio().ConsumirReservaAsync(_vaso.Id, TipoMovimientoInventario.Venta, 1m);

        Assert.Equal(EstadoEnvase.Agotado, envase.Estado);
    }

    [Fact]
    public async Task Consumir_VentaDirecta_NoTocaLoReservadoPorMesas()
    {
        Abrir(5);
        _stockVaso.Reservar(5);

        await Assert.ThrowsAnyAsync<ExistenciaAgotadaException>(
            () => CrearServicio().AplicarAsync(_vaso.Id, TipoMovimientoInventario.Venta, -1m, "venta", Guid.NewGuid(), "Salida por venta"));

        Assert.Equal(5, _stockVaso.PorcionesReservadas);
    }

    [Fact]
    public async Task Consumir_VentaDirecta_ConPorcionesLibres_SirveYRegistra()
    {
        var envase = Abrir(5);
        _stockVaso.Reservar(2);

        await CrearServicio().AplicarAsync(_vaso.Id, TipoMovimientoInventario.Venta, -3m, "venta", Guid.NewGuid(), "Salida por venta");

        Assert.Equal(2, envase.PorcionesRestantes);
        Assert.Equal(2, _stockVaso.PorcionesReservadas);   // las reservas ajenas no cambian
        Assert.Equal(-3m, Assert.Single(_movimientos).Cantidad);
    }

    [Fact]
    public async Task Reintegro_DeUnaPorcionYaServida_NoModificaLosEnvases()
    {
        var envase = Abrir(4);

        var movimiento = await CrearServicio().AplicarAsync(
            _vaso.Id, TipoMovimientoInventario.Venta, 2m, "comanda", Guid.NewGuid(), "Reintegro por comanda no servida");

        Assert.Equal(4, envase.PorcionesRestantes);
        Assert.Equal(0m, movimiento.Cantidad);
        Assert.Empty(_movimientos);
    }

    [Fact]
    public async Task Aplicar_UnaAperturaSobreUnaPorcion_Falla()
    {
        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().AplicarAsync(_vaso.Id, TipoMovimientoInventario.Apertura, -1m));
    }

    /* ===================== 4.4 Recetas con porciones ===================== */

    [Fact]
    public async Task Desglosar_UnCoctelConPorciones_MultiplicaPorLaCantidadPedida()
    {
        var trago = new ProductoVariante { Nombre = "Trago de ron" };
        var coctel = new ProductoVariante { Nombre = "Cuba libre", Producto = new Producto { Nombre = "Cuba libre" } };
        coctel.RecetasVendidas.Add(new Receta { VarianteVendidaId = coctel.Id, VarianteInsumoId = trago.Id, Cantidad = 1m });
        coctel.RecetasVendidas.Add(new Receta { VarianteVendidaId = coctel.Id, VarianteInsumoId = _vaso.Id, Cantidad = 1m });
        _inventario.Setup(i => i.ObtenerVarianteConRecetasAsync(coctel.Id, It.IsAny<CancellationToken>())).ReturnsAsync(coctel);

        var insumos = await CrearServicio().DesglosarInsumosAsync(coctel.Id, 2m);

        Assert.Equal(2m, insumos.Single(i => i.VarianteId == trago.Id).Cantidad);
        Assert.Equal(2m, insumos.Single(i => i.VarianteId == _vaso.Id).Cantidad);
    }

    [Fact]
    public async Task Reservar_DosCubaLibre_ReservaLosVasosYSeRechazaSiFaltaUnTrago()
    {
        var trago = new ProductoVariante { Nombre = "Trago de ron", Producto = new Producto { Nombre = "Ron" } };
        var stockTrago = new StockPorcion { VarianteId = trago.Id };
        var envaseTrago = EnvaseAbierto.Destapar(trago.Id, Guid.NewGuid(), 1, null, Ahora);   // solo queda 1 trago
        _porciones.Setup(p => p.TieneOrigenesAsync(trago.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _porciones.Setup(p => p.ObtenerAbiertosAsync(trago.Id, It.IsAny<CancellationToken>())).ReturnsAsync([envaseTrago]);
        _porciones.Setup(p => p.ObtenerStockPorcionAsync(trago.Id, It.IsAny<CancellationToken>())).ReturnsAsync(stockTrago);
        _porciones.Setup(p => p.ObtenerOrigenesAsync(trago.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new List<PorcionOrigen>());
        _inventario.Setup(i => i.ObtenerVarianteConRecetasAsync(trago.Id, It.IsAny<CancellationToken>())).ReturnsAsync(trago);
        Abrir(8);

        var ex = await Assert.ThrowsAsync<ExistenciaAgotadaException>(
            () => CrearServicio().ReservarAsync([new KardexInsumo(trago.Id, 2m), new KardexInsumo(_vaso.Id, 2m)]));

        Assert.Equal("Ron · Trago de ron", ex.Producto);
    }
}
