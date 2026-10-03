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

    public decimal SubtotalUSD => EsCortesia ? 0m : Cantidad * PrecioUnitarioUSD;

    public void CambiarEstado(EstadoItemComanda estado)
    {
        Estado = estado;
        MarcarModificado();
    }
}
