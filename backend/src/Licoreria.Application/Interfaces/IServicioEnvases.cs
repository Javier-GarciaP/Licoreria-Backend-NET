using Licoreria.Application.Dtos;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

/// <summary>Destapar, listar y descartar envases para servir porciones (vasos, tragos).</summary>
public interface IServicioEnvases
{
    /// <summary>
    /// Destapa una unidad de la presentación elegida para servir la porción: descuenta una unidad
    /// (movimiento de apertura en el kardex) y crea un envase abierto con las porciones del origen.
    /// </summary>
    Task<EnvaseAbiertoDto> DestaparAsync(DestaparEnvaseDto dto, CancellationToken cancellationToken = default);

    /// <summary>Retira un envase abierto registrando como merma las porciones que quedaban. Devuelve null si no existe.</summary>
    Task<EnvaseAbiertoDto?> DescartarAsync(Guid id, DescartarEnvaseDto dto, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EnvaseAbiertoDto>> ListarAsync(
        Guid? porcionVarianteId = null,
        EstadoEnvase? estado = null,
        int limite = 100,
        CancellationToken cancellationToken = default);
}
