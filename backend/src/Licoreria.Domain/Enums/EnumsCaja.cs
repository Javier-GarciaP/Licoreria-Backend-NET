namespace Licoreria.Domain.Enums;

/// <summary>Estado de una sesión de caja.</summary>
public enum EstadoSesionCaja
{
    Abierta = 1,
    Cerrada = 2
}

/// <summary>Naturaleza de un movimiento de caja.</summary>
public enum TipoMovimientoCaja
{
    Ingreso = 1,
    Egreso = 2
}

/// <summary>Tipo de denominación para el arqueo.</summary>
public enum TipoDenominacion
{
    Billete = 1,
    Moneda = 2
}
