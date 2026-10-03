using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>
/// Catálogo de categorías jerárquicas.
/// </summary>
[ApiController]
[Route("api/v1/categorias")]
[Authorize(Policy = Permisos.CatalogoLeer)]
public class CategoriasController : ControllerBase
{
    private readonly IServicioCatalogo _servicio;

    public CategoriasController(IServicioCatalogo servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoriaDto>>> ObtenerTodas(CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerCategoriasAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoriaDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var categoria = await _servicio.ObtenerCategoriaAsync(id, cancellationToken);
        return categoria is null ? NotFound() : Ok(categoria);
    }

    [HttpPost]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<ActionResult<CategoriaDto>> Crear(
        [FromBody] CategoriaCrearDto dto,
        CancellationToken cancellationToken)
    {
        var creada = await _servicio.CrearCategoriaAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<ActionResult<CategoriaDto>> Editar(
        Guid id,
        [FromBody] CategoriaEditarDto dto,
        CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { mensaje = "El identificador de la ruta no coincide con el cuerpo." });
        }

        var editada = await _servicio.EditarCategoriaAsync(dto, cancellationToken);
        return editada is null ? NotFound() : Ok(editada);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        var eliminada = await _servicio.EliminarCategoriaAsync(id, cancellationToken);
        return eliminada ? NoContent() : NotFound();
    }
}
