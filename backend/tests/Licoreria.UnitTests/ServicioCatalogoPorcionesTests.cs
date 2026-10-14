using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>Presentaciones, porciones y sus orígenes en <see cref="ServicioCatalogo"/>, aislado con Moq.</summary>
public sealed class ServicioCatalogoPorcionesTests
{
    private readonly Mock<IProductoRepository> _productos = new();
    private readonly Mock<IRepository<ProductoVariante>> _variantes = new();
    private readonly Mock<IInventarioRepository> _inventario = new();
    private readonly Mock<IPorcionRepository> _porciones = new();

    private readonly Producto _cola = new() { Nombre = "Coca-Cola", Tipo = TipoProducto.Simple, AreaDestino = AreaDestino.Barra };
    private readonly ProductoVariante _litro;
    private readonly ProductoVariante _dosLitros;
    private readonly ProductoVariante _vaso;

    public ServicioCatalogoPorcionesTests()
    {
        _litro = Variante("Botella 1 L", esBase: true);
        _dosLitros = Variante("Botella 2 L", esBase: true);
        _vaso = Variante("Vaso", esBase: false);

        _porciones
            .Setup(p => p.ObtenerOrigenesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PorcionOrigen>());
        _porciones
            .Setup(p => p.ObtenerAbiertosAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EnvaseAbierto>());
        _porciones.Setup(p => p.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _porciones
            .Setup(p => p.ObtenerOrigenesDeVariantesAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PorcionOrigen>());
        _porciones
            .Setup(p => p.ObtenerResumenAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ResumenPorcion>());
        _porciones
            .Setup(p => p.ObtenerIdsConRecetaAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<Guid>());
        _inventario
            .Setup(i => i.ObtenerStockDeVariantesAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StockProducto>());
    }

    private ProductoVariante Variante(string nombre, bool esBase)
    {
        var variante = new ProductoVariante { Nombre = nombre, Sku = nombre, EsBase = esBase, Activo = true, Producto = _cola };
        variante.ProductoId = _cola.Id;
        _cola.Variantes.Add(variante);
        _inventario.Setup(i => i.ObtenerVarianteConRecetasAsync(variante.Id, It.IsAny<CancellationToken>())).ReturnsAsync(variante);
        _variantes.Setup(r => r.GetByIdAsync(variante.Id, It.IsAny<CancellationToken>())).ReturnsAsync(variante);
        return variante;
    }

    private ServicioCatalogo CrearServicio()
        => new(
            _productos.Object,
            Mock.Of<ICategoriaRepository>(),
            Mock.Of<IRepository<Marca>>(),
            Mock.Of<IRepository<UnidadMedida>>(),
            Mock.Of<IRepository<Impuesto>>(),
            Mock.Of<IRepository<ListaPrecio>>(),
            _variantes.Object,
            Mock.Of<IRepository<Modificador>>(),
            Mock.Of<IRepository<ProductoModificador>>(),
            _inventario.Object,
            _porciones.Object);

    private static StockProducto Stock(ProductoVariante variante, decimal cantidad, decimal reservada = 0m)
    {
        var stock = new StockProducto { VarianteId = variante.Id };
        stock.AplicarMovimiento(cantidad);
        if (reservada > 0) stock.Reservar(reservada);
        return stock;
    }

    /* ===================== Orígenes ===================== */

    [Fact]
    public async Task ReemplazarOrigenes_VasoConDosBotellas_LosGuarda()
    {
        IReadOnlyList<PorcionOrigen>? guardados = null;
        _porciones
            .Setup(p => p.ReemplazarOrigenesAsync(_vaso.Id, It.IsAny<IReadOnlyList<PorcionOrigen>>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, IReadOnlyList<PorcionOrigen>, CancellationToken>((_, nuevos, _) => guardados = nuevos)
            .Returns(Task.CompletedTask);

        var resultado = await CrearServicio().ReemplazarOrigenesAsync(
            _vaso.Id, [new(_litro.Id, 4), new(_dosLitros.Id, 8)]);

        Assert.Equal(2, resultado.Count);
        Assert.Equal(4, guardados!.Single(o => o.PresentacionVarianteId == _litro.Id).PorcionesPorUnidad);
        Assert.Equal(8, guardados!.Single(o => o.PresentacionVarianteId == _dosLitros.Id).PorcionesPorUnidad);
        _porciones.Verify(p => p.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task ReemplazarOrigenes_PorcionesPorUnidadInvalidas_Falla(int porciones)
    {
        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().ReemplazarOrigenesAsync(_vaso.Id, [new(_litro.Id, porciones)]));

        _porciones.Verify(
            p => p.ReemplazarOrigenesAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyList<PorcionOrigen>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ReemplazarOrigenes_OrigenDeOtroProducto_Falla()
    {
        var otro = new ProductoVariante { Nombre = "Botella Pepsi", EsBase = true, Activo = true, ProductoId = Guid.NewGuid() };
        _inventario.Setup(i => i.ObtenerVarianteConRecetasAsync(otro.Id, It.IsAny<CancellationToken>())).ReturnsAsync(otro);

        var ex = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().ReemplazarOrigenesAsync(_vaso.Id, [new(otro.Id, 4)]));

        Assert.Contains("mismo producto", ex.Message);
    }

    [Fact]
    public async Task ReemplazarOrigenes_OrigenQueNoEsPresentacion_Falla()
    {
        var otraPorcion = Variante("Medio vaso", esBase: false);
        _inventario.Setup(i => i.ObtenerVarianteConRecetasAsync(otraPorcion.Id, It.IsAny<CancellationToken>())).ReturnsAsync(otraPorcion);

        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().ReemplazarOrigenesAsync(_vaso.Id, [new(otraPorcion.Id, 2)]));
    }

    [Fact]
    public async Task ReemplazarOrigenes_UnaPresentacionNoPuedeSerPorcion()
    {
        var ex = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().ReemplazarOrigenesAsync(_litro.Id, [new(_dosLitros.Id, 2)]));

        Assert.Contains("una sola naturaleza", ex.Message);
    }

    [Fact]
    public async Task ReemplazarOrigenes_VarianteConReceta_NoPuedeSerPorcion()
    {
        _vaso.RecetasVendidas.Add(new Receta { VarianteVendidaId = _vaso.Id, VarianteInsumoId = _litro.Id, Cantidad = 0.25m });

        var ex = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().ReemplazarOrigenesAsync(_vaso.Id, [new(_litro.Id, 4)]));

        Assert.Contains("receta", ex.Message);
    }

    [Theory]
    [InlineData(TipoProducto.Preparado, AreaDestino.Barra)]
    [InlineData(TipoProducto.Simple, AreaDestino.Cocina)]
    public async Task ReemplazarOrigenes_SoloProductosSimplesDeBarra(TipoProducto tipo, AreaDestino area)
    {
        _cola.Tipo = tipo;
        _cola.AreaDestino = area;

        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().ReemplazarOrigenesAsync(_vaso.Id, [new(_litro.Id, 4)]));
    }

    [Fact]
    public async Task ReemplazarOrigenes_PresentacionRepetida_Falla()
    {
        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().ReemplazarOrigenesAsync(_vaso.Id, [new(_litro.Id, 4), new(_litro.Id, 5)]));
    }

