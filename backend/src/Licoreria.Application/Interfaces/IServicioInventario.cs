using Licoreria.Application.Dtos;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Casos de uso de inventario: mermas, cortesías y conversión de moneda.
/// </summary>
public interface IServicioInventario
{
    ResultadoMermaDto EvaluarMerma(MermaRequest request);

    decimal ConvertirAusdBolivares(decimal montoUsd, decimal tasaCambio);
}
