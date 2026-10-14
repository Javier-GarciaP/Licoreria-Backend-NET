using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Catálogo de impuestos (IVA, IGTF).</summary>
[ApiController]
[Route("api/v1/impuestos")]
[Authorize(Policy = Permisos.CatalogoLeer)]
public class ImpuestosController : ControllerBase
{
    private readonly IServicioCatalogo _servicio;

    public ImpuestosController(IServicioCatalogo servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ImpuestoDto>>> ObtenerTodos(CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerImpuestosAsync(cancellationToken));

    [HttpPost]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<ActionResult<ImpuestoDto>> Crear([FromBody] ImpuestoCrearDto dto, CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.CrearImpuestoAsync(dto, cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<ActionResult<ImpuestoDto>> Editar(Guid id, [FromBody] ImpuestoEditarDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Solicitud Inválida", detail: "El identificador de la ruta no coincide con el cuerpo.");
        }

        var editado = await _servicio.EditarImpuestoAsync(dto, cancellationToken);
        return editado is null ? NotFound() : Ok(editado);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
        => await _servicio.EliminarImpuestoAsync(id, cancellationToken) ? NoContent() : NotFound();
}
