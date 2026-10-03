using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>
/// Catálogo de productos con sus variantes, códigos de barras y recetas.
/// </summary>
[ApiController]
[Route("api/v1/productos")]
[Authorize(Policy = Permisos.CatalogoLeer)]
public class ProductosController : ControllerBase
{
    private readonly IServicioCatalogo _servicio;

    public ProductosController(IServicioCatalogo servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<ProductoDto>>> ObtenerTodos(
        [FromQuery] PaginacionRequest paginacion,
        [FromQuery] Guid? categoriaId,
        [FromQuery] string? busqueda,
        [FromQuery] bool? activo,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerProductosAsync(paginacion, categoriaId, busqueda, activo, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductoDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var producto = await _servicio.ObtenerProductoAsync(id, cancellationToken);
        return producto is null ? NotFound() : Ok(producto);
    }

    [HttpPost]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<ActionResult<ProductoDto>> Crear(
        [FromBody] ProductoCrearDto dto,
        CancellationToken cancellationToken)
    {
        var creado = await _servicio.CrearProductoAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<ActionResult<ProductoDto>> Editar(
        Guid id,
        [FromBody] ProductoEditarDto dto,
        CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { mensaje = "El identificador de la ruta no coincide con el cuerpo." });
        }

        var editado = await _servicio.EditarProductoAsync(dto, cancellationToken);
        return editado is null ? NotFound() : Ok(editado);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
        => await _servicio.EliminarProductoAsync(id, cancellationToken) ? NoContent() : NotFound();

    [HttpGet("{id:guid}/recetas")]
    public async Task<ActionResult<IReadOnlyList<RecetaDto>>> ObtenerRecetas(Guid id, CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerRecetasAsync(id, cancellationToken));

    [HttpPost("{id:guid}/recetas")]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<ActionResult<RecetaDto>> AgregarReceta(
        Guid id,
        [FromBody] RecetaCrearDto dto,
        CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.AgregarRecetaAsync(id, dto, cancellationToken));

    [HttpDelete("{id:guid}/recetas/{recetaId:guid}")]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<IActionResult> EliminarReceta(Guid id, Guid recetaId, CancellationToken cancellationToken)
        => await _servicio.EliminarRecetaAsync(id, recetaId, cancellationToken) ? NoContent() : NotFound();
}
