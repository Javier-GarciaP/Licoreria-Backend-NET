using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Listas de precio (detal, mayorista, happy hour).</summary>
[ApiController]
[Route("api/v1/listas-precio")]
[Authorize(Policy = Permisos.CatalogoLeer)]
public class ListasPrecioController : ControllerBase
{
    private readonly IServicioCatalogo _servicio;

    public ListasPrecioController(IServicioCatalogo servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ListaPrecioDto>>> ObtenerTodas(CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerListasPrecioAsync(cancellationToken));

    [HttpPost]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<ActionResult<ListaPrecioDto>> Crear([FromBody] ListaPrecioCrearDto dto, CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.CrearListaPrecioAsync(dto, cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<ActionResult<ListaPrecioDto>> Editar(Guid id, [FromBody] ListaPrecioEditarDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Solicitud Inválida", detail: "El identificador de la ruta no coincide con el cuerpo.");
        }

        var editada = await _servicio.EditarListaPrecioAsync(dto, cancellationToken);
        return editada is null ? NotFound() : Ok(editada);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.CatalogoEscribir)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
        => await _servicio.EliminarListaPrecioAsync(id, cancellationToken) ? NoContent() : NotFound();
}
