using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Services;

namespace Licoreria.Application.Services;

public sealed class ServicioProducto : IServicioProducto
{
    private readonly EvaluadorSaludStock _evaluadorSaludStock;
    private readonly GeneradorSku _generadorSku;

    public ServicioProducto(EvaluadorSaludStock evaluadorSaludStock, GeneradorSku generadorSku)
    {
        _evaluadorSaludStock = evaluadorSaludStock;
        _generadorSku = generadorSku;
    }

    public InformeSaludStockDto EvaluarSalud(SaludStockRequest request)
    {
        var informe = _evaluadorSaludStock.Evaluar(
            request.Sku,
            request.StockActual,
            request.StockMinimo,
            request.StockMaximo);

        return new InformeSaludStockDto(
            informe.Sku,
            informe.StockActual,
            informe.StockMinimo,
            informe.StockMaximo,
            informe.Estado.ToString(),
            informe.MensajeDiagnostico,
            informe.UnidadesCompraSugeridas);
    }

    public ResultadoSkuDto GenerarSku(SkuRequest request)
    {
        var resultado = _generadorSku.Generar(
            request.Categoria,
            request.Producto,
            request.Secuencia);

        return new ResultadoSkuDto(
            resultado.EntradaOriginal,
            resultado.Categoria,
            resultado.NombreSanitizado,
            resultado.SkuGenerado,
            resultado.FormulaSku,
            resultado.EsValidoEmpresarial);
    }
}
