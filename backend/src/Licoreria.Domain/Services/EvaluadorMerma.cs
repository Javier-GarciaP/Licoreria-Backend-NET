using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Services;

/// <summary>
/// Movimiento de inventario sugerido a partir de una merma.
/// </summary>
public sealed record MovimientoInventarioSugerido(string Tipo, decimal Cantidad, string Motivo);

/// <summary>
/// Resultado del análisis de una merma y su eventual reposición.
/// </summary>
public sealed record ResultadoMerma(
    IReadOnlyList<MovimientoInventarioSugerido> Movimientos,
    decimal TotalUnidadesDescontadas);

/// <summary>
/// Evalúa una merma y determina los movimientos de inventario necesarios.
/// Una reposición sin cobro genera además una cortesía (ambas descuentan inventario).
/// </summary>
public sealed class EvaluadorMerma
{
    public ResultadoMerma Evaluar(MotivoMerma motivo, decimal cantidad, bool reponerSinCobro)
    {
        if (cantidad <= 0)
        {
            throw new InvalidOperationException("La cantidad de la merma debe ser mayor que cero.");
        }

        var movimientos = new List<MovimientoInventarioSugerido>
        {
            new("Merma", cantidad, motivo.ToString())
        };

        if (reponerSinCobro)
        {
            movimientos.Add(new MovimientoInventarioSugerido("Cortesia", cantidad, "Reposicion sin cobro"));
        }

        return new ResultadoMerma(movimientos, movimientos.Sum(m => m.Cantidad));
    }
}
