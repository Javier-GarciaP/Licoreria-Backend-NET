using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

/// <summary>Porciones abiertas y reservadas de una variante-porción.</summary>
public sealed record ResumenPorcion(int PorcionesAbiertas, int PorcionesReservadas);

/// <summary>Acceso a datos de orígenes de porción, envases abiertos y reservas de porciones.</summary>
public interface IPorcionRepository
{
    // ----- Orígenes -----
    Task<IReadOnlyList<PorcionOrigen>> ObtenerOrigenesAsync(Guid porcionVarianteId, CancellationToken cancellationToken = default);

    /// <summary>Orígenes de varias porciones (sin rastreo) con su presentación, para calcular la disponibilidad por lotes.</summary>
    Task<IReadOnlyList<PorcionOrigen>> ObtenerOrigenesDeVariantesAsync(IReadOnlyList<Guid> porcionIds, CancellationToken cancellationToken = default);

    Task ReemplazarOrigenesAsync(Guid porcionVarianteId, IReadOnlyList<PorcionOrigen> nuevos, CancellationToken cancellationToken = default);

    Task<bool> TieneOrigenesAsync(Guid varianteId, CancellationToken cancellationToken = default);

    /// <summary>¿La presentación es origen de alguna porción?</summary>
    Task<bool> EsOrigenAsync(Guid presentacionId, CancellationToken cancellationToken = default);

    /// <summary>¿Hay envases abiertos de esta variante, ya sea como porción o como presentación de origen?</summary>
    Task<bool> HayEnvasesAbiertosAsync(Guid varianteId, CancellationToken cancellationToken = default);

    Task<IReadOnlySet<Guid>> ObtenerIdsConRecetaAsync(IReadOnlyList<Guid> varianteIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, ResumenPorcion>> ObtenerResumenAsync(IReadOnlyList<Guid> porcionIds, CancellationToken cancellationToken = default);

    // ----- Envases y reservas -----

    /// <summary>Envases abiertos de la porción con rastreo, el más antiguo primero.</summary>
    Task<IReadOnlyList<EnvaseAbierto>> ObtenerAbiertosAsync(Guid porcionVarianteId, CancellationToken cancellationToken = default);

    Task<EnvaseAbierto?> ObtenerEnvaseAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EnvaseAbierto>> ListarAsync(
        Guid? porcionVarianteId,
        EstadoEnvase? estado,
        int limite,
        CancellationToken cancellationToken = default);

    Task AgregarEnvaseAsync(EnvaseAbierto envase, CancellationToken cancellationToken = default);

    Task<StockPorcion?> ObtenerStockPorcionAsync(Guid varianteId, CancellationToken cancellationToken = default);

    Task AgregarStockPorcionAsync(StockPorcion stock, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
