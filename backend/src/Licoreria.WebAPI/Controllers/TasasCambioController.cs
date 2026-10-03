using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Licoreria.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Histórico y registro de tasas de cambio (BCV/paralelo).</summary>
[ApiController]
[Route("api/v1/tasas-cambio")]
public class TasasCambioController : ControllerBase
{
    private readonly IServicioFinanzas _servicio;

    public TasasCambioController(IServicioFinanzas servicio) => _servicio = servicio;

    /// <summary>Tasa vigente (pública para la web del cliente).</summary>
    [HttpGet("actual")]
    [AllowAnonymous]
    public async Task<ActionResult<TasaCambioDto>> Actual([FromQuery] TipoTasa tipo, CancellationToken cancellationToken)
    {
        var tasa = await _servicio.ObtenerTasaActualAsync(tipo, cancellationToken);
        return tasa is null ? NotFound() : Ok(tasa);
    }

    [HttpGet]
    [Authorize(Policy = Permisos.FinanzasLeer)]
    public async Task<ActionResult<IReadOnlyList<TasaCambioDto>>> Obtener(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] TipoTasa? tipo,
        CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerHistoricoAsync(desde, hasta, tipo, cancellationToken));

    [HttpPost]
    [Authorize(Policy = Permisos.FinanzasTasa)]
    public async Task<ActionResult<TasaCambioDto>> Registrar(
        [FromBody] RegistrarTasaDto dto,
        CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.RegistrarTasaAsync(dto, cancellationToken));
}
