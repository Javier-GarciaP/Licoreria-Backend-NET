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
    public async Task<ActionResult<IReadOnlyList<PlanoDto>>> ObtenerTodos(
        [FromQuery] bool plantillas,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerPlanosAsync(plantillas && PuedeEditarDistribucion(), cancellationToken));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<PlanoDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var plano = await _servicio.ObtenerPlanoAsync(id, PuedeEditarDistribucion(), cancellationToken);
        return plano is null ? NotFound() : Ok(plano);
    }

    [HttpPost]
    [Authorize(Policy = Permisos.ClubDistribucion)]
    public async Task<ActionResult<PlanoDto>> Crear([FromBody] PlanoCrearDto dto, CancellationToken cancellationToken)
    {
        var creado = await _servicio.CrearPlanoAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permisos.ClubDistribucion)]
    public async Task<ActionResult<PlanoDto>> Editar(Guid id, [FromBody] PlanoEditarDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Solicitud Inválida", detail: "El identificador de la ruta no coincide con el cuerpo.");
        }

        var editado = await _servicio.EditarPlanoAsync(dto, cancellationToken);
        return editado is null ? NotFound() : Ok(editado);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.ClubDistribucion)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
        => await _servicio.EliminarPlanoAsync(id, cancellationToken) ? NoContent() : NotFound();

    /// <summary>Sube o reemplaza el croquis del plano (PNG, JPG o WebP de hasta 5 MB).</summary>
    [HttpPost("{id:guid}/fondo")]
    [Authorize(Policy = Permisos.ClubDistribucion)]
    [RequestSizeLimit(5_242_880 + 65_536)]
    public async Task<ActionResult<PlanoDto>> SubirFondo(Guid id, IFormFile archivo, CancellationToken cancellationToken)
    {
        if (archivo is null || archivo.Length == 0)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Solicitud Inválida", detail: "Debe adjuntar un archivo con contenido.");
        }

        await using var stream = archivo.OpenReadStream();
        var plano = await _servicio.AsignarFondoPlanoAsync(id, stream, archivo.FileName, archivo.ContentType, archivo.Length, cancellationToken);
        return plano is null ? NotFound() : Ok(plano);
    }

    [HttpDelete("{id:guid}/fondo")]
    [Authorize(Policy = Permisos.ClubDistribucion)]
    public async Task<ActionResult<PlanoDto>> QuitarFondo(Guid id, CancellationToken cancellationToken)
    {
        var plano = await _servicio.QuitarFondoPlanoAsync(id, cancellationToken);
        return plano is null ? NotFound() : Ok(plano);
    }

    /// <summary>Duplica un plano (queda inactivo) o lo guarda como plantilla; desde una plantilla crea un plano normal.</summary>
    [HttpPost("{id:guid}/duplicar")]
    [Authorize(Policy = Permisos.ClubDistribucion)]
    public async Task<ActionResult<PlanoDto>> Duplicar(Guid id, [FromBody] PlanoDuplicarDto? dto, CancellationToken cancellationToken)
    {
        var copia = await _servicio.DuplicarPlanoAsync(id, dto ?? new PlanoDuplicarDto(), cancellationToken);
        return copia is null ? NotFound() : CreatedAtAction(nameof(ObtenerPorId), new { id = copia.Id }, copia);
    }

    private bool PuedeEditarDistribucion()
        => User.HasClaim("permissions", Permisos.ClubDistribucion);
}
