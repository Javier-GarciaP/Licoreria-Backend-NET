namespace Licoreria.Domain.Enums;

/// <summary>
/// Clasificación de los movimientos del kardex de inventario.
/// </summary>
public enum TipoMovimientoInventario
{
    Compra = 1,
    Venta = 2,
    Ajuste = 3,
    Merma = 4,
    Cortesia = 5,
    ConsumoInterno = 6
}
