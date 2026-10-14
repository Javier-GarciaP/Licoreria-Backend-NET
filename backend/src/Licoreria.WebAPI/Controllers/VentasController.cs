using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Ventas, pagos mixtos, comprobantes y devoluciones.</summary>
[ApiController]
[Route("api/v1/ventas")]
public class VentasController : ControllerBase
{
    private readonly IServicioVentas _servicio;

    public VentasController(IServicioVentas servicio) => _servicio = servicio;

    [HttpGet]
    [Authorize(Policy = Permisos.VentasLeer)]
    public async Task<ActionResult<ResultadoPaginado<VentaDto>>> Obtener(
        [FromQuery] PaginacionRequest paginacion,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerVentasAsync(paginacion, desde, hasta, cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permisos.VentasLeer)]
    public async Task<ActionResult<VentaDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var venta = await _servicio.ObtenerVentaAsync(id, cancellationToken);
        return venta is null ? NotFound() : Ok(venta);
    }

    [HttpPost]
    [Authorize(Policy = Permisos.VentasEscribir)]
    public async Task<ActionResult<VentaDto>> Registrar(
        [FromBody] RegistrarVentaDto dto,
        CancellationToken cancellationToken)
    {
        var venta = await _servicio.RegistrarVentaAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = venta.Id }, venta);
    }

    [HttpPost("{id:guid}/pagos")]
    [Authorize(Policy = Permisos.VentasEscribir)]
    public async Task<ActionResult<VentaDto>> RegistrarPago(
        Guid id,
        [FromBody] RegistrarPagoVentaDto dto,
        CancellationToken cancellationToken)
    {
        var venta = await _servicio.RegistrarPagoAsync(id, dto, cancellationToken);
        return venta is null ? NotFound() : Ok(venta);
    }

    [HttpPost("{id:guid}/devoluciones")]
    [Authorize(Policy = Permisos.VentasAnular)]
    public async Task<ActionResult<DevolucionDto>> RegistrarDevolucion(
        Guid id,
        [FromBody] RegistrarDevolucionDto dto,
        CancellationToken cancellationToken)
    {
        var devolucion = await _servicio.RegistrarDevolucionAsync(id, dto, cancellationToken);
        return devolucion is null ? NotFound() : StatusCode(StatusCodes.Status201Created, devolucion);
    }
}
