using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Promociones y descuentos aplicables a las ventas.</summary>
[ApiController]
[Route("api/v1/promociones")]
[Authorize(Policy = Permisos.VentasLeer)]
public class PromocionesController : ControllerBase
{
    private readonly IServicioVentas _servicio;

    public PromocionesController(IServicioVentas servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PromocionDto>>> Obtener([FromQuery] bool soloVigentes, CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerPromocionesAsync(soloVigentes, cancellationToken));

    [HttpPost]
    [Authorize(Policy = Permisos.VentasEscribir)]
    public async Task<ActionResult<PromocionDto>> Crear([FromBody] PromocionCrearDto dto, CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.CrearPromocionAsync(dto, cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permisos.VentasEscribir)]
    public async Task<ActionResult<PromocionDto>> Editar(Guid id, [FromBody] PromocionEditarDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Solicitud Inválida", detail: "El identificador de la ruta no coincide con el cuerpo.");
        }

        var editada = await _servicio.EditarPromocionAsync(dto, cancellationToken);
        return editada is null ? NotFound() : Ok(editada);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.VentasEscribir)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
        => await _servicio.EliminarPromocionAsync(id, cancellationToken) ? NoContent() : NotFound();
}
