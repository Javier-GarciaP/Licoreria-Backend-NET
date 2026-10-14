using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Tomas físicas de inventario y sus ajustes por diferencia.</summary>
[ApiController]
[Route("api/v1/tomas-fisicas")]
[Authorize(Policy = Permisos.InventarioLeer)]
public class TomasFisicasController : ControllerBase
{
    private readonly IServicioInventario _servicio;

    public TomasFisicasController(IServicioInventario servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<TomaFisicaDto>>> Obtener(
        [FromQuery] PaginacionRequest paginacion,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerTomasFisicasAsync(paginacion, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TomaFisicaDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var toma = await _servicio.ObtenerTomaFisicaAsync(id, cancellationToken);
        return toma is null ? NotFound() : Ok(toma);
    }

    [HttpPost]
    [Authorize(Policy = Permisos.InventarioEscribir)]
    public async Task<ActionResult<TomaFisicaDto>> Registrar(
        [FromBody] RegistrarTomaFisicaDto dto,
        CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.RegistrarTomaFisicaAsync(dto, cancellationToken));
}
