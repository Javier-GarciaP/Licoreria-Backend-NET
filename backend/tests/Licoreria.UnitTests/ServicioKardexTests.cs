using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.UnitTests.Support;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>
/// Movimientos de kardex, desglose de recetas y verificación de stock de <see cref="ServicioKardex"/>,
/// aislado con Moq (las porciones y la reserva tienen sus propios archivos de pruebas).
/// </summary>
public sealed class ServicioKardexTests
{
    private readonly Mock<IInventarioRepository> _inventario = new();
    private readonly Mock<IPorcionRepository> _porciones = new();
    private readonly Dictionary<Guid, StockProducto> _stocks = new();

    public ServicioKardexTests()
    {
        _porciones
            .Setup(p => p.TieneOrigenesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _inventario
            .Setup(i => i.ObtenerStockAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => _stocks.GetValueOrDefault(id));
        _inventario
            .Setup(i => i.ObtenerStockDeVariantesAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Guid> ids, CancellationToken _) =>
                ids.Where(_stocks.ContainsKey).Select(id => _stocks[id]).ToList());
    }

    private ServicioKardex CrearServicio() => new(_inventario.Object, _porciones.Object, RelojFijo.Crear().Object);

    private Guid ConStock(decimal cantidad)
    {
        var id = Guid.NewGuid();
        var stock = new StockProducto { VarianteId = id };
        stock.AplicarMovimiento(cantidad);
        _stocks[id] = stock;
        return id;
    }

