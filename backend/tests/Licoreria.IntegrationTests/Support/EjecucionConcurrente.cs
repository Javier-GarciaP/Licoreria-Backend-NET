using System.Diagnostics;

namespace Licoreria.IntegrationTests.Support;

/// <summary>Resultado de una operación lanzada por <see cref="EjecucionConcurrente"/>.</summary>
public sealed record ResultadoConcurrente<T>(int Indice, T? Valor, Exception? Error, TimeSpan Duracion)
{
    public bool Fallo => Error is not null;
}

/// <summary>
/// Lanza N operaciones en paralelo detenidas por una señal de salida común, de modo que arranquen
/// juntas y se maximice la superposición, y recoge el resultado o la excepción de cada una.
/// </summary>
public static class EjecucionConcurrente
{
    public static readonly TimeSpan TiempoMaximoPorDefecto = TimeSpan.FromSeconds(60);

    public static async Task<IReadOnlyList<ResultadoConcurrente<T>>> EjecutarAsync<T>(
        int cantidad,
        Func<int, Task<T>> operacion,
        TimeSpan? tiempoMaximo = null)
    {
        var salida = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var tareas = Enumerable.Range(0, cantidad)
            .Select(indice => Task.Run(async () =>
            {
                await salida.Task;
                var reloj = Stopwatch.StartNew();
                try
                {
                    var valor = await operacion(indice);
                    return new ResultadoConcurrente<T>(indice, valor, null, reloj.Elapsed);
                }
                catch (Exception ex)
                {
                    return new ResultadoConcurrente<T>(indice, default, ex, reloj.Elapsed);
                }
            }))
            .ToList();

        salida.SetResult();

        var todas = Task.WhenAll(tareas);
        var limite = tiempoMaximo ?? TiempoMaximoPorDefecto;
        if (await Task.WhenAny(todas, Task.Delay(limite)) != todas)
        {
            var colgadas = tareas.Select((t, i) => (t, i)).Where(x => !x.t.IsCompleted).Select(x => x.i);
            throw new TimeoutException(
                $"Las operaciones {string.Join(", ", colgadas)} no terminaron en {limite.TotalSeconds:0} s.");
        }

        return await todas;
    }

    public const int RepeticionesPorDefecto = 10;

    /// <summary>Repite un escenario de carrera <see cref="RepeticionesPorDefecto"/> veces.</summary>
    public static Task RepetirAsync(Func<Task> escenario) => RepetirAsync(RepeticionesPorDefecto, escenario);

    /// <summary>
    /// Repite un escenario de carrera para reducir los falsos verdes: una sola ejecución puede ganar la carrera por azar.
    /// El escenario debe crear sus propios datos en cada repetición y verificar invariantes, no el orden de llegada.
    /// Si una repetición falla, la excepción indica cuál.
    /// </summary>
    public static async Task RepetirAsync(int veces, Func<Task> escenario)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(veces, 1);

        for (var repeticion = 1; repeticion <= veces; repeticion++)
        {
            try
            {
                await escenario();
            }
            catch (Exception ex)
            {
                throw new RepeticionFallidaException(repeticion, veces, ex);
            }
        }
    }
}

/// <summary>Una repetición de un escenario de carrera falló; <see cref="Exception.InnerException"/> trae la causa.</summary>
public sealed class RepeticionFallidaException(int repeticion, int total, Exception causa)
    : Exception($"El escenario falló en la repetición {repeticion} de {total}: {causa.Message}", causa)
{
    public int Repeticion { get; } = repeticion;
}
