using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Planos del local con sus elementos.</summary>
[ApiController]
[Route("api/v1/planos")]
public class PlanosController : ControllerBase
{
    private readonly IServicioClub _servicio;

    public PlanosController(IServicioClub servicio) => _servicio = servicio;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<PlanoDto>>> ObtenerTodos(CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerPlanosAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<PlanoDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var plano = await _servicio.ObtenerPlanoAsync(id, cancellationToken);
        return plano is null ? NotFound() : Ok(plano);
    }

    [HttpPost]
    [Authorize(Policy = Permisos.ClubGestionar)]
    public async Task<ActionResult<PlanoDto>> Crear([FromBody] PlanoCrearDto dto, CancellationToken cancellationToken)
    {
        var creado = await _servicio.CrearPlanoAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permisos.ClubGestionar)]
    public async Task<ActionResult<PlanoDto>> Editar(Guid id, [FromBody] PlanoEditarDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { mensaje = "El identificador de la ruta no coincide con el cuerpo." });
        }

        var editado = await _servicio.EditarPlanoAsync(dto, cancellationToken);
        return editado is null ? NotFound() : Ok(editado);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.ClubGestionar)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
        => await _servicio.EliminarPlanoAsync(id, cancellationToken) ? NoContent() : NotFound();
}
