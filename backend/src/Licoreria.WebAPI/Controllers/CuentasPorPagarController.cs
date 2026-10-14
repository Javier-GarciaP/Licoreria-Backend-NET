using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Cuentas por pagar a proveedores y sus pagos.</summary>
[ApiController]
[Route("api/v1/cuentas-por-pagar")]
[Authorize(Policy = Permisos.ComprasLeer)]
public class CuentasPorPagarController : ControllerBase
{
    private readonly IServicioCompras _servicio;

    public CuentasPorPagarController(IServicioCompras servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<CuentaPorPagarDto>>> Obtener(
        [FromQuery] PaginacionRequest paginacion,
        [FromQuery] Guid? proveedorId,
        [FromQuery] bool soloPendientes,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerCuentasPorPagarAsync(paginacion, proveedorId, soloPendientes, cancellationToken));

    [HttpPost("{id:guid}/pagos")]
    [Authorize(Policy = Permisos.ComprasEscribir)]
    public async Task<ActionResult<CuentaPorPagarDto>> RegistrarPago(
        Guid id,
        [FromBody] RegistrarPagoProveedorDto dto,
        CancellationToken cancellationToken)
    {
        var cuenta = await _servicio.RegistrarPagoCuentaAsync(id, dto, cancellationToken);
        return cuenta is null ? NotFound() : Ok(cuenta);
    }
}
