using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Métodos de pago disponibles.</summary>
[ApiController]
[Route("api/v1/metodos-pago")]
[Authorize(Policy = Permisos.VentasLeer)]
public class MetodosPagoController : ControllerBase
{
    private readonly IServicioVentas _servicio;

    public MetodosPagoController(IServicioVentas servicio) => _servicio = servicio;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<MetodoPagoDto>>> ObtenerTodos(CancellationToken cancellationToken)
        => Ok(await _servicio.ObtenerMetodosPagoAsync(cancellationToken));
}
