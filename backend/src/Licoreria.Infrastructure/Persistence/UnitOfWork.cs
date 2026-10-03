using Licoreria.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Persistence;

/// <summary>
/// Implementación de la unidad de trabajo sobre <see cref="LicoreriaDbContext"/>.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly LicoreriaDbContext _context;

    public UnitOfWork(LicoreriaDbContext context) => _context = context;

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    public async Task EjecutarEnTransaccionAsync(
        Func<CancellationToken, Task> operacion,
        CancellationToken cancellationToken = default)
    {
        var estrategia = _context.Database.CreateExecutionStrategy();

        await estrategia.ExecuteAsync(async ct =>
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync(ct);
            await operacion(ct);
            await _context.SaveChangesAsync(ct);
            await transaccion.CommitAsync(ct);
        }, cancellationToken);
    }
}
