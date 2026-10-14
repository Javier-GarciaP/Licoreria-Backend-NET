using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Ajustes autorizados de inventario (movimiento del kardex).</summary>
[ApiController]
[Route("api/v1/ajustes-inventario")]
[Authorize(Policy = Permisos.InventarioEscribir)]
public class AjustesInventarioController : ControllerBase
{
    private readonly IServicioInventario _servicio;

    public AjustesInventarioController(IServicioInventario servicio) => _servicio = servicio;

    [HttpPost]
    public async Task<ActionResult<MovimientoKardexDto>> Registrar(
        [FromBody] AjusteInventarioDto dto,
        CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.RegistrarAjusteAsync(dto, cancellationToken));
}
