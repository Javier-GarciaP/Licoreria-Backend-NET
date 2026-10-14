using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>Parte de una cuenta dividida.</summary>
public class CuentaDivision : BaseEntity
{
    public Guid CuentaId { get; set; }
    public Cuenta Cuenta { get; set; } = null!;

    public int Indice { get; set; }
    public decimal Monto { get; set; }
    public bool Pagada { get; private set; }

    public void Pagar()
    {
        Pagada = true;
        MarcarModificado();
    }
}

/// <summary>Promoción o descuento aplicable a las ventas.</summary>
public class Promocion : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public TipoPromocion Tipo { get; set; }
    public decimal Valor { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }

    public bool EstaVigente(DateTime ahora)
        => Activo
           && (FechaInicio is null || FechaInicio <= ahora)
           && (FechaFin is null || FechaFin >= ahora);

    public decimal CalcularDescuento(decimal subtotal)
        => Tipo == TipoPromocion.Porcentaje
            ? Math.Round(subtotal * Valor / 100m, 2)
            : Math.Min(Valor, subtotal);
}
