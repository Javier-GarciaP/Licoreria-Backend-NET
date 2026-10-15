using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Licoreria.IntegrationTests;

/// <summary>El registro de auditoría viaja en la misma transacción que el cambio que describe.</summary>
[Collection("api")]
public class AuditoriaAtomicidadIntegrationTests
{
    private readonly LicoreriaApiFactory _factory;

    public AuditoriaAtomicidadIntegrationTests(LicoreriaApiFactory factory) => _factory = factory;

    private async Task<int> ContarAsync(Guid entidadId, string? accion = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LicoreriaDbContext>();
        return await db.AuditLogs.CountAsync(l => l.EntidadId == entidadId && (accion == null || l.Accion == accion));
    }

    [HechoDeIntegracion]
    public async Task TransaccionRevertida_NoDejaEntradasDeAuditoria()
    {
        using var scope = _factory.Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var db = scope.ServiceProvider.GetRequiredService<LicoreriaDbContext>();
        var categoria = new Categoria { Nombre = $"Cat-{Guid.NewGuid():N}", Descripcion = "prueba" };

        await Assert.ThrowsAsync<InvalidOperationException>(() => uow.EjecutarEnTransaccionAsync(async ct =>
        {
            db.Categorias.Add(categoria);
            await db.SaveChangesAsync(ct);
            throw new InvalidOperationException("falla después de guardar");
        }));

        Assert.Equal(0, await ContarAsync(categoria.Id));
    }

    [HechoDeIntegracion]
    public async Task TransaccionConfirmada_DejaUnaEntradaDeCreacion()
    {
        using var scope = _factory.Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var db = scope.ServiceProvider.GetRequiredService<LicoreriaDbContext>();
        var categoria = new Categoria { Nombre = $"Cat-{Guid.NewGuid():N}", Descripcion = "prueba" };

        await uow.EjecutarEnTransaccionAsync(_ =>
        {
            db.Categorias.Add(categoria);
            return Task.CompletedTask;
        });

        Assert.Equal(1, await ContarAsync(categoria.Id, "crear"));
    }

    [HechoDeIntegracion]
    public async Task ConflictoConReintento_DejaUnaSolaEntradaPorCambioReal()
    {
        Guid id;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LicoreriaDbContext>();
            var categoria = new Categoria { Nombre = $"Cat-{Guid.NewGuid():N}", Descripcion = "inicial" };
            db.Categorias.Add(categoria);
            await db.SaveChangesAsync();
            id = categoria.Id;
        }

        using var scopeOperacion = _factory.Services.CreateScope();
        var uow = scopeOperacion.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var contexto = scopeOperacion.ServiceProvider.GetRequiredService<LicoreriaDbContext>();
        var intentos = 0;

        await uow.EjecutarEnTransaccionAsync(async ct =>
        {
            intentos++;
            var categoria = await contexto.Categorias.SingleAsync(c => c.Id == id, ct);

            if (intentos == 1)
            {
                // Otro proceso modifica la fila entre la lectura y el guardado: provoca el conflicto.
                using var otro = _factory.Services.CreateScope();
                var otroDb = otro.ServiceProvider.GetRequiredService<LicoreriaDbContext>();
                var rival = await otroDb.Categorias.SingleAsync(c => c.Id == id, ct);
                rival.Descripcion = "del rival";
                await otroDb.SaveChangesAsync(ct);
            }

            categoria.Descripcion = "final";
        });

        Assert.True(intentos >= 2, "El conflicto debía forzar un reintento");
        // Un cambio real del rival + un cambio final; el intento fallido no deja entrada.
        Assert.Equal(2, await ContarAsync(id, "editar"));
    }
}
