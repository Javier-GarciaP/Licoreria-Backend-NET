using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>
/// Pruebas deterministas de la lógica de negocio de productos en
/// <see cref="ServicioCatalogo"/>. El repositorio de datos
/// (<see cref="IProductoRepository"/>) se aísla con Moq, por lo que no se
/// requiere conexión real a la base de datos.
/// </summary>
public sealed class ServicioCatalogoProductoTests
{
    private readonly Mock<IProductoRepository> _productos = new();
    private readonly Mock<ICategoriaRepository> _categorias = new();

    public ServicioCatalogoProductoTests()
    {
        // El resto de repositorios se proveen como mocks neutros.
        _ = new Mock<IRepository<Marca>>();
        _ = new Mock<IRepository<UnidadMedida>>();
        _ = new Mock<IRepository<Impuesto>>();
        _ = new Mock<IRepository<ListaPrecio>>();
        _ = new Mock<IRepository<ProductoVariante>>();
        _ = new Mock<IRepository<Modificador>>();
        _ = new Mock<IRepository<ProductoModificador>>();
    }

    private ServicioCatalogo CrearServicio()
        => new(
            _productos.Object,
            _categorias.Object,
            Mock.Of<IRepository<Marca>>(),
            Mock.Of<IRepository<UnidadMedida>>(),
            Mock.Of<IRepository<Impuesto>>(),
            Mock.Of<IRepository<ListaPrecio>>(),
            Mock.Of<IRepository<ProductoVariante>>(),
            Mock.Of<IRepository<Modificador>>(),
            Mock.Of<IRepository<ProductoModificador>>());

    private static ProductoCrearDto CrearDto(params VarianteCrearDto[] variantes)
        => new(
            "Whisky Escocés 12 años",
            "Botella de 750ml",
            Guid.NewGuid(),
            null,
            null,
            TipoProducto.Simple,
            40m,
            null,
            variantes);

    private static VarianteCrearDto Variante(string sku = "WHI-750-001")
        => new(null, "Whisky 750ml", sku, Guid.NewGuid(), 10m, 20m, []);

    private void CategoriaExiste()
        => _categorias
            .Setup(c => c.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Categoria { Nombre = "Destilados", Activo = true });

    [Fact]
    public async Task CrearProductoAsync_SinVariantes_LanzaReglaNegocio()
    {
        CategoriaExiste();
        var servicio = CrearServicio();

        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => servicio.CrearProductoAsync(CrearDto(), CancellationToken.None));
    }

    [Fact]
    public async Task CrearProductoAsync_CategoriaInexistente_LanzaNoEncontrado()
    {
        _categorias
            .Setup(c => c.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Categoria?)null);
        var servicio = CrearServicio();

        await Assert.ThrowsAsync<NoEncontradoException>(
            () => servicio.CrearProductoAsync(CrearDto(Variante()), CancellationToken.None));
    }

    [Fact]
    public async Task CrearProductoAsync_SkuDuplicado_LanzaConflicto()
    {
        CategoriaExiste();
        _productos
            .Setup(r => r.ExisteSkuAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var servicio = CrearServicio();

        await Assert.ThrowsAsync<ConflictoException>(
            () => servicio.CrearProductoAsync(CrearDto(Variante()), CancellationToken.None));
    }

    [Fact]
    public async Task CrearProductoAsync_CodigoBarrasDuplicado_LanzaConflicto()
    {
        CategoriaExiste();
        _productos
            .Setup(r => r.ExisteSkuAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _productos
            .Setup(r => r.ExisteCodigoBarrasAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var variante = Variante() with { CodigosBarras = ["1234567890"] };
        var servicio = CrearServicio();

        await Assert.ThrowsAsync<ConflictoException>(
            () => servicio.CrearProductoAsync(CrearDto(variante), CancellationToken.None));
    }

    [Fact]
    public async Task CrearProductoAsync_Valido_PersisteYDevuelveProducto()
    {
        CategoriaExiste();
        _productos
            .Setup(r => r.ExisteSkuAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _productos
            .Setup(r => r.ExisteCodigoBarrasAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _productos
            .Setup(r => r.ObtenerConDetalleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Producto { Nombre = "Whisky Escocés 12 años", Activo = true });

        var servicio = CrearServicio();

        var resultado = await servicio.CrearProductoAsync(CrearDto(Variante()), CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal("Whisky Escocés 12 años", resultado.Nombre);
        _productos.Verify(
            r => r.AddAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _productos.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ObtenerProductosAsync_MapeaResultadoPaginado()
    {
        var producto = new Producto { Nombre = "Ron Añejo", Activo = true };
        _productos
            .Setup(r => r.ObtenerPaginadoAsync(
                It.IsAny<PaginacionRequest>(),
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultadoPaginado<Producto>.Crear([producto], 2, 20, 45));

        var servicio = CrearServicio();

        var resultado = await servicio.ObtenerProductosAsync(new PaginacionRequest { Page = 2, PageSize = 20 });

        Assert.Single(resultado.Items);
        Assert.Equal(45, resultado.TotalItems);
        Assert.Equal(3, resultado.TotalPages);
    }

    [Fact]
    public async Task EditarProductoAsync_Inexistente_DevuelveNull()
    {
        _productos
            .Setup(r => r.ObtenerConDetalleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Producto?)null);

        var servicio = CrearServicio();

        var resultado = await servicio.EditarProductoAsync(
            new ProductoEditarDto(Guid.NewGuid(), "x", null, Guid.NewGuid(), null, null,
                TipoProducto.Simple, null, null, true, [Variante()]));

        Assert.Null(resultado);
    }

    [Fact]
    public async Task EliminarProductoAsync_Inexistente_DevuelveFalse()
    {
        _productos
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Producto?)null);

        var servicio = CrearServicio();

        Assert.False(await servicio.EliminarProductoAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task EliminarProductoAsync_Existente_AplicaBorradoLogico()
    {
        var producto = new Producto { Nombre = "Vodka", Activo = true };
        _productos
            .Setup(r => r.GetByIdAsync(producto.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);

        var servicio = CrearServicio();

        var resultado = await servicio.EliminarProductoAsync(producto.Id);

        Assert.True(resultado);
        Assert.True(producto.IsDeleted);
        _productos.Verify(r => r.Update(producto), Times.Once);
        _productos.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
