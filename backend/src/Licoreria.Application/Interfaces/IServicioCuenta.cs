using Licoreria.Application.Dtos;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Casos de uso de cuentas: cálculo de totales y validación de abonos.
/// </summary>
public interface IServicioCuenta
{
    ResumenCuentaDto Calcular(IEnumerable<decimal> consumos, IEnumerable<decimal> abonos);

    void ValidarAbono(AbonoRequest request);
}
