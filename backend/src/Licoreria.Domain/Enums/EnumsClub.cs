namespace Licoreria.Domain.Enums;

/// <summary>Tipo de zona del local.</summary>
public enum TipoZona
{
    Barra = 1,
    Mesas = 2,
    Juegos = 3,
    Pista = 4,
    Vip = 5
}

/// <summary>Estado de una reserva.</summary>
public enum EstadoReserva
{
    Pendiente = 1,
    Confirmada = 2,
    Cancelada = 3,
    Asistio = 4,
    NoAsistio = 5
}

/// <summary>Canal de origen de una reserva.</summary>
public enum OrigenReserva
{
    Web = 1,
    Whatsapp = 2,
    Presencial = 3
}

/// <summary>Estado de validación de la seña de una reserva.</summary>
public enum EstadoReservaPago
{
    Pendiente = 1,
    Validado = 2,
    Rechazado = 3
}
