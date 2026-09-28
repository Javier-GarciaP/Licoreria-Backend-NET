namespace Licoreria.Domain.Services;

/// <summary>
/// Rango temporal de ocupación de una mesa.
/// </summary>
public sealed record IntervaloReserva(Guid MesaId, DateTime Inicio, DateTime Fin);

/// <summary>
/// Detecta solapamientos de reservas para una misma mesa.
/// </summary>
public sealed class DetectorConflictosReserva
{
    public bool HayConflicto(IntervaloReserva existente, IntervaloReserva candidata)
    {
        if (existente.Fin <= existente.Inicio || candidata.Fin <= candidata.Inicio)
        {
            throw new InvalidOperationException("El intervalo de reserva no es válido.");
        }

        return existente.MesaId == candidata.MesaId
            && candidata.Inicio < existente.Fin
            && existente.Inicio < candidata.Fin;
    }

    public bool HayConflicto(IEnumerable<IntervaloReserva> existentes, IntervaloReserva candidata)
        => existentes.Any(existente => HayConflicto(existente, candidata));
}
