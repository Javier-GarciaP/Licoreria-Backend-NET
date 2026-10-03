using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Tasa de cambio vigente para una fecha y tipo (BCV/paralelo).
/// </summary>
public class TasaCambio : BaseEntity
{
    public DateTime Fecha { get; set; }
    public TipoTasa Tipo { get; set; }
    public decimal Valor { get; set; }

    public void ActualizarValor(decimal valor)
    {
        if (valor <= 0)
        {
            throw new InvalidOperationException("La tasa de cambio debe ser mayor que cero.");
        }

        Valor = valor;
        MarcarModificado();
    }
}
