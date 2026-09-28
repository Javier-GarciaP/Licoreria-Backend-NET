namespace Licoreria.Domain.Services;

/// <summary>
/// Conversión de montos entre dólares (USD) y bolívares (BS) según la tasa vigente.
/// </summary>
public sealed class ConversorMoneda
{
    public decimal ConvertirAusdBolivares(decimal montoUsd, decimal tasaCambio)
    {
        ValidarMonto(montoUsd);
        ValidarTasa(tasaCambio);

        return Math.Round(montoUsd * tasaCambio, 2, MidpointRounding.AwayFromZero);
    }

    public decimal ConvertirDeBsAUsd(decimal montoBs, decimal tasaCambio)
    {
        ValidarMonto(montoBs);
        ValidarTasa(tasaCambio);

        return Math.Round(montoBs / tasaCambio, 2, MidpointRounding.AwayFromZero);
    }

    private static void ValidarMonto(decimal monto)
    {
        if (monto < 0m)
        {
            throw new InvalidOperationException("El monto no puede ser negativo.");
        }
    }

    private static void ValidarTasa(decimal tasaCambio)
    {
        if (tasaCambio <= 0m)
        {
            throw new InvalidOperationException("La tasa de cambio debe ser mayor que cero.");
        }
    }
}
