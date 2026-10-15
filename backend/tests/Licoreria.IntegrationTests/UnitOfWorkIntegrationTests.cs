using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Licoreria.IntegrationTests;

/// <summary>Comportamiento transaccional real del <see cref="IUnitOfWork"/> contra PostgreSQL.</summary>
[Collection("api")]
public class UnitOfWorkIntegrationTests
{
    private readonly LicoreriaApiFactory _factory;

    public UnitOfWorkIntegrationTests(LicoreriaApiFactory factory) => _factory = factory;

    [HechoDeIntegracion]
    public async Task Anidada_ComparteLaTransaccion_YUnFalloExternoRevierteTodo()
    {
        using var scope = _factory.Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var context = scope.ServiceProvider.GetRequiredService<LicoreriaDbContext>();
        var nombre = $"Cat-{Guid.NewGuid():N}";

        await Assert.ThrowsAsync<InvalidOperationException>(() => uow.EjecutarEnTransaccionAsync(async ct =>
        {
            await uow.EjecutarEnTransaccionAsync(_ =>
            {
                context.Categorias.Add(new Categoria { Nombre = nombre, Descripcion = "prueba" });
                return Task.CompletedTask;
            }, ct);

            // La interna no abre ni cierra la transacción: sigue activa y los cambios ya se enviaron.
            Assert.NotNull(context.Database.CurrentTransaction);
            Assert.True(await context.Categorias.AnyAsync(c => c.Nombre == nombre, ct));

            throw new InvalidOperationException("falla externa");
        }));

        Assert.False(await context.Categorias.AnyAsync(c => c.Nombre == nombre));
    }

    [HechoDeIntegracion]
    public async Task Anidada_SiTodoTerminaBien_ConfirmaUnaSolaVez()
    {
        using var scope = _factory.Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var context = scope.ServiceProvider.GetRequiredService<LicoreriaDbContext>();
        var nombre = $"Cat-{Guid.NewGuid():N}";

        await uow.EjecutarEnTransaccionAsync(async ct =>
        {
            await uow.EjecutarEnTransaccionAsync(_ =>
            {
                context.Categorias.Add(new Categoria { Nombre = nombre, Descripcion = "prueba" });
                return Task.CompletedTask;
            }, ct);
        });

        Assert.Equal(1, await context.Categorias.CountAsync(c => c.Nombre == nombre));
        Assert.Null(context.Database.CurrentTransaction);
    }
}
