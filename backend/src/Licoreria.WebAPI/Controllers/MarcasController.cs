using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Catálogo de marcas.</summary>
[ApiController]
[Route("api/v1/marcas")]
[Authorize(Policy = Permisos.CatalogoLeer)]
public class MarcasController : ControllerBase
{
    private readonly IServicioCatalogo _servicio;

    public MarcasController(IServicioCatalogo servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MarcaDto>>> ObtenerTodas(CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerMarcasAsync(cancellationToken));

    [HttpPost]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<ActionResult<MarcaDto>> Crear([FromBody] MarcaCrearDto dto, CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.CrearMarcaAsync(dto, cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<ActionResult<MarcaDto>> Editar(Guid id, [FromBody] MarcaEditarDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { mensaje = "El identificador de la ruta no coincide con el cuerpo." });
        }

        var editada = await _servicio.EditarMarcaAsync(dto, cancellationToken);
        return editada is null ? NotFound() : Ok(editada);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
        => await _servicio.EliminarMarcaAsync(id, cancellationToken) ? NoContent() : NotFound();
}
