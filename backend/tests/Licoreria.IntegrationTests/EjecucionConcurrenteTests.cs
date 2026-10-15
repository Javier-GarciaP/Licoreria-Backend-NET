namespace Licoreria.IntegrationTests;

/// <summary>Pruebas del arnés de concurrencia. No usan la base de datos: se ejecutan siempre.</summary>
public class EjecucionConcurrenteTests
{
    [Fact]
    public async Task Ejecutar_LanzaLasOperacionesJuntasYDevuelveUnResultadoPorCada()
    {
        const int cantidad = 10;
        var llegaron = 0;
        var todasLlegaron = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        // Cada operación espera a que lleguen las demás: solo termina si el arnés las ejecuta a la vez.
        var resultados = await EjecucionConcurrente.EjecutarAsync(cantidad, async indice =>
        {
            if (Interlocked.Increment(ref llegaron) == cantidad)
            {
                todasLlegaron.SetResult();
            }

            await todasLlegaron.Task;
            return indice * 2;
        }, TimeSpan.FromSeconds(10));

        Assert.Equal(cantidad, resultados.Count);
        Assert.Equal(Enumerable.Range(0, cantidad), resultados.Select(r => r.Indice).OrderBy(i => i));
        Assert.All(resultados, r => Assert.Equal(r.Indice * 2, r.Valor));
        Assert.All(resultados, r => Assert.False(r.Fallo));
    }

    [Fact]
    public async Task Ejecutar_RecogeLaExcepcionDeCadaOperacionSinPerderLasDemas()
    {
        var resultados = await EjecucionConcurrente.EjecutarAsync<int>(3, indice =>
            indice == 1 ? throw new InvalidOperationException("falla la 1") : Task.FromResult(indice));

        Assert.Equal(3, resultados.Count);
        var fallida = Assert.Single(resultados, r => r.Fallo);
        Assert.Equal(1, fallida.Indice);
        Assert.IsType<InvalidOperationException>(fallida.Error);
        Assert.All(resultados.Where(r => !r.Fallo), r => Assert.Equal(r.Indice, r.Valor));
    }

    [Fact]
    public async Task Ejecutar_RegistraLaDuracionDeCadaOperacion()
    {
        var resultados = await EjecucionConcurrente.EjecutarAsync(2, async _ =>
        {
            await Task.Delay(50);
            return 0;
        });

        Assert.All(resultados, r => Assert.True(r.Duracion >= TimeSpan.FromMilliseconds(40), $"Duración {r.Duracion}"));
    }

    [Fact]
    public async Task Ejecutar_ConUnaOperacionColgada_FallaPorTiempoMaximoIndicandoCualNoTermino()
    {
        var ex = await Assert.ThrowsAsync<TimeoutException>(() =>
            EjecucionConcurrente.EjecutarAsync(4, async indice =>
            {
                if (indice == 2)
                {
                    await Task.Delay(Timeout.Infinite);
                }

                return indice;
            }, TimeSpan.FromMilliseconds(300)));

        Assert.Contains("2", ex.Message);
        Assert.DoesNotContain("0, 1", ex.Message);
    }

    [Fact]
    public async Task Repetir_EjecutaElEscenarioLasVecesIndicadas()
    {
        var corridas = 0;

        await EjecucionConcurrente.RepetirAsync(7, () =>
        {
            corridas++;
            return Task.CompletedTask;
        });

        Assert.Equal(7, corridas);
    }

    [Fact]
    public async Task Repetir_SinCantidad_RepiteDiezVeces()
    {
        var corridas = 0;

        await EjecucionConcurrente.RepetirAsync(() =>
        {
            corridas++;
            return Task.CompletedTask;
        });

        Assert.Equal(EjecucionConcurrente.RepeticionesPorDefecto, corridas);
        Assert.Equal(10, corridas);
    }

    [Fact]
    public async Task Repetir_SiUnaRepeticionFalla_SeDetieneEIndicaCual()
    {
        var corridas = 0;

        var ex = await Assert.ThrowsAsync<RepeticionFallidaException>(() =>
            EjecucionConcurrente.RepetirAsync(10, () =>
            {
                corridas++;
                return corridas == 4 ? throw new InvalidOperationException("invariante rota") : Task.CompletedTask;
            }));

        Assert.Equal(4, ex.Repeticion);
        Assert.Equal(4, corridas);
        Assert.Contains("repetición 4 de 10", ex.Message);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    [Fact]
    public async Task Repetir_ConCantidadInvalida_LanzaArgumentOutOfRange()
        => await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => EjecucionConcurrente.RepetirAsync(0, () => Task.CompletedTask));
}
