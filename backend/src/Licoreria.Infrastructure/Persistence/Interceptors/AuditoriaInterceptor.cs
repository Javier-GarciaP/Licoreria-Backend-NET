using Licoreria.Application.Interfaces;
using Licoreria.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Licoreria.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Completa los campos de auditoría (<c>CreatedBy</c>/<c>UpdatedBy</c> y fechas)
/// de forma automática antes de persistir, usando el usuario autenticado y el reloj del sistema.
/// </summary>
public sealed class AuditoriaInterceptor : SaveChangesInterceptor
{
    private readonly IContextoUsuario _contextoUsuario;
    private readonly IRelojSistema _reloj;

    public AuditoriaInterceptor(IContextoUsuario contextoUsuario, IRelojSistema reloj)
    {
        _contextoUsuario = contextoUsuario;
        _reloj = reloj;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Aplicar(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Aplicar(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Aplicar(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var usuarioId = _contextoUsuario.UsuarioId;
        var ahora = _reloj.UtcNow;

        foreach (var entrada in context.ChangeTracker.Entries<BaseEntity>())
        {
            switch (entrada.State)
            {
                case EntityState.Added:
                    entrada.Entity.EstablecerCreador(usuarioId, ahora);
                    break;
                case EntityState.Modified:
                    entrada.Entity.EstablecerModificador(usuarioId, ahora);
                    break;
            }
        }
    }
}
