using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>
/// Catálogo de modificadores/extras aplicables a los productos.
/// </summary>
[ApiController]
[Route("api/v1/modificadores")]
[Authorize(Policy = Permisos.CatalogoLeer)]
public class ModificadoresController : ControllerBase
{
    private readonly IServicioCatalogo _servicio;

    public ModificadoresController(IServicioCatalogo servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ModificadorDto>>> ObtenerTodos(CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerModificadoresAsync(cancellationToken));

    [HttpPost]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<ActionResult<ModificadorDto>> Crear(
        [FromBody] ModificadorCrearDto dto,
        CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.CrearModificadorAsync(dto, cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<ActionResult<ModificadorDto>> Editar(
        Guid id,
        [FromBody] ModificadorEditarDto dto,
        CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Solicitud Inválida", detail: "El identificador de la ruta no coincide con el cuerpo.");
        }

        var editado = await _servicio.EditarModificadorAsync(dto, cancellationToken);
        return editado is null ? NotFound() : Ok(editado);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
        => await _servicio.EliminarModificadorAsync(id, cancellationToken) ? NoContent() : NotFound();
}
