using Licoreria.Application.Dtos;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Casos de uso relacionados con productos: salud de stock y generación de SKU.
/// </summary>
public interface IServicioProducto
{
    InformeSaludStockDto EvaluarSalud(SaludStockRequest request);

    ResultadoSkuDto GenerarSku(SkuRequest request);
}
