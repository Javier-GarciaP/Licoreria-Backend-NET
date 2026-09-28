using Licoreria.Application.Interfaces;

namespace Licoreria.Infrastructure.Services;

/// <summary>
/// Cache en memoria de la tasa de cambio vigente.
/// Se registra como Singleton: una única instancia durante toda la vida de la aplicación.
/// </summary>
public sealed class CacheTasasEnMemoria : IServicioTasas
{
    private decimal _tasaActual;

    public decimal ObtenerTasaActual() => _tasaActual;

    public void ActualizarTasa(decimal tasa)
    {
        if (tasa <= 0m)
        {
            throw new InvalidOperationException("La tasa de cambio debe ser mayor que cero.");
        }

        _tasaActual = tasa;
    }
}
