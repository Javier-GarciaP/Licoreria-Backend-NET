using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Billete o moneda usado en el arqueo de caja.
/// </summary>
public class Denominacion : BaseEntity
{
    public Moneda Moneda { get; set; }
    public TipoDenominacion Tipo { get; set; }
    public decimal Valor { get; set; }
    public bool Activo { get; set; } = true;
}
