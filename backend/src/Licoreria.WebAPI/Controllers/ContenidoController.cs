using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Horarios de atención e información del local.</summary>
[ApiController]
[Route("api/v1")]
public class ContenidoController : ControllerBase
{
    private readonly IServicioContenido _servicio;

    public ContenidoController(IServicioContenido servicio) => _servicio = servicio;

    [HttpGet("horarios")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<HorarioDto>>> ObtenerHorarios(CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerHorariosAsync(cancellationToken));

    [HttpPut("horarios")]
    [Authorize(Policy = Permisos.ContenidoPublicar)]
    public async Task<ActionResult<HorarioDto>> GuardarHorario([FromBody] HorarioGuardarDto dto, CancellationToken cancellationToken)
        => Ok(await _servicio.GuardarHorarioAsync(dto, cancellationToken));

    [HttpGet("local-info")]
    [AllowAnonymous]
    public async Task<ActionResult<LocalInfoDto>> ObtenerLocalInfo(CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerLocalInfoAsync(cancellationToken));

    [HttpPut("local-info")]
    [Authorize(Policy = Permisos.ContenidoPublicar)]
    public async Task<ActionResult<LocalInfoDto>> ActualizarLocalInfo([FromBody] LocalInfoEditarDto dto, CancellationToken cancellationToken)
        => Ok(await _servicio.ActualizarLocalInfoAsync(dto, cancellationToken));

    /// <summary>Enlace de WhatsApp con mensaje precargado.</summary>
    [HttpGet("local-info/whatsapp")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> Whatsapp([FromQuery] string? mensaje, CancellationToken cancellationToken)
    {
        var info = await _servicio.ObtenerLocalInfoAsync(cancellationToken);
        var numero = new string(info.Whatsapp.Where(char.IsDigit).ToArray());
        var texto = Uri.EscapeDataString(string.IsNullOrWhiteSpace(mensaje) ? "Hola, quisiera más información" : mensaje);
        return Ok(new { numero, url = $"https://wa.me/{numero}?text={texto}" });
    }
}
