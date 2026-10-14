using Licoreria.Domain.Common;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.UnitTests;

public sealed class PoliticaReintentoConcurrenciaTests
{
    [Fact]
    public async Task Ejecutar_SinConflicto_EjecutaUnaVezYNoReintenta()
    {
        var intentos = 0;
        var reintentos = 0;

        await PoliticaReintentoConcurrencia.EjecutarAsync(
            () => { intentos++; return Task.CompletedTask; },
            () => reintentos++);

        Assert.Equal(1, intentos);
        Assert.Equal(0, reintentos);
    }

    [Fact]
    public async Task Ejecutar_ConflictoTransitorio_SeRecuperaAlReintentar()
    {
        var intentos = 0;
        var reintentos = 0;

        await PoliticaReintentoConcurrencia.EjecutarAsync(
            () =>
            {
                intentos++;
                if (intentos == 1)
                {
                    throw new DbUpdateConcurrencyException("conflicto de versión");
                }

                return Task.CompletedTask;
            },
            () => reintentos++);

        Assert.Equal(2, intentos);
        Assert.Equal(1, reintentos);
    }

    [Fact]
    public async Task Ejecutar_ConflictoPersistente_LanzaConflictoTrasLosReintentos()
    {
        var intentos = 0;

        var ex = await Assert.ThrowsAsync<ConflictoException>(() =>
            PoliticaReintentoConcurrencia.EjecutarAsync(
                () =>
                {
                    intentos++;
                    throw new DbUpdateConcurrencyException("conflicto de versión");
                },
                () => { }));

        // Primer intento más los reintentos permitidos.
        Assert.Equal(1 + PoliticaReintentoConcurrencia.MaxReintentos, intentos);
        Assert.Contains("al mismo tiempo", ex.Message);
    }

    [Fact]
    public async Task Ejecutar_OtraExcepcion_NoSeReintentaYSePropaga()
    {
        var intentos = 0;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            PoliticaReintentoConcurrencia.EjecutarAsync(
                () => { intentos++; throw new InvalidOperationException("otra"); },
                () => { }));

        Assert.Equal(1, intentos);
    }
}
