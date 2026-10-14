using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Consulta de la auditoría de acciones sensibles (solo Admin).</summary>
[ApiController]
[Route("api/v1/auditoria")]
[Authorize(Roles = "Admin")]
public class AuditoriaController : ControllerBase
{
    private readonly IServicioAuditoria _servicio;

    public AuditoriaController(IServicioAuditoria servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<AuditLogDto>>> Obtener(
        [FromQuery] PaginacionRequest paginacion,
        [FromQuery] string? entidad,
        [FromQuery] Guid? usuarioId,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] string? accion,
        [FromQuery] TipoAuditoria? tipo,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerAsync(paginacion, entidad, usuarioId, desde, hasta, accion, tipo, cancellationToken));

    /// <summary>Entidades con registros, para poblar el filtro de la vista.</summary>
    [HttpGet("entidades")]
    public async Task<ActionResult<IReadOnlyList<string>>> ObtenerEntidades(CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerEntidadesAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AuditLogDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var entrada = await _servicio.ObtenerPorIdAsync(id, cancellationToken);
        return entrada is null ? NotFound() : Ok(entrada);
    }
}
