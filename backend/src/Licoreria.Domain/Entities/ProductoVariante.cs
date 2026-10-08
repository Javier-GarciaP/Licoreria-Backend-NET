using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Presentación vendible de un producto (botella, tobo, unidad). Concentra el SKU,
/// la unidad de venta y los precios.
/// </summary>
public class ProductoVariante : BaseEntity
{
    public Guid ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal PrecioCompraUSD { get; set; }
    public decimal PrecioVentaUSD { get; set; }
    public bool Activo { get; set; } = true;

    public Guid UnidadMedidaId { get; set; }
    public UnidadMedida UnidadMedida { get; set; } = null!;

    public ICollection<CodigoBarras> CodigosBarras { get; set; } = new List<CodigoBarras>();

    public void ActualizarDatos(
        string nombre,
        string sku,
        Guid unidadMedidaId,
        decimal precioCompraUsd,
        decimal precioVentaUsd)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new InvalidOperationException("El nombre de la variante es obligatorio.");
        }

        if (precioCompraUsd < 0 || precioVentaUsd < 0)
        {
            throw new InvalidOperationException("Los precios no pueden ser negativos.");
        }

        if (precioVentaUsd < precioCompraUsd)
        {
            throw new InvalidOperationException("El precio de venta no puede ser menor que el de compra.");
        }

        Nombre = nombre;
        Sku = sku;
        UnidadMedidaId = unidadMedidaId;
        PrecioCompraUSD = precioCompraUsd;
        PrecioVentaUSD = precioVentaUsd;
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