    private void ConVariante(Guid id, ProductoVariante variante)
        => _inventario
            .Setup(i => i.ObtenerVarianteConRecetasAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(variante);

    // ---------- AplicarAsync ----------

    [Fact]
    public async Task Aplicar_ConExistencia_ActualizaLaCantidadYRegistraElMovimiento()
    {
        var variante = ConStock(10m);
        var referencia = Guid.NewGuid();

        var movimiento = await CrearServicio().AplicarAsync(
            variante, TipoMovimientoInventario.Venta, -3m, "Venta", referencia, "Venta de prueba");

        Assert.Equal(7m, _stocks[variante].Cantidad);
        Assert.Equal(variante, movimiento.VarianteId);
        Assert.Equal(TipoMovimientoInventario.Venta, movimiento.Tipo);
        Assert.Equal(-3m, movimiento.Cantidad);
        Assert.Equal("Venta", movimiento.ReferenciaTipo);
        Assert.Equal(referencia, movimiento.ReferenciaId);
        Assert.Equal("Venta de prueba", movimiento.Motivo);
        _inventario.Verify(i => i.AgregarMovimientoAsync(movimiento, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Aplicar_VarianteSinExistencia_CreaLaExistenciaConLaEntrada()
    {
        var variante = Guid.NewGuid();
        StockProducto? creado = null;
        _inventario
            .Setup(i => i.AgregarStockAsync(It.IsAny<StockProducto>(), It.IsAny<CancellationToken>()))
            .Callback<StockProducto, CancellationToken>((s, _) => creado = s)
            .Returns(Task.CompletedTask);

        var movimiento = await CrearServicio().AplicarAsync(variante, TipoMovimientoInventario.Compra, 12m);

        Assert.NotNull(creado);
        Assert.Equal(variante, creado!.VarianteId);
        Assert.Equal(12m, creado.Cantidad);
        Assert.Equal(12m, movimiento.Cantidad);
    }

    [Fact]
    public async Task Aplicar_QueDejaExistenciaNegativa_LanzaReglaDeNegocioYNoRegistraMovimiento()
    {
        var variante = ConStock(2m);

        var ex = await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            CrearServicio().AplicarAsync(variante, TipoMovimientoInventario.Merma, -5m));

        Assert.Contains("negativo", ex.Message);
        Assert.Equal(2m, _stocks[variante].Cantidad);
        _inventario.Verify(
            i => i.AgregarMovimientoAsync(It.IsAny<MovimientoInventario>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Aplicar_SacandoExactamenteLaExistencia_DejaCero()
    {
        var variante = ConStock(4m);

        await CrearServicio().AplicarAsync(variante, TipoMovimientoInventario.Ajuste, -4m);

        Assert.Equal(0m, _stocks[variante].Cantidad);
    }

    // ---------- DesglosarInsumosAsync ----------

    [Fact]
    public async Task Desglosar_VarianteInexistente_DevuelveLaPropiaVariante()
    {
        var id = Guid.NewGuid();
        _inventario
            .Setup(i => i.ObtenerVarianteConRecetasAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductoVariante?)null);

        var insumos = await CrearServicio().DesglosarInsumosAsync(id, 2m);

        Assert.Equal([new KardexInsumo(id, 2m)], insumos);
    }

    [Fact]
    public async Task Desglosar_VarianteSinReceta_DevuelveLaPropiaVariante()
    {
        var id = Guid.NewGuid();
        ConVariante(id, new ProductoVariante { Nombre = "Lata", Producto = new Producto { AreaDestino = AreaDestino.Barra } });

        var insumos = await CrearServicio().DesglosarInsumosAsync(id, 3m);

        Assert.Equal([new KardexInsumo(id, 3m)], insumos);
    }

    [Fact]
    public async Task Desglosar_ConReceta_DevuelveUnaFilaPorInsumoMultiplicadaPorLaCantidad()
    {
        var id = Guid.NewGuid();
        var ron = Guid.NewGuid();
        var cola = Guid.NewGuid();
        var variante = new ProductoVariante { Nombre = "Cuba libre", Producto = new Producto { AreaDestino = AreaDestino.Barra } };
        variante.RecetasVendidas.Add(new Receta { VarianteInsumoId = ron, Cantidad = 0.05m });
        variante.RecetasVendidas.Add(new Receta { VarianteInsumoId = cola, Cantidad = 0.2m });
        ConVariante(id, variante);

        var insumos = await CrearServicio().DesglosarInsumosAsync(id, 4m);

        Assert.Equal(2, insumos.Count);
        Assert.Contains(new KardexInsumo(ron, 0.2m), insumos);
        Assert.Contains(new KardexInsumo(cola, 0.8m), insumos);
    }

    [Fact]
    public async Task Desglosar_ProductoDeCocina_NoGeneraInsumosAunqueTengaReceta()
    {
        var id = Guid.NewGuid();
        var variante = new ProductoVariante { Nombre = "Hamburguesa", Producto = new Producto { AreaDestino = AreaDestino.Cocina } };
        variante.RecetasVendidas.Add(new Receta { VarianteInsumoId = Guid.NewGuid(), Cantidad = 1m });
        ConVariante(id, variante);

        var insumos = await CrearServicio().DesglosarInsumosAsync(id, 2m);

        Assert.Empty(insumos);
    }

    // ---------- VerificarStockAsync ----------

    [Fact]
    public async Task VerificarStock_SinInsumos_NoConsultaNada()
    {
        await CrearServicio().VerificarStockAsync([]);

        _inventario.Verify(
            i => i.ObtenerStockDeVariantesAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task VerificarStock_ConExistenciaSuficiente_NoLanza()
    {
        var a = ConStock(5m);

        await CrearServicio().VerificarStockAsync([new KardexInsumo(a, 5m)]);
    }

    [Fact]
    public async Task VerificarStock_AgrupaLosInsumosRepetidosAntesDeComparar()
    {
        var a = ConStock(5m);
        ConVariante(a, new ProductoVariante { Nombre = "Lata", Producto = new Producto { Nombre = "Coca-Cola" } });

        // Dos líneas de 3 unidades suman 6 y solo hay 5: ninguna línea por separado alcanza el límite.
        var ex = await Assert.ThrowsAsync<ExistenciaAgotadaException>(() =>
            CrearServicio().VerificarStockAsync([new KardexInsumo(a, 3m), new KardexInsumo(a, 3m)]));

        Assert.Equal("Coca-Cola", ex.Producto);
        Assert.Equal(6m, ex.Requerido);
        Assert.Equal(5m, ex.Disponible);
    }

    [Fact]
    public async Task VerificarStock_DescuentaLoReservado()
    {
        var a = ConStock(5m);
        _stocks[a].Reservar(4m);
        ConVariante(a, new ProductoVariante { Nombre = "Lata" });

        var ex = await Assert.ThrowsAsync<ExistenciaAgotadaException>(() =>
            CrearServicio().VerificarStockAsync([new KardexInsumo(a, 2m)]));

        Assert.Equal(1m, ex.Disponible);
    }

    [Fact]
    public async Task VerificarStock_VarianteSinExistenciaRegistrada_SeTrataComoCero()
    {
        var a = Guid.NewGuid();
        ConVariante(a, new ProductoVariante { Nombre = "Lata" });

        var ex = await Assert.ThrowsAsync<ExistenciaAgotadaException>(() =>
            CrearServicio().VerificarStockAsync([new KardexInsumo(a, 1m)]));

        Assert.Equal(0m, ex.Disponible);
    }

    [Fact]
    public async Task VerificarStock_ConVariasVariantes_ConsultaLaExistenciaEnUnaSolaLlamada()
    {
        var a = ConStock(5m);
        var b = ConStock(5m);

        await CrearServicio().VerificarStockAsync([new KardexInsumo(a, 1m), new KardexInsumo(b, 1m)]);

        _inventario.Verify(
            i => i.ObtenerStockDeVariantesAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
