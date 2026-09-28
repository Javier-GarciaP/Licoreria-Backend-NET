using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>
/// Catálogo de categorías. Requiere autenticación; la eliminación es exclusiva del rol Admin.
/// </summary>
[ApiController]
[Route("api/v1/categorias")]
[Authorize(Roles = "Admin,Employee")]
public class CategoriasController : ControllerBase
{
    private readonly IServicioCatalogo _servicioCatalogo;

    public CategoriasController(IServicioCatalogo servicioCatalogo)
    {
        _servicioCatalogo = servicioCatalogo;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoriaDto>>> ObtenerTodas(CancellationToken cancellationToken)
        => Ok(await _servicioCatalogo.ObtenerCategoriasAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<CategoriaDto>> Crear(
        [FromBody] CategoriaCrearDto dto,
        CancellationToken cancellationToken)
    {
        var creada = await _servicioCatalogo.CrearCategoriaAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, creada);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        var eliminado = await _servicioCatalogo.EliminarCategoriaAsync(id, cancellationToken);
        return eliminado ? NoContent() : NotFound();
    }
}
