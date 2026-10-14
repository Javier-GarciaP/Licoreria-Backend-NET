using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Licoreria.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Órdenes de compra y su ciclo de aprobación.</summary>
[ApiController]
[Route("api/v1/ordenes-compra")]
[Authorize(Policy = Permisos.ComprasLeer)]
public class OrdenesCompraController : ControllerBase
{
    private readonly IServicioCompras _servicio;

    public OrdenesCompraController(IServicioCompras servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<OrdenCompraDto>>> Obtener(
        [FromQuery] PaginacionRequest paginacion,
        [FromQuery] EstadoOrdenCompra? estado,
        [FromQuery] Guid? proveedorId,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerOrdenesAsync(paginacion, estado, proveedorId, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrdenCompraDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var orden = await _servicio.ObtenerOrdenAsync(id, cancellationToken);
        return orden is null ? NotFound() : Ok(orden);
    }

    [HttpPost]
    [Authorize(Policy = Permisos.ComprasEscribir)]
    public async Task<ActionResult<OrdenCompraDto>> Crear([FromBody] OrdenCompraCrearDto dto, CancellationToken cancellationToken)
    {
        var creada = await _servicio.CrearOrdenAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
    }

    [HttpPost("{id:guid}/aprobar")]
    [Authorize(Policy = Permisos.ComprasEscribir)]
    public async Task<ActionResult<OrdenCompraDto>> Aprobar(Guid id, CancellationToken cancellationToken)
    {
        var orden = await _servicio.AprobarOrdenAsync(id, cancellationToken);
        return orden is null ? NotFound() : Ok(orden);
    }

    [HttpPost("{id:guid}/enviar")]
    [Authorize(Policy = Permisos.ComprasEscribir)]
    public async Task<ActionResult<OrdenCompraDto>> Enviar(Guid id, CancellationToken cancellationToken)
    {
        var orden = await _servicio.EnviarOrdenAsync(id, cancellationToken);
        return orden is null ? NotFound() : Ok(orden);
    }

    [HttpPost("{id:guid}/cancelar")]
    [Authorize(Policy = Permisos.ComprasEscribir)]
    public async Task<ActionResult<OrdenCompraDto>> Cancelar(Guid id, CancellationToken cancellationToken)
    {
        var orden = await _servicio.CancelarOrdenAsync(id, cancellationToken);
        return orden is null ? NotFound() : Ok(orden);
    }
}
