using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Sesiones de caja, movimientos, arqueo y cierre Z.</summary>
[ApiController]
[Route("api/v1/sesiones-caja")]
public class SesionesCajaController : ControllerBase
{
    private readonly IServicioCaja _servicio;

    public SesionesCajaController(IServicioCaja servicio) => _servicio = servicio;

    [HttpPost]
    [Authorize(Policy = Permisos.CajaAbrir)]
    public async Task<ActionResult<SesionCajaDto>> Abrir([FromBody] AbrirCajaDto dto, CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.AbrirSesionAsync(dto, cancellationToken));

    [HttpGet("activa")]
    [Authorize(Policy = Permisos.CajaMovimiento)]
    public async Task<ActionResult<SesionCajaDto>> Activa(CancellationToken cancellationToken)
    {
        var sesion = await _servicio.ObtenerSesionActivaAsync(cancellationToken);
        return sesion is null ? NotFound() : Ok(sesion);
    }

    [HttpGet]
    [Authorize(Policy = Permisos.CajaMovimiento)]
    public async Task<ActionResult<ResultadoPaginado<SesionCajaDto>>> Obtener(
        [FromQuery] PaginacionRequest paginacion,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerSesionesAsync(paginacion, cancellationToken));

    [HttpPost("{id:guid}/movimientos")]
    [Authorize(Policy = Permisos.CajaMovimiento)]
    public async Task<ActionResult<SesionCajaDto>> RegistrarMovimiento(
        Guid id,
        [FromBody] MovimientoCajaCrearDto dto,
        CancellationToken cancellationToken)
    {
        var sesion = await _servicio.RegistrarMovimientoAsync(id, dto, cancellationToken);
        return sesion is null ? NotFound() : Ok(sesion);
    }

    [HttpPost("{id:guid}/cerrar")]
    [Authorize(Policy = Permisos.CajaCerrar)]
    public async Task<ActionResult<SesionCajaDto>> Cerrar(
        Guid id,
        [FromBody] CerrarCajaDto dto,
        CancellationToken cancellationToken)
    {
        var sesion = await _servicio.CerrarSesionAsync(id, dto, cancellationToken);
        return sesion is null ? NotFound() : Ok(sesion);
    }
}
