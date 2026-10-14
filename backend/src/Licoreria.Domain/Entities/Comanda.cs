using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Pedido enviado a un área (barra o cocina) dentro de una cuenta.
/// </summary>
public class Comanda : BaseEntity
{
    public Guid CuentaId { get; set; }
    public Cuenta Cuenta { get; set; } = null!;

    public AreaDestino Area { get; set; }
    public EstadoComanda Estado { get; set; } = EstadoComanda.Pendiente;

    public ICollection<ComandaDetalle> Detalles { get; set; } = new List<ComandaDetalle>();
}