    [Fact]
    public async Task ReemplazarOrigenes_VarianteInexistente_LanzaNoEncontrado()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(
            () => CrearServicio().ReemplazarOrigenesAsync(Guid.NewGuid(), [new(_litro.Id, 4)]));
    }

    [Fact]
    public async Task ReemplazarOrigenes_QuitarUnOrigenConEnvaseAbierto_LanzaConflicto()
    {
        _porciones
            .Setup(p => p.ObtenerOrigenesAsync(_vaso.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PorcionOrigen { PorcionVarianteId = _vaso.Id, PresentacionVarianteId = _dosLitros.Id, PorcionesPorUnidad = 8 }]);
        _porciones
            .Setup(p => p.ObtenerAbiertosAsync(_vaso.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([EnvaseAbierto.Destapar(_vaso.Id, _dosLitros.Id, 8, null, DateTime.UtcNow)]);

        await Assert.ThrowsAsync<ConflictoException>(
            () => CrearServicio().ReemplazarOrigenesAsync(_vaso.Id, [new(_litro.Id, 4)]));
    }

    [Fact]
    public async Task ReemplazarOrigenes_ListaVacia_QuitaTodosLosOrigenes()
    {
        var resultado = await CrearServicio().ReemplazarOrigenesAsync(_vaso.Id, []);

        Assert.Empty(resultado);
        _porciones.Verify(
            p => p.ReemplazarOrigenesAsync(_vaso.Id, It.Is<IReadOnlyList<PorcionOrigen>>(l => l.Count == 0), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /* ===================== Recetas con porciones ===================== */

    private void ElInsumoEsPorcion(ProductoVariante insumo)
        => _porciones.Setup(p => p.TieneOrigenesAsync(insumo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

    [Fact]
    public async Task AgregarReceta_ConUnaPorcionComoInsumo_LaAcepta()
    {
        var coctel = Variante("Cuba libre", esBase: false);
        ElInsumoEsPorcion(_vaso);
        Receta? agregada = null;
        _productos
            .Setup(p => p.AgregarRecetaAsync(It.IsAny<Receta>(), It.IsAny<CancellationToken>()))
            .Callback<Receta, CancellationToken>((r, _) => { r.VarianteInsumo = _vaso; agregada = r; })
            .Returns(Task.CompletedTask);
        _productos
            .Setup(p => p.ObtenerRecetasAsync(coctel.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => [agregada!]);

        var receta = await CrearServicio().AgregarRecetaAsync(coctel.Id, new RecetaCrearDto(_vaso.Id, 1m));

        Assert.Equal(1m, receta.Cantidad);
        _productos.Verify(p => p.AgregarRecetaAsync(It.Is<Receta>(r => r.VarianteInsumoId == _vaso.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AgregarReceta_PorcionConCantidadFraccionaria_Falla()
    {
        var coctel = Variante("Cuba libre", esBase: false);
        ElInsumoEsPorcion(_vaso);

        var ex = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().AgregarRecetaAsync(coctel.Id, new RecetaCrearDto(_vaso.Id, 0.5m)));

        Assert.Contains("entero", ex.Message);
    }

    [Fact]
    public async Task AgregarReceta_EnUnaPorcion_Falla()
    {
        _porciones.Setup(p => p.TieneOrigenesAsync(_vaso.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().AgregarRecetaAsync(_vaso.Id, new RecetaCrearDto(_litro.Id, 0.25m)));

        Assert.Contains("naturaleza", ex.Message);
    }

    [Fact]
    public async Task AgregarReceta_InsumoSinExistenciaNiPorcion_SigueFallando()
    {
        var coctel = Variante("Cuba libre", esBase: false);
        var sinStock = Variante("Hielo", esBase: false);

        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().AgregarRecetaAsync(coctel.Id, new RecetaCrearDto(sinStock.Id, 1m)));
    }

    /* ===================== Integridad ===================== */

    [Fact]
    public async Task EliminarProducto_ConUnaPresentacionOrigenDeUnaPorcion_LanzaConflicto()
    {
        _productos.Setup(p => p.ObtenerConDetalleAsync(_cola.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_cola);
        _porciones.Setup(p => p.EsOrigenAsync(_litro.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<ConflictoException>(() => CrearServicio().EliminarProductoAsync(_cola.Id));

        Assert.Contains("origen", ex.Message);
        Assert.False(_cola.IsDeleted);
    }

    [Fact]
    public async Task EliminarProducto_ConEnvasesAbiertos_LanzaConflicto()
    {
        _productos.Setup(p => p.ObtenerConDetalleAsync(_cola.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_cola);
        _porciones.Setup(p => p.HayEnvasesAbiertosAsync(_dosLitros.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<ConflictoException>(() => CrearServicio().EliminarProductoAsync(_cola.Id));

        Assert.Contains("envases abiertos", ex.Message);
    }

    [Fact]
    public async Task EliminarProducto_SinVinculos_LoElimina()
    {
        _productos.Setup(p => p.ObtenerConDetalleAsync(_cola.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_cola);
        _productos.Setup(p => p.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        Assert.True(await CrearServicio().EliminarProductoAsync(_cola.Id));
        Assert.True(_cola.IsDeleted);
    }

    /* ===================== DTO con naturaleza y disponibilidad ===================== */

    [Fact]
    public async Task ObtenerProducto_Coca_ColaConPresentacionesYVaso_InformaNaturalezaYDisponibilidad()
    {
        var heredada = Variante("Litro heredado", esBase: false);
        _productos.Setup(p => p.ObtenerConDetalleAsync(_cola.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_cola);
        _inventario
            .Setup(i => i.ObtenerStockDeVariantesAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([Stock(_litro, 3m), Stock(_dosLitros, 5m)]);
        _porciones
            .Setup(p => p.ObtenerOrigenesDeVariantesAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new PorcionOrigen { PorcionVarianteId = _vaso.Id, PresentacionVarianteId = _litro.Id, PorcionesPorUnidad = 4 },
                new PorcionOrigen { PorcionVarianteId = _vaso.Id, PresentacionVarianteId = _dosLitros.Id, PorcionesPorUnidad = 8 }
            ]);
        _porciones
            .Setup(p => p.ObtenerResumenAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ResumenPorcion> { [_vaso.Id] = new ResumenPorcion(4, 1) });
        _porciones
            .Setup(p => p.ObtenerIdsConRecetaAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<Guid> { heredada.Id });

        var producto = await CrearServicio().ObtenerProductoAsync(_cola.Id);

        var porNombre = producto!.Variantes.ToDictionary(v => v.Nombre);
        Assert.Equal(NaturalezaVariante.Presentacion, porNombre["Botella 1 L"].Naturaleza);
        Assert.Equal(NaturalezaVariante.Presentacion, porNombre["Botella 2 L"].Naturaleza);
        Assert.Equal(NaturalezaVariante.Receta, porNombre["Litro heredado"].Naturaleza);

        var vaso = porNombre["Vaso"];
        Assert.Equal(NaturalezaVariante.Porcion, vaso.Naturaleza);
        Assert.Equal(3, vaso.PorcionesDisponibles);            // 4 abiertas - 1 reservada
        Assert.Equal(8, vaso.EnvasesCerradosAbribles);         // 3 botellas de 1 L + 5 de 2 L
        Assert.Equal(3 * 4 + 5 * 8, vaso.PorcionesPotenciales);
    }

    [Fact]
    public async Task ObtenerProducto_LasBotellasReservadasNoCuentanComoAbribles()
    {
        _productos.Setup(p => p.ObtenerConDetalleAsync(_cola.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_cola);
        _inventario
            .Setup(i => i.ObtenerStockDeVariantesAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([Stock(_litro, 2m, reservada: 2m)]);
        _porciones
            .Setup(p => p.ObtenerOrigenesDeVariantesAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PorcionOrigen { PorcionVarianteId = _vaso.Id, PresentacionVarianteId = _litro.Id, PorcionesPorUnidad = 4 }]);

        var producto = await CrearServicio().ObtenerProductoAsync(_cola.Id);

        Assert.Equal(0, producto!.Variantes.Single(v => v.Nombre == "Vaso").EnvasesCerradosAbribles);
    }
}
