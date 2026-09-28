using Microsoft.AspNetCore.Mvc;

namespace Licoreria.WebAPI.Controllers;

[ApiController]
[Route("api/test-error")]
public class TestErrorController : ControllerBase
{
    // Simula la búsqueda de un recurso inexistente -> 404 Not Found
    [HttpGet("not-found")]
    public IActionResult GetNotFound()
    {
        throw new KeyNotFoundException("El recurso solicitado no fue encontrado.");
    }

    // Simula una operación de negocio inválida -> 400 Bad Request
    [HttpGet("bad-request")]
    public IActionResult GetBadRequest()
    {
        throw new InvalidOperationException("La operación solicitada no es válida.");
    }

    // Simula un fallo no controlado -> 500 Internal Server Error
    [HttpGet("server-error")]
    public IActionResult GetServerError()
    {
        throw new Exception("Error interno simulado para demostración del middleware.");
    }
}
