using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

/// <summary>Monedas soportadas.</summary>
[ApiController]
[Route("api/v1/monedas")]
[Authorize(Policy = Permisos.FinanzasLeer)]
public class MonedasController : ControllerBase
{
    private readonly IServicioFinanzas _servicio;

    public MonedasController(IServicioFinanzas servicio) => _servicio = servicio;

    [HttpGet]
    public ActionResult<IReadOnlyList<MonedaDto>> ObtenerTodas() => Ok(_servicio.ObtenerMonedas());
}
