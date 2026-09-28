namespace Licoreria.Domain.Enums;

/// <summary>
/// Clasificación del estado de salud del inventario de un producto.
/// </summary>
public enum EstadoSaludStock
{
    SinStock = 1,
    RiesgoCritico = 2,
    Subabastecido = 3,
    Optimo = 4,
    Sobreabastecido = 5,
    Excesivo = 6
}
