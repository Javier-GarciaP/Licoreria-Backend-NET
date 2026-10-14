using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>
/// Recetas (insumos/conversión) de una variante vendida: presentaciones derivadas
/// y preparados que consumen de la(s) variante(s) base al venderse.
/// </summary>
[ApiController]
[Route("api/v1/variantes")]
public class VariantesController : ControllerBase
{
    private readonly IServicioCatalogo _servicio;

    public VariantesController(IServicioCatalogo servicio) => _servicio = servicio;

    [HttpGet("{id:guid}/recetas")]
    [Authorize(Policy = Permisos.CatalogoLeer)]
    public async Task<ActionResult<IReadOnlyList<RecetaDto>>> ObtenerRecetas(Guid id, CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerRecetasAsync(id, cancellationToken));

    [HttpPost("{id:guid}/recetas")]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<ActionResult<RecetaDto>> AgregarReceta(
        Guid id,
        [FromBody] RecetaCrearDto dto,
        CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.AgregarRecetaAsync(id, dto, cancellationToken));

    [HttpDelete("{id:guid}/recetas/{recetaId:guid}")]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<IActionResult> EliminarReceta(Guid id, Guid recetaId, CancellationToken cancellationToken)
        => await _servicio.EliminarRecetaAsync(id, recetaId, cancellationToken) ? NoContent() : NotFound();

    /// <summary>De qué presentaciones sale una porción (vaso, trago) y cuántas porciones da cada una.</summary>
    [HttpGet("{id:guid}/origenes")]
    [Authorize(Policy = Permisos.CatalogoLeer)]
    public async Task<ActionResult<IReadOnlyList<OrigenPorcionDto>>> ObtenerOrigenes(Guid id, CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerOrigenesAsync(id, cancellationToken));

    /// <summary>Reemplaza la lista de orígenes de la porción.</summary>
    [HttpPut("{id:guid}/origenes")]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<ActionResult<IReadOnlyList<OrigenPorcionDto>>> ReemplazarOrigenes(
        Guid id,
        [FromBody] IReadOnlyList<OrigenPorcionCrearDto> origenes,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ReemplazarOrigenesAsync(id, origenes, cancellationToken));
}
