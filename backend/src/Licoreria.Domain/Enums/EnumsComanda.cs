namespace Licoreria.Domain.Enums;

/// <summary>Estado de una cuenta de mesa.</summary>
public enum EstadoCuenta
{
    Abierta = 1,
    PorCobrar = 2,
    Cerrada = 3
}

/// <summary>Área a la que se enruta una comanda.</summary>
public enum AreaDestino
{
    Barra = 1,
    Cocina = 2
}

/// <summary>Estado general de una comanda.</summary>
public enum EstadoComanda
{
    Pendiente = 1,
    EnPreparacion = 2,
    Lista = 3,
    Entregada = 4,
    Cancelada = 5
}

/// <summary>Estado de cada línea de una comanda.</summary>
public enum EstadoItemComanda
{
    Recibido = 1,
    Preparado = 2,
    Entregado = 3,
    Cancelado = 4,
    EnProceso = 5
}
