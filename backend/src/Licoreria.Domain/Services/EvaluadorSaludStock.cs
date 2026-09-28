using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Services;

/// <summary>
/// Resultado del diagnóstico de salud del inventario para un producto.
/// </summary>
public sealed record InformeSaludStock(
    string Sku,
    int StockActual,
    int StockMinimo,
    int StockMaximo,
    EstadoSaludStock Estado,
    string MensajeDiagnostico,
    int UnidadesCompraSugeridas);

/// <summary>
/// Evalúa la salud del inventario de un producto a partir de sus umbrales de stock.
/// Lógica pura del dominio, sin dependencias de infraestructura.
/// </summary>
public sealed class EvaluadorSaludStock
{
    public InformeSaludStock Evaluar(string sku, int stockActual, int stockMinimo, int stockMaximo)
    {
        if (stockMinimo < 0 || stockMaximo < stockMinimo)
        {
            throw new InvalidOperationException(
                "Los umbrales de stock no son válidos: el máximo debe ser mayor o igual al mínimo.");
        }

        var (estado, mensaje, unidadesSugeridas) = (stockActual, stockMinimo, stockMaximo) switch
        {
            (var stock, _, _) when stock <= 0 => (
                EstadoSaludStock.SinStock,
                "Producto agotado. Se requiere reposición inmediata.",
                stockMaximo),

            (var stock, var minimo, _) when stock <= minimo => (
                EstadoSaludStock.RiesgoCritico,
                "Existencias en el umbral mínimo. Riesgo de desabastecimiento.",
                stockMaximo - stock),

            (var stock, var minimo, var maximo) when stock <= minimo + (maximo - minimo) / 2 => (
                EstadoSaludStock.Subabastecido,
                "Existencias por debajo del punto medio recomendado.",
                maximo - stock),

            (var stock, _, var maximo) when stock < maximo => (
                EstadoSaludStock.Optimo,
                "Nivel de existencias óptimo.",
                0),

            (var stock, _, var maximo) when stock == maximo => (
                EstadoSaludStock.Sobreabastecido,
                "Existencias en el límite máximo. Evitar nuevas compras.",
                0),

            _ => (
                EstadoSaludStock.Excesivo,
                "Existencias por encima del máximo. Riesgo de capital inmovilizado.",
                0)
        };

        return new InformeSaludStock(
            sku,
            stockActual,
            stockMinimo,
            stockMaximo,
            estado,
            mensaje,
            unidadesSugeridas);
    }
}
