using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Menú digital, su código QR y su versión PDF (público para la web).</summary>
[ApiController]
[Route("api/v1/menu-digital")]
[AllowAnonymous]
public class MenuDigitalController : ControllerBase
{
    private readonly IServicioContenido _servicio;
    private readonly IGeneradorMenuPdf _generadorPdf;

    public MenuDigitalController(IServicioContenido servicio, IGeneradorMenuPdf generadorPdf)
    {
        _servicio = servicio;
        _generadorPdf = generadorPdf;
    }

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

    /// <summary>Descarga el menú en PDF.</summary>
    [HttpGet("pdf")]
    public async Task<IActionResult> Pdf(CancellationToken cancellationToken)
    {
        var menu = await _servicio.ObtenerMenuDigitalAsync(cancellationToken);
        var bytes = _generadorPdf.Generar(menu);
        return File(bytes, "application/pdf", "menu.pdf");
    }
}
