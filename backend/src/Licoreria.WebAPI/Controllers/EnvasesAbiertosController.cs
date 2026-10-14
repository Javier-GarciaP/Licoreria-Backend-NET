using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Licoreria.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>
/// Envases (botellas) que la barra destapó para servir porciones: destapar, listar y descartar.
/// </summary>
[ApiController]
[Route("api/v1/envases-abiertos")]
public class EnvasesAbiertosController : ControllerBase
{
    private readonly IServicioEnvases _servicio;

    public EnvasesAbiertosController(IServicioEnvases servicio) => _servicio = servicio;

    /// <summary>
    /// Lista los envases. Sin filtros devuelve los abiertos (el más antiguo primero); con <c>estado</c> filtra por
    /// ese estado y con <c>todos=true</c> devuelve el historial completo, el más reciente primero.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = Permisos.InventarioLeer)]
    public async Task<ActionResult<IReadOnlyList<EnvaseAbiertoDto>>> Listar(
        [FromQuery] Guid? porcionVarianteId,
        [FromQuery] EstadoEnvase? estado,
        [FromQuery] bool todos = false,
        [FromQuery] int limite = 100,
        CancellationToken cancellationToken = default)
        => Ok(await _servicio.ListarAsync(
            porcionVarianteId,
            todos ? null : estado ?? EstadoEnvase.Abierto,
            limite,
            cancellationToken));

    /// <summary>Destapa una unidad de la presentación elegida para servir la porción indicada.</summary>
    [HttpPost]
    [Authorize(Policy = Permisos.InventarioAbrir)]
    public async Task<ActionResult<EnvaseAbiertoDto>> Destapar([FromBody] DestaparEnvaseDto dto, CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.DestaparAsync(dto, cancellationToken));

    /// <summary>Descarta un envase abierto con un motivo; registra como merma las porciones que quedaban.</summary>
    [HttpPost("{id:guid}/descartar")]
    [Authorize(Policy = Permisos.InventarioAbrir)]
    public async Task<ActionResult<EnvaseAbiertoDto>> Descartar(Guid id, [FromBody] DescartarEnvaseDto dto, CancellationToken cancellationToken)
    {
        var envase = await _servicio.DescartarAsync(id, dto, cancellationToken);
        return envase is null ? NotFound() : Ok(envase);
    }
}
