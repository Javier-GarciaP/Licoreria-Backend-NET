namespace Licoreria.Domain.Services;

/// <summary>
/// Resumen del estado financiero de una cuenta de mesa.
/// </summary>
public sealed record ResumenCuenta(
    decimal Total,
    decimal TotalAbonado,
    decimal Saldo,
    bool EstaSaldada);

/// <summary>
/// Calcula los totales de una cuenta y valida los abonos.
/// </summary>
public sealed class CalculadoraCuenta
{
    public ResumenCuenta Calcular(IEnumerable<decimal> consumos, IEnumerable<decimal> abonos)
    {
        var total = consumos?.Sum() ?? 0m;
        var totalAbonado = abonos?.Sum() ?? 0m;
        var saldo = total - totalAbonado;

        return new ResumenCuenta(total, totalAbonado, saldo, saldo <= 0m);
    }

    public void ValidarAbono(decimal total, decimal totalAbonado, decimal montoAbono)
    {
        if (montoAbono <= 0m)
        {
            throw new InvalidOperationException("El abono debe ser mayor que cero.");
        }

        if (totalAbonado + montoAbono > total)
        {
            throw new InvalidOperationException("El abono excede el saldo pendiente de la cuenta.");
        }
    }
}
