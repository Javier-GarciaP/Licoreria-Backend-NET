using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Subida y gestión de archivos (imágenes, comprobantes, media).</summary>
[ApiController]
[Route("api/v1/archivos")]
public class ArchivosController : ControllerBase
{
    private readonly IAlmacenamientoArchivos _almacenamiento;
    private readonly IServicioContenido _contenido;

    public ArchivosController(IAlmacenamientoArchivos almacenamiento, IServicioContenido contenido)
    {
        _almacenamiento = almacenamiento;
        _contenido = contenido;
    }

    [HttpGet]
    [Authorize(Policy = Permisos.ContenidoLeer)]
    public async Task<ActionResult<IReadOnlyList<MediaAssetDto>>> Obtener(CancellationToken cancellationToken)
        => Ok(await _contenido.ObtenerMediaAsync(cancellationToken));

    /// <summary>Sube un archivo y devuelve su URL pública.</summary>
    [HttpPost]
    [Authorize(Policy = Permisos.ContenidoPublicar)]
    [RequestSizeLimit(10_485_760)]
    public async Task<ActionResult<MediaAssetDto>> Subir(
        IFormFile archivo,
        [FromQuery] string carpeta = "media",
        CancellationToken cancellationToken = default)
    {
        if (archivo is null || archivo.Length == 0)
        {
            return BadRequest(new { mensaje = "Debe adjuntar un archivo." });
        }

        await using var stream = archivo.OpenReadStream();
        var guardado = await _almacenamiento.GuardarAsync(stream, archivo.FileName, carpeta, cancellationToken);
        var asset = await _contenido.RegistrarMediaAsync(
            archivo.FileName,
            guardado.RutaRelativa,
            guardado.Url,
            archivo.ContentType,
            guardado.Tamano,
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, asset);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.ContenidoPublicar)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
        => await _contenido.EliminarMediaAsync(id, cancellationToken) ? NoContent() : NotFound();
}
