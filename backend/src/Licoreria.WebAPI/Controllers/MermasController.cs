using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Registro y consulta de mermas y cortesías.</summary>
[ApiController]
[Route("api/v1/mermas")]
[Authorize(Policy = Permisos.InventarioLeer)]
public class MermasController : ControllerBase
{
    private readonly IServicioInventario _servicio;

    public MermasController(IServicioInventario servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<MermaDto>>> Obtener(
        [FromQuery] PaginacionRequest paginacion,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerMermasAsync(paginacion, desde, hasta, cancellationToken));

    [HttpPost]
    [Authorize(Policy = Permisos.InventarioMerma)]
    public async Task<ActionResult<MermaDto>> Registrar(
        [FromBody] RegistrarMermaDto dto,
        CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.RegistrarMermaAsync(dto, cancellationToken));
}
