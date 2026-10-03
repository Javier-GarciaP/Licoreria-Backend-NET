using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Catálogo de unidades de medida.</summary>
[ApiController]
[Route("api/v1/unidades-medida")]
[Authorize(Policy = Permisos.CatalogoLeer)]
public class UnidadesMedidaController : ControllerBase
{
    private readonly IServicioCatalogo _servicio;

    public UnidadesMedidaController(IServicioCatalogo servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UnidadMedidaDto>>> ObtenerTodas(CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerUnidadesAsync(cancellationToken));

    [HttpPost]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<ActionResult<UnidadMedidaDto>> Crear([FromBody] UnidadMedidaCrearDto dto, CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.CrearUnidadAsync(dto, cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<ActionResult<UnidadMedidaDto>> Editar(Guid id, [FromBody] UnidadMedidaEditarDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { mensaje = "El identificador de la ruta no coincide con el cuerpo." });
        }

        var editada = await _servicio.EditarUnidadAsync(dto, cancellationToken);
        return editada is null ? NotFound() : Ok(editada);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
        => await _servicio.EliminarUnidadAsync(id, cancellationToken) ? NoContent() : NotFound();
}
