using Licoreria.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Licoreria.Infrastructure.Persistence;

/// <summary>
/// Traduce la violación de un índice único de PostgreSQL (SQLSTATE 23505) a un
/// <see cref="ConflictoException"/> con un mensaje de negocio (HTTP 409).
/// </summary>
internal static class ViolacionUnicidad
{
    /// <summary>Guarda los cambios y, si falla por el índice indicado, lanza el conflicto con ese mensaje.</summary>
    public static async Task<int> GuardarAsync(
        DbContext contexto,
        CancellationToken cancellationToken,
        params (string Indice, string Mensaje)[] indices)
    {
        try
        {
            return await contexto.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation
        } pg && Buscar(pg.ConstraintName, indices) is { } mensaje)
        {
            throw new ConflictoException(mensaje);
        }
    }

    private static string? Buscar(string? indice, (string Indice, string Mensaje)[] indices)
        => indices.FirstOrDefault(i => i.Indice == indice).Mensaje;
}
