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

    /// <summary>
    /// Ejecuta la operación en una transacción. Es reentrante: si ya hay una transacción
    /// activa, la operación participa en ella (guarda sus cambios dentro de la transacción
    /// exterior, que es quien confirma y reintenta). La transacción exterior reintenta hasta
    /// <see cref="PoliticaReintentoConcurrencia.MaxReintentos"/> veces ante un conflicto de versión.
    /// </summary>
    public async Task EjecutarEnTransaccionAsync(
        Func<CancellationToken, Task> operacion,
        CancellationToken cancellationToken = default)
    {
        if (_context.Database.CurrentTransaction is not null)
        {
            await operacion(cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        var estrategia = _context.Database.CreateExecutionStrategy();

        await PoliticaReintentoConcurrencia.EjecutarAsync(
            () => estrategia.ExecuteAsync(async ct =>
            {
                await using var transaccion = await _context.Database.BeginTransactionAsync(ct);
                await operacion(ct);
                await _context.SaveChangesAsync(ct);
                await transaccion.CommitAsync(ct);
            }, cancellationToken),
            // El intento fallido se revierte al descartar la transacción; el rastreo se limpia
            // para que el siguiente intento relea los datos actuales.
            () => _context.ChangeTracker.Clear());
    }
}
