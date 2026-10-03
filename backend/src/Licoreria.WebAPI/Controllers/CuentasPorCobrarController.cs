using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Cuentas por cobrar de clientes (crédito).</summary>
[ApiController]
[Route("api/v1/cuentas-por-cobrar")]
[Authorize(Policy = Permisos.CrmLeer)]
public class CuentasPorCobrarController : ControllerBase
{
    private readonly IServicioCrm _servicio;

    public CuentasPorCobrarController(IServicioCrm servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<CuentaPorCobrarDto>>> Obtener(
        [FromQuery] PaginacionRequest paginacion,
        [FromQuery] Guid? clienteId,
        [FromQuery] bool soloPendientes,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerCuentasPorCobrarAsync(paginacion, clienteId, soloPendientes, cancellationToken));

    [HttpPost]
    [Authorize(Policy = Permisos.CrmEscribir)]
    public async Task<ActionResult<CuentaPorCobrarDto>> Crear(
        [FromBody] CuentaPorCobrarCrearDto dto,
        CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.RegistrarCuentaPorCobrarAsync(dto, cancellationToken));

    [HttpPost("{id:guid}/pagos")]
    [Authorize(Policy = Permisos.CrmEscribir)]
    public async Task<ActionResult<CuentaPorCobrarDto>> RegistrarPago(
        Guid id,
        [FromBody] PagoCuentaPorCobrarDto dto,
        CancellationToken cancellationToken)
    {
        var cuenta = await _servicio.RegistrarPagoCuentaPorCobrarAsync(id, dto, cancellationToken);
        return cuenta is null ? NotFound() : Ok(cuenta);
    }
}
