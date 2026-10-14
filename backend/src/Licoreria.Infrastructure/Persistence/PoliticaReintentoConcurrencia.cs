using Licoreria.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Persistence;

/// <summary>
/// Reintento acotado ante conflictos de versión (<see cref="DbUpdateConcurrencyException"/>).
/// Cada intento vuelve a ejecutar la operación completa, que debe releer sus datos.
/// Si los reintentos se agotan, el conflicto se informa como <see cref="ConflictoException"/> (HTTP 409).
/// </summary>
public static class PoliticaReintentoConcurrencia
{
    /// <summary>Reintentos permitidos después del primer intento.</summary>
    public const int MaxReintentos = 3;

    public static async Task EjecutarAsync(
        Func<Task> intento,
        Action alReintentar,
        int maxReintentos = MaxReintentos)
    {
        for (var reintentos = 0; ; reintentos++)
        {
            try
            {
                await intento();
                return;
            }
            catch (DbUpdateConcurrencyException) when (reintentos < maxReintentos)
            {
                alReintentar();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictoException(
                    "Otra operación modificó los mismos datos al mismo tiempo. Actualice la información e intente de nuevo.");
            }
        }
    }
}
