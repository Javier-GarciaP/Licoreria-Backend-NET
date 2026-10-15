using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Línea de una comanda con su área destino y estado.
/// </summary>
public class ComandaDetalle : BaseEntity
{
    public Guid ComandaId { get; set; }
    public Comanda Comanda { get; set; } = null!;

    public Guid VarianteId { get; set; }
    public ProductoVariante Variante { get; set; } = null!;

    public decimal Cantidad { get; set; }
    public decimal PrecioUnitarioUSD { get; set; }
    public AreaDestino AreaDestino { get; set; }
    public EstadoItemComanda Estado { get; set; } = EstadoItemComanda.Recibido;
    public bool EsCortesia { get; set; }

    /// <summary>Indica que los insumos del ítem están reservados y aún no se sirvieron ni liberaron.</summary>
    public bool Reservado { get; set; }

    /// <summary>
    /// Momento en que el ítem se sirvió por primera vez (Preparado o Entregado). Es nulo en los
    /// ítems anteriores a esta marca o que aún no se sirvieron, y nunca se sobrescribe.
    /// </summary>
    public DateTime? ServidoEn { get; set; }

    public decimal SubtotalUSD => EsCortesia ? 0m : Cantidad * PrecioUnitarioUSD;

    public void CambiarEstado(EstadoItemComanda estado, DateTime? ahora = null)
    {
        Estado = estado;

        if (ServidoEn is null && estado is EstadoItemComanda.Preparado or EstadoItemComanda.Entregado)
        {
            ServidoEn = ahora ?? DateTime.UtcNow;
        }

        MarcarModificado();
    }
}
