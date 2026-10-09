using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.UnitTests;

public class ProductoTests
{
    private static Producto CrearProducto()
        => new()
        {
            Nombre = "Ron Cacique",
            Descripcion = "Ron añejo venezolano.",
            CategoriaId = Guid.NewGuid()
        };

    [Fact]
    public void Constructor_InicializaElEstadoPorDefecto()
    {
        var producto = CrearProducto();

        Assert.True(producto.Activo);
        Assert.Null(producto.LastModifiedAt);
        Assert.Empty(producto.Variantes);
    }

    [Fact]
    public void ActualizarDatos_RegistraLaModificacion()
    {
        var producto = CrearProducto();

        producto.ActualizarDatos("Ron Cacique Añejo", "Nueva descripción", producto.CategoriaId, null, null, producto.Tipo, AreaDestino.Barra, null, null);

        Assert.Equal("Ron Cacique Añejo", producto.Nombre);
        Assert.NotNull(producto.LastModifiedAt);
    }

    [Fact]
    public void ActualizarDatos_SinNombre_Lanza()
    {
        var producto = CrearProducto();

        Assert.Throws<InvalidOperationException>(() =>
            producto.ActualizarDatos("", null, producto.CategoriaId, null, null, producto.Tipo, AreaDestino.Barra, null, null));
    }

    [Fact]
    public void Desactivar_CambiaElEstado()
    {
        var producto = CrearProducto();

        producto.Desactivar();

        Assert.False(producto.Activo);
    }

    [Fact]
    public void Variante_ActualizarDatos_ConVentaMenorAlCosto_Lanza()
    {
        var variante = new ProductoVariante();

        Assert.Throws<InvalidOperationException>(() =>
            variante.ActualizarDatos("Botella", "SKU-1", Guid.NewGuid(), 10m, 9m));
    }
}
