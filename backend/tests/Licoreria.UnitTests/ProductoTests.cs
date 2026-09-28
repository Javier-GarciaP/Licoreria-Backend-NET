using Licoreria.Domain.Entities;

namespace Licoreria.UnitTests;

public class ProductoTests
{
    private static Producto CrearProducto()
        => new(
            nombre: "Ron Cacique",
            sku: "LIC-RON-0001",
            codigoBarras: "759100100101",
            precioCompraUSD: 8.50m,
            precioVentaUSD: 12.00m,
            stockMinimo: 5,
            stockMaximo: 60,
            categoriaId: Guid.NewGuid());

    [Fact]
    public void Constructor_InicializaElEstadoPorDefecto()
    {
        var producto = CrearProducto();

        Assert.Equal(0, producto.Stock);
        Assert.True(producto.Activo);
        Assert.Null(producto.LastModifiedAt);
    }

    [Fact]
    public void ActualizarStock_IncrementaYRegistraModificacion()
    {
        var producto = CrearProducto();

        producto.ActualizarStock(10);

        Assert.Equal(10, producto.Stock);
        Assert.NotNull(producto.LastModifiedAt);
    }

    [Fact]
    public void ActualizarStock_QueDejaStockNegativo_Lanza()
    {
        var producto = CrearProducto();

        Assert.Throws<InvalidOperationException>(() => producto.ActualizarStock(-1));
    }

    [Fact]
    public void ActualizarPrecios_ConVentaMenorAlCosto_Lanza()
    {
        var producto = CrearProducto();

        Assert.Throws<InvalidOperationException>(() => producto.ActualizarPrecios(10m, 9m));
    }

    [Fact]
    public void Desactivar_CambiaElEstado()
    {
        var producto = CrearProducto();

        producto.Desactivar();

        Assert.False(producto.Activo);
    }
}
