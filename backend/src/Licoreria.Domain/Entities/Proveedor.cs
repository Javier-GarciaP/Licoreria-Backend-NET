using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>Proveedor del local con datos fiscales y de contacto.</summary>
public class Proveedor : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Rif { get; set; }
    public string? Contacto { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public int DiasCredito { get; set; }
    public bool Activo { get; set; } = true;
}

/// <summary>Orden de compra a un proveedor.</summary>
public class OrdenCompra : BaseEntity
{
    public string Numero { get; set; } = string.Empty;
    public Guid ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;

    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public EstadoOrdenCompra Estado { get; set; } = EstadoOrdenCompra.Borrador;
    public string? Observaciones { get; set; }
    public decimal TotalUSD { get; private set; }

    public ICollection<OrdenCompraDetalle> Detalles { get; set; } = new List<OrdenCompraDetalle>();

    public void RecalcularTotal() => TotalUSD = Detalles.Sum(d => d.SubtotalUSD);

    public void Aprobar() => Estado = EstadoOrdenCompra.Aprobada;
    public void Enviar() => Estado = EstadoOrdenCompra.Enviada;
    public void Cancelar() => Estado = EstadoOrdenCompra.Cancelada;

    public void ActualizarEstadoRecepcion()
    {
        Estado = Detalles.All(d => d.CantidadRecibida >= d.Cantidad)
            ? EstadoOrdenCompra.Recibida
            : EstadoOrdenCompra.RecibidaParcial;
    }
}

/// <summary>Línea de una orden de compra.</summary>
public class OrdenCompraDetalle : BaseEntity
{
    public Guid OrdenCompraId { get; set; }
    public OrdenCompra OrdenCompra { get; set; } = null!;

    public Guid VarianteId { get; set; }
    public ProductoVariante Variante { get; set; } = null!;

    public decimal Cantidad { get; set; }
    public decimal CostoUnitarioUSD { get; set; }
    public decimal CantidadRecibida { get; set; }

    public decimal SubtotalUSD => Cantidad * CostoUnitarioUSD;

    public void Recibir(decimal cantidad)
    {
        if (cantidad <= 0)
        {
            throw new InvalidOperationException("La cantidad a recibir debe ser mayor que cero.");
        }

        if (CantidadRecibida + cantidad > Cantidad)
        {
            throw new InvalidOperationException("La cantidad recibida no puede superar la ordenada.");
        }

        CantidadRecibida += cantidad;
        MarcarModificado();
    }
}
