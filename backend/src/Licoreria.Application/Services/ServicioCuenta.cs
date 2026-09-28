using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Services;

namespace Licoreria.Application.Services;

public sealed class ServicioCuenta : IServicioCuenta
{
    private readonly CalculadoraCuenta _calculadoraCuenta;

    public ServicioCuenta(CalculadoraCuenta calculadoraCuenta)
    {
        _calculadoraCuenta = calculadoraCuenta;
    }

    public ResumenCuentaDto Calcular(IEnumerable<decimal> consumos, IEnumerable<decimal> abonos)
    {
        var resumen = _calculadoraCuenta.Calcular(consumos, abonos);

        return new ResumenCuentaDto(
            resumen.Total,
            resumen.TotalAbonado,
            resumen.Saldo,
            resumen.EstaSaldada);
    }

    public void ValidarAbono(AbonoRequest request)
        => _calculadoraCuenta.ValidarAbono(request.Total, request.TotalAbonado, request.MontoAbono);
}
