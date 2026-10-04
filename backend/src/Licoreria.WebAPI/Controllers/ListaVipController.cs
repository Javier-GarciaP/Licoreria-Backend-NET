using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Lista VIP del local.</summary>
[ApiController]
[Route("api/v1/lista-vip")]
[Authorize(Policy = Permisos.ClubLeer)]
public class ListaVipController : ControllerBase
{
    private readonly IServicioClub _servicio;

    public ListaVipController(IServicioClub servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ListaVipDto>>> Obtener(CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerListaVipAsync(cancellationToken));

    [HttpPost]
    [Authorize(Policy = Permisos.ClubGestionar)]
    public async Task<ActionResult<ListaVipDto>> Crear([FromBody] ListaVipCrearDto dto, CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.CrearVipAsync(dto, cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permisos.ClubGestionar)]
    public async Task<ActionResult<ListaVipDto>> Editar(Guid id, [FromBody] ListaVipEditarDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Solicitud Inválida", detail: "El identificador de la ruta no coincide con el cuerpo.");
        }

        var editado = await _servicio.EditarVipAsync(dto, cancellationToken);
        return editado is null ? NotFound() : Ok(editado);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.ClubGestionar)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
        => await _servicio.EliminarVipAsync(id, cancellationToken) ? NoContent() : NotFound();
}
