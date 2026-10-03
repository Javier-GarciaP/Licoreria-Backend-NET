namespace Licoreria.Domain.Enums;

/// <summary>Naturaleza de un movimiento de puntos de fidelidad.</summary>
public enum TipoMovimientoPuntos
{
    Acumulacion = 1,
    Canje = 2
}

/// <summary>Estado de una cuenta por cobrar.</summary>
public enum EstadoCuentaPorCobrar
{
    Pendiente = 1,
    Pagada = 2,
    Vencida = 3
}
