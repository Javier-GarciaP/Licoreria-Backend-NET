using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

public class Producto : BaseEntity
{
    public string Nombre { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public string CodigoBarras { get; private set; } = string.Empty;

    public decimal PrecioCompraUSD { get; private set; }
    public decimal PrecioVentaUSD { get; private set; }

    public int Stock { get; private set; }
    public int StockMinimo { get; private set; }
    public int StockMaximo { get; private set; }

    public string? ImagenUrl { get; private set; }
    public bool Activo { get; private set; } = true;

    public Guid CategoriaId { get; private set; }
    public Categoria Categoria { get; private set; } = null!;

    /// <summary>
    /// Constructor sin parámetros requerido por el ORM para la materialización.
    /// </summary>
    private Producto()
    {
    }

    public Producto(
        string nombre,
        string sku,
        string codigoBarras,
        decimal precioCompraUSD,
        decimal precioVentaUSD,
        int stockMinimo,
        int stockMaximo,
        Guid categoriaId)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new InvalidOperationException("El nombre del producto es obligatorio.");
        }

        if (stockMinimo < 0 || stockMaximo < stockMinimo)
        {
            throw new InvalidOperationException("Los límites de stock no son válidos.");
        }

        Nombre = nombre;
        Sku = sku;
        CodigoBarras = codigoBarras;
        PrecioCompraUSD = precioCompraUSD;
        PrecioVentaUSD = precioVentaUSD;
        StockMinimo = stockMinimo;
        StockMaximo = stockMaximo;
        CategoriaId = categoriaId;
        Stock = 0;
        Activo = true;
    }

    /// <summary>
    /// Incrementa o decrementa el stock. Un resultado negativo viola la regla de negocio.
    /// </summary>
    public void ActualizarStock(int cantidad)
    {
        var nuevoStock = Stock + cantidad;

        if (nuevoStock < 0)
        {
            throw new InvalidOperationException("El stock no puede quedar en negativo.");
        }

        Stock = nuevoStock;
        MarcarModificado();
    }

    public void ActualizarPrecios(decimal precioCompraUSD, decimal precioVentaUSD)
    {
        if (precioCompraUSD < 0 || precioVentaUSD < 0)
        {
            throw new InvalidOperationException("Los precios no pueden ser negativos.");
        }

        if (precioVentaUSD < precioCompraUSD)
        {
            throw new InvalidOperationException("El precio de venta no puede ser menor que el de compra.");
        }

        PrecioCompraUSD = precioCompraUSD;
        PrecioVentaUSD = precioVentaUSD;
        MarcarModificado();
    }

    public void ActualizarDatos(string nombre, string codigoBarras, string? imagenUrl)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new InvalidOperationException("El nombre del producto es obligatorio.");
        }

        Nombre = nombre;
        CodigoBarras = codigoBarras;
        ImagenUrl = imagenUrl;
        MarcarModificado();
    }

    public void Activar()
    {
        Activo = true;
        MarcarModificado();
    }

    public void Desactivar()
    {
        Activo = false;
        MarcarModificado();
    }
}
