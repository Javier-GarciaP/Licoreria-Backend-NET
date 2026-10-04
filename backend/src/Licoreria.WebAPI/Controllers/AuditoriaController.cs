using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
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
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerAsync(paginacion, entidad, usuarioId, desde, hasta, cancellationToken));
}
