using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Chat del personal (mesoneros, barra y cocina).</summary>
[ApiController]
[Route("api/v1/staff/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IServicioChat _servicio;

    public ChatController(IServicioChat servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ChatMensajeDto>>> Obtener([FromQuery] int limit = 50, CancellationToken cancellationToken = default)
        => Ok(await _servicio.ObtenerAsync(limit, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ChatMensajeDto>> Enviar([FromBody] ChatEnviarDto dto, CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await _servicio.EnviarAsync(dto.Mensaje, cancellationToken));
}
