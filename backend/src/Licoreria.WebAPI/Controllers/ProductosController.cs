using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>
/// Catálogo de productos. Requiere autenticación; la eliminación es exclusiva del rol Admin.
/// </summary>
[ApiController]
[Route("api/v1/productos")]
[Authorize(Roles = "Admin,Employee")]
public class ProductosController : ControllerBase
{
    private readonly IServicioCatalogo _servicioCatalogo;

    public ProductosController(IServicioCatalogo servicioCatalogo)
    {
        _servicioCatalogo = servicioCatalogo;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductoDto>>> ObtenerTodos(CancellationToken cancellationToken)
        => Ok(await _servicioCatalogo.ObtenerProductosAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductoDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var producto = await _servicioCatalogo.ObtenerProductoAsync(id, cancellationToken);
        return producto is null ? NotFound() : Ok(producto);
    }

    [HttpPost]
    public async Task<ActionResult<ProductoDto>> Crear(
        [FromBody] ProductoCrearDto dto,
        CancellationToken cancellationToken)
    {
        var creado = await _servicioCatalogo.CrearProductoAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductoDto>> Editar(
        Guid id,
        [FromBody] ProductoEditarDto dto,
        CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { mensaje = "El identificador de la ruta no coincide con el cuerpo." });
        }

        var editado = await _servicioCatalogo.EditarProductoAsync(dto, cancellationToken);
        return editado is null ? NotFound() : Ok(editado);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        var eliminado = await _servicioCatalogo.EliminarProductoAsync(id, cancellationToken);
        return eliminado ? NoContent() : NotFound();
    }
}
