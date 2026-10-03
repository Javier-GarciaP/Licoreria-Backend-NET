using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Licoreria.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Módulo de IA: digitalización de planos, secciones, imágenes y aprobación.</summary>
[ApiController]
[Route("api/v1/ai")]
[Authorize(Policy = Permisos.IaGenerar)]
public class IaController : ControllerBase
{
    private readonly IServicioIa _servicio;

    public IaController(IServicioIa servicio) => _servicio = servicio;

    /// <summary>Digitaliza un plano desde una imagen (resultado simulado).</summary>
    [HttpPost("planos")]
    [RequestSizeLimit(10_485_760)]
    public async Task<ActionResult<AiGeneracionDto>> Plano(IFormFile imagen, CancellationToken cancellationToken)
    {
        if (imagen is null || imagen.Length == 0)
        {
            return BadRequest(new { mensaje = "Debe adjuntar la imagen del croquis." });
        }

        return StatusCode(StatusCodes.Status202Accepted, await _servicio.SolicitarPlanoAsync(imagen.FileName, cancellationToken));
    }

    [HttpPost("secciones")]
    public async Task<ActionResult<AiGeneracionDto>> Seccion([FromBody] SolicitarSeccionIaDto dto, CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status202Accepted, await _servicio.SolicitarSeccionAsync(dto, cancellationToken));

    [HttpPost("imagenes")]
    public async Task<ActionResult<AiGeneracionDto>> Imagen([FromBody] SolicitarImagenIaDto dto, CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status202Accepted, await _servicio.SolicitarImagenAsync(dto, cancellationToken));

    [HttpGet("generaciones")]
    public async Task<ActionResult<ResultadoPaginado<AiGeneracionDto>>> Obtener(
        [FromQuery] PaginacionRequest paginacion,
        [FromQuery] string? tipo,
        [FromQuery] EstadoIa? estado,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerGeneracionesAsync(paginacion, tipo, estado, cancellationToken));

    [HttpGet("generaciones/{id:guid}")]
    public async Task<ActionResult<AiGeneracionDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var generacion = await _servicio.ObtenerGeneracionAsync(id, cancellationToken);
        return generacion is null ? NotFound() : Ok(generacion);
    }

    [HttpPost("generaciones/{id:guid}/aprobar")]
    [Authorize(Policy = Permisos.IaAprobar)]
    public async Task<ActionResult<AiGeneracionDto>> Aprobar(Guid id, CancellationToken cancellationToken)
    {
        var generacion = await _servicio.AprobarAsync(id, cancellationToken);
        return generacion is null ? NotFound() : Ok(generacion);
    }
}
