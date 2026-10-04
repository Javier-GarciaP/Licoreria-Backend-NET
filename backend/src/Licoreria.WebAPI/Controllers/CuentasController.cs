using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Licoreria.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Cuentas de mesa, comandas, abonos y cierre.</summary>
[ApiController]
[Route("api/v1/cuentas")]
[Authorize(Policy = Permisos.VentasEscribir)]
public class CuentasController : ControllerBase
{
    private readonly IServicioCuentas _servicio;

    public CuentasController(IServicioCuentas servicio) => _servicio = servicio;

    [HttpGet]
    [Authorize(Policy = Permisos.VentasLeer)]
    public async Task<ActionResult<ResultadoPaginado<CuentaDto>>> Obtener(
        [FromQuery] PaginacionRequest paginacion,
        [FromQuery] EstadoCuenta? estado,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerCuentasAsync(paginacion, estado, cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permisos.VentasLeer)]
    public async Task<ActionResult<CuentaDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        var cuenta = await _servicio.ObtenerCuentaAsync(id, cancellationToken);
        return cuenta is null ? NotFound() : Ok(cuenta);
    }

    /// <summary>Abre una mesa creando su sesión y cuenta.</summary>
    [HttpPost]
    public async Task<ActionResult<CuentaDto>> AbrirMesa([FromBody] AbrirMesaDto dto, CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.AbrirMesaAsync(dto, cancellationToken));

    [HttpPost("{id:guid}/comandas")]
    public async Task<ActionResult<CuentaDto>> AgregarComanda(
        Guid id,
        [FromBody] CrearComandaDto dto,
        CancellationToken cancellationToken)
    {
        var cuenta = await _servicio.AgregarComandaAsync(id, dto, cancellationToken);
        return cuenta is null ? NotFound() : Ok(cuenta);
    }

    [HttpPut("{id:guid}/comandas/{comandaId:guid}/detalles/{detalleId:guid}/estado")]
    public async Task<ActionResult<CuentaDto>> CambiarEstado(
        Guid id,
        Guid comandaId,
        Guid detalleId,
        [FromBody] ActualizarEstadoItemDto dto,
        CancellationToken cancellationToken)
    {
        var cuenta = await _servicio.CambiarEstadoItemAsync(id, comandaId, detalleId, dto, cancellationToken);
        return cuenta is null ? NotFound() : Ok(cuenta);
    }

    [HttpPost("{id:guid}/abonos")]
    [Authorize(Policy = Permisos.CuentasAbonar)]
    public async Task<ActionResult<CuentaDto>> RegistrarAbono(
        Guid id,
        [FromBody] RegistrarAbonoCuentaDto dto,
        CancellationToken cancellationToken)
    {
        var cuenta = await _servicio.RegistrarAbonoAsync(id, dto, cancellationToken);
        return cuenta is null ? NotFound() : Ok(cuenta);
    }

    [HttpPost("{id:guid}/dividir")]
    public async Task<ActionResult<IReadOnlyList<CuentaDivisionDto>>> Dividir(
        Guid id,
        [FromBody] DividirCuentaDto dto,
        CancellationToken cancellationToken)
    {
        var divisiones = await _servicio.DividirCuentaAsync(id, dto, cancellationToken);
        return divisiones is null ? NotFound() : Ok(divisiones);
    }

    [HttpPost("{id:guid}/cerrar")]
    [Authorize(Policy = Permisos.CuentasCerrar)]
    public async Task<ActionResult<VentaDto>> Cerrar(
        Guid id,
        [FromBody] CerrarCuentaDto dto,
        CancellationToken cancellationToken)
    {
        var venta = await _servicio.CerrarCuentaAsync(id, dto, cancellationToken);
        return venta is null ? NotFound() : Ok(venta);
    }
}
