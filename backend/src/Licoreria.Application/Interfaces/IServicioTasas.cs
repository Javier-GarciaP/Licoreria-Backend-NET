namespace Licoreria.Application.Interfaces;

/// <summary>
/// Gestiona la tasa de cambio vigente en memoria.
/// </summary>
public interface IServicioTasas
{
    decimal ObtenerTasaActual();

    void ActualizarTasa(decimal tasa);
}
