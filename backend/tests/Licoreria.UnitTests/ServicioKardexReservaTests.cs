using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>Reserva, liberación y consumo de existencia en <see cref="ServicioKardex"/>, aislado con Moq.</summary>
public sealed class ServicioKardexReservaTests
{
    private readonly Mock<IInventarioRepository> _inventario = new();
    private readonly Dictionary<Guid, StockProducto> _stocks = new();

    private readonly Mock<IPorcionRepository> _porciones = new();

    private ServicioKardex CrearServicio() => new(_inventario.Object, _porciones.Object, Mock.Of<IRelojSistema>());

    public ServicioKardexReservaTests()
    {
        _inventario
            .Setup(i => i.ObtenerStockAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => _stocks.GetValueOrDefault(id));
        _inventario
            .Setup(i => i.ObtenerStockDeVariantesAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Guid> ids, CancellationToken _) =>
                ids.Where(_stocks.ContainsKey).Select(id => _stocks[id]).ToList());
        _inventario
            .Setup(i => i.ObtenerVarianteConRecetasAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new ProductoVariante
            {
                Nombre = "Lata 355ml",
                Producto = new Producto { Nombre = "Coca-Cola" }
            });
    }

    private Guid ConStock(decimal cantidad, decimal reservado = 0m)
    {
        var id = Guid.NewGuid();
        var stock = new StockProducto { VarianteId = id };
        stock.AplicarMovimiento(cantidad);
        if (reservado > 0)
        {
            stock.Reservar(reservado);
        }

        _stocks[id] = stock;
        return id;
    }

    [Fact]
    public async Task Reservar_ConExistencia_ReservaCadaInsumo()
    {
        var a = ConStock(10m);
        var b = ConStock(4m);

        await CrearServicio().ReservarAsync([new KardexInsumo(a, 3m), new KardexInsumo(b, 1m)]);

        Assert.Equal(3m, _stocks[a].CantidadReservada);
        Assert.Equal(1m, _stocks[b].CantidadReservada);
        Assert.Equal(10m, _stocks[a].Cantidad);
    }

    [Fact]
    public async Task Reservar_MismoInsumoEnVariasLineas_SumaLoRequerido()
    {
        var a = ConStock(5m);

        await CrearServicio().ReservarAsync([new KardexInsumo(a, 2m), new KardexInsumo(a, 2m)]);

        Assert.Equal(4m, _stocks[a].CantidadReservada);
    }

    [Fact]
    public async Task Reservar_SinExistenciaDisponible_LanzaExistenciaAgotadaConLosDatos()
    {
        var a = ConStock(1m, reservado: 1m); // la última unidad ya está comprometida

        var ex = await Assert.ThrowsAsync<ExistenciaAgotadaException>(
            () => CrearServicio().ReservarAsync([new KardexInsumo(a, 1m)]));

        Assert.Equal("Coca-Cola", ex.Producto);
        Assert.Equal(1m, ex.Requerido);
        Assert.Equal(0m, ex.Disponible);
        Assert.Equal(1m, _stocks[a].CantidadReservada);
    }

    [Fact]
    public async Task Reservar_VarianteSinStock_LanzaExistenciaAgotadaConDisponibleCero()
    {
        var ex = await Assert.ThrowsAsync<ExistenciaAgotadaException>(
            () => CrearServicio().ReservarAsync([new KardexInsumo(Guid.NewGuid(), 1m)]));

        Assert.Equal(0m, ex.Disponible);
    }

    [Fact]
    public async Task VerificarStock_UsaLaExistenciaDisponibleYNoLaTotal()
    {
        var a = ConStock(5m, reservado: 4m);

        var ex = await Assert.ThrowsAsync<ExistenciaAgotadaException>(
            () => CrearServicio().VerificarStockAsync([new KardexInsumo(a, 2m)]));

        Assert.Equal(1m, ex.Disponible);
        await CrearServicio().VerificarStockAsync([new KardexInsumo(a, 1m)]);
    }

    [Fact]
    public async Task LiberarReserva_DevuelveLoReservadoADisponible()
    {
        var a = ConStock(5m, reservado: 3m);

        await CrearServicio().LiberarReservaAsync([new KardexInsumo(a, 2m)]);

        Assert.Equal(1m, _stocks[a].CantidadReservada);
        Assert.Equal(4m, _stocks[a].Disponible);
    }

    [Fact]
    public async Task LiberarReserva_MasDeLoReservado_LiberaSoloLoQueHay()
    {
        var a = ConStock(5m, reservado: 1m);

        await CrearServicio().LiberarReservaAsync([new KardexInsumo(a, 3m)]);

        Assert.Equal(0m, _stocks[a].CantidadReservada);
        Assert.Equal(5m, _stocks[a].Cantidad);
    }

    [Fact]
    public async Task LiberarReserva_VarianteSinStock_NoFalla()
    {
        await CrearServicio().LiberarReservaAsync([new KardexInsumo(Guid.NewGuid(), 1m)]);
    }

    [Fact]
    public async Task ConsumirReserva_DescuentaExistenciaYReservaYRegistraElMovimiento()
    {
        var a = ConStock(5m, reservado: 2m);
        MovimientoInventario? registrado = null;
        _inventario
            .Setup(i => i.AgregarMovimientoAsync(It.IsAny<MovimientoInventario>(), It.IsAny<CancellationToken>()))
            .Callback<MovimientoInventario, CancellationToken>((m, _) => registrado = m)
            .Returns(Task.CompletedTask);

        await CrearServicio().ConsumirReservaAsync(a, TipoMovimientoInventario.Venta, 2m, "comanda", Guid.NewGuid(), "Salida por comanda servida");

        Assert.Equal(3m, _stocks[a].Cantidad);
        Assert.Equal(0m, _stocks[a].CantidadReservada);
        Assert.NotNull(registrado);
        Assert.Equal(-2m, registrado!.Cantidad);
        Assert.Equal(TipoMovimientoInventario.Venta, registrado.Tipo);
    }

    [Fact]
    public async Task ConsumirReserva_ConReservaInsuficiente_DescuentaElRestoDeLaExistencia()
    {
        // Ítem anterior a la reserva: solo 1 de las 3 unidades figura reservada.
        var a = ConStock(5m, reservado: 1m);

        await CrearServicio().ConsumirReservaAsync(a, TipoMovimientoInventario.Venta, 3m);

        Assert.Equal(2m, _stocks[a].Cantidad);
        Assert.Equal(0m, _stocks[a].CantidadReservada);
    }

    [Fact]
    public async Task ConsumirReserva_SinExistenciaSuficiente_LanzaReglaDeNegocio()
    {
        var a = ConStock(1m);

        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().ConsumirReservaAsync(a, TipoMovimientoInventario.Venta, 3m));
    }

    [Fact]
    public async Task ConsumirReserva_VarianteSinStock_LanzaExistenciaAgotada()
    {
        await Assert.ThrowsAsync<ExistenciaAgotadaException>(
            () => CrearServicio().ConsumirReservaAsync(Guid.NewGuid(), TipoMovimientoInventario.Venta, 1m));
    }
}
