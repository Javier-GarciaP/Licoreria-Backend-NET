namespace Licoreria.Domain.Services;

/// <summary>
/// Motivos por los que un producto deja de ser vendible.
/// </summary>
public enum MotivoMerma
{
    Danado = 1,
    Partido = 2,
    Vencido = 3
}

/// <summary>
/// Movimiento de inventario sugerido a partir de una merma.
/// </summary>
public sealed record MovimientoInventarioSugerido(string Tipo, int Cantidad, string Motivo);

/// <summary>
/// Resultado del análisis de una merma y su eventual reposición.
/// </summary>
public sealed record ResultadoMerma(
    IReadOnlyList<MovimientoInventarioSugerido> Movimientos,
    int TotalUnidadesDescontadas);

/// <summary>
/// Evalúa una merma y determina los movimientos de inventario necesarios.
/// Una reposición sin cobro genera además una cortesía (ambas descuentan inventario).
/// </summary>
public sealed class EvaluadorMerma
{
    public ResultadoMerma Evaluar(MotivoMerma motivo, int cantidad, bool reponerSinCobro)
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
