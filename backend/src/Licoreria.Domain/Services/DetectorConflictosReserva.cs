namespace Licoreria.Domain.Services;

/// <summary>
/// Rango temporal de ocupación de una mesa.
/// </summary>
public sealed record IntervaloReserva(Guid MesaId, DateTime Inicio, DateTime Fin)
{
    /// <summary>
    /// Intervalo presunto de una reserva que solo guarda un horario único.
    /// </summary>
    public static IntervaloReserva Presunto(Guid mesaId, DateTime inicio, int horas)
        => new(mesaId, inicio, inicio.AddHours(horas));
}

/// <summary>
/// Detecta solapamientos de reservas para una misma mesa.
/// </summary>
public sealed class DetectorConflictosReserva
{
    /// <summary>
    /// Duración que asumimos que ocupa una reserva aunque el modelo solo guarde un
    /// horario único. Tanto la disponibilidad como la validación de solapamientos
    /// se calculan sobre este intervalo presunto.
    /// </summary>
    public static readonly int DuracionReservaPresuntaHoras = 2;

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
