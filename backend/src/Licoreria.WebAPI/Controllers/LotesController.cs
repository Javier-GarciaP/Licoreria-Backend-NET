using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Lotes y fechas de vencimiento por variante.</summary>
[ApiController]
[Route("api/v1/lotes")]
[Authorize(Policy = Permisos.InventarioLeer)]
public class LotesController : ControllerBase
{
    private readonly IServicioInventario _servicio;

    public LotesController(IServicioInventario servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LoteDto>>> Obtener([FromQuery] Guid? varianteId, CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerLotesAsync(varianteId, cancellationToken));

    [HttpPost]
    [Authorize(Policy = Permisos.InventarioEscribir)]
    public async Task<ActionResult<LoteDto>> Crear([FromBody] LoteCrearDto dto, CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.CrearLoteAsync(dto, cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permisos.InventarioEscribir)]
    public async Task<ActionResult<LoteDto>> Editar(Guid id, [FromBody] LoteEditarDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Solicitud Inválida", detail: "El identificador de la ruta no coincide con el cuerpo.");
        }

        var editado = await _servicio.EditarLoteAsync(dto, cancellationToken);
        return editado is null ? NotFound() : Ok(editado);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.InventarioEscribir)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
        => await _servicio.EliminarLoteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
