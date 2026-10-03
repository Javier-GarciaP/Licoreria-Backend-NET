using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Licoreria.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Reservas, señas y su validación.</summary>
[ApiController]
[Route("api/v1/reservas")]
public class ReservasController : ControllerBase
{
    private readonly IServicioClub _servicio;

    public ReservasController(IServicioClub servicio) => _servicio = servicio;

    [HttpGet]
    [Authorize(Policy = Permisos.ReservasGestionar)]
    public async Task<ActionResult<ResultadoPaginado<ReservaDto>>> Obtener(
        [FromQuery] PaginacionRequest paginacion,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] EstadoReserva? estado,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerReservasAsync(paginacion, desde, hasta, estado, cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permisos.ReservasGestionar)]
    public async Task<ActionResult<ReservaDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var reserva = await _servicio.ObtenerReservaAsync(id, cancellationToken);
        return reserva is null ? NotFound() : Ok(reserva);
    }

    /// <summary>Crea una reserva desde el sistema interno o la web pública.</summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ReservaDto>> Crear([FromBody] ReservaCrearDto dto, CancellationToken cancellationToken)
    {
        var creada = await _servicio.CrearReservaAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
    }

    /// <summary>Adjunta una seña con comprobante (queda pendiente de validación).</summary>
    [HttpPost("{id:guid}/pagos")]
    [AllowAnonymous]
    public async Task<ActionResult<ReservaDto>> RegistrarPago(
        Guid id,
        [FromBody] ReservaPagoCrearDto dto,
        CancellationToken cancellationToken)
    {
        var reserva = await _servicio.RegistrarPagoReservaAsync(id, dto, cancellationToken);
        return reserva is null ? NotFound() : Ok(reserva);
    }

    [HttpPost("{id:guid}/pagos/{pagoId:guid}/validar")]
    [Authorize(Policy = Permisos.ReservasGestionar)]
    public async Task<ActionResult<ReservaDto>> ValidarPago(
        Guid id,
        Guid pagoId,
        [FromBody] ValidarReservaPagoDto dto,
        CancellationToken cancellationToken)
    {
        var reserva = await _servicio.ValidarPagoReservaAsync(id, pagoId, dto, cancellationToken);
        return reserva is null ? NotFound() : Ok(reserva);
    }

    [HttpPut("{id:guid}/estado")]
    [Authorize(Policy = Permisos.ReservasGestionar)]
    public async Task<ActionResult<ReservaDto>> CambiarEstado(
        Guid id,
        [FromBody] CambiarEstadoReservaDto dto,
        CancellationToken cancellationToken)
    {
        var reserva = await _servicio.CambiarEstadoReservaAsync(id, dto, cancellationToken);
        return reserva is null ? NotFound() : Ok(reserva);
    }
}
