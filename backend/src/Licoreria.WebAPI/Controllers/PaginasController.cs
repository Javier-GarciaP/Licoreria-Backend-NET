using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Páginas y secciones de la web pública.</summary>
[ApiController]
[Route("api/v1/paginas")]
public class PaginasController : ControllerBase
{
    private readonly IServicioContenido _servicio;

    public PaginasController(IServicioContenido servicio) => _servicio = servicio;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<PaginaDto>>> ObtenerPublicadas(CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerPaginasAsync(soloPublicadas: true, cancellationToken));

    [HttpGet("todos")]
    [Authorize(Policy = Permisos.ContenidoLeer)]
    public async Task<ActionResult<IReadOnlyList<PaginaDto>>> ObtenerTodas(CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerPaginasAsync(soloPublicadas: false, cancellationToken));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<PaginaDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var pagina = await _servicio.ObtenerPaginaAsync(id, cancellationToken);
        return pagina is null ? NotFound() : Ok(pagina);
    }

    [HttpPost]
    [Authorize(Policy = Permisos.ContenidoPublicar)]
    public async Task<ActionResult<PaginaDto>> Crear([FromBody] PaginaCrearDto dto, CancellationToken cancellationToken)
    {
        var creada = await _servicio.CrearPaginaAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permisos.ContenidoPublicar)]
    public async Task<ActionResult<PaginaDto>> Editar(Guid id, [FromBody] PaginaEditarDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Solicitud Inválida", detail: "El identificador de la ruta no coincide con el cuerpo.");
        }

        var editada = await _servicio.EditarPaginaAsync(dto, cancellationToken);
        return editada is null ? NotFound() : Ok(editada);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.ContenidoPublicar)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
        => await _servicio.EliminarPaginaAsync(id, cancellationToken) ? NoContent() : NotFound();
}
