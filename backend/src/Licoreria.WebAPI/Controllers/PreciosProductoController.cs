using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Licoreria.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Precios de las variantes por lista y moneda.</summary>
[ApiController]
[Route("api/v1/precios-producto")]
[Authorize(Policy = Permisos.CatalogoLeer)]
public class PreciosProductoController : ControllerBase
{
    private readonly IServicioCatalogo _servicio;

    public PreciosProductoController(IServicioCatalogo servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PrecioVarianteDto>>> Obtener(
        [FromQuery] Guid? varianteId,
        [FromQuery] Guid? listaPrecioId,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerPreciosAsync(varianteId, listaPrecioId, cancellationToken));

    [HttpGet("vigente")]
    public async Task<ActionResult<object>> Vigente(
        [FromQuery] Guid varianteId,
        [FromQuery] Guid listaPrecioId,
        [FromQuery] Moneda moneda,
        CancellationToken cancellationToken)
    {
        var precio = await _servicio.ObtenerPrecioAsync(varianteId, listaPrecioId, moneda, cancellationToken);
        return precio is null ? NotFound() : Ok(new { varianteId, listaPrecioId, moneda, precio });
    }

    [HttpPut]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<ActionResult<PrecioVarianteDto>> Establecer([FromBody] EstablecerPrecioDto dto, CancellationToken cancellationToken)
        => Ok(await _servicio.EstablecerPrecioAsync(dto, cancellationToken));

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
        => await _servicio.EliminarPrecioAsync(id, cancellationToken) ? NoContent() : NotFound();
}
