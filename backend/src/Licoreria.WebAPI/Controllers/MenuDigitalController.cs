using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Menú digital y su código QR (público para la web).</summary>
[ApiController]
[Route("api/v1/menu-digital")]
[AllowAnonymous]
public class MenuDigitalController : ControllerBase
{
    private readonly IServicioContenido _servicio;

    public MenuDigitalController(IServicioContenido servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<MenuDigitalDto>> Obtener(CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerMenuDigitalAsync(cancellationToken));

    /// <summary>Devuelve la URL a codificar en el QR del menú.</summary>
    [HttpGet("qr")]
    public async Task<ActionResult<QrMenuDto>> Qr([FromQuery] string? baseUrl, CancellationToken cancellationToken)
    {
        var url = string.IsNullOrWhiteSpace(baseUrl) ? $"{Request.Scheme}://{Request.Host}" : baseUrl;
        return Ok(await _servicio.ObtenerQrMenuAsync(url, cancellationToken));
    }
}
