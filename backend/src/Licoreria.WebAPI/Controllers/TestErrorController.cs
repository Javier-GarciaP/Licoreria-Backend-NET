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
        throw new KeyNotFoundException("El producto con ID 9999 no fue encontrado.");
    }

    // Simula una operación de negocio inválida -> 400 Bad Request
    [HttpGet("bad-request")]
    public IActionResult GetBadRequest()
    {
        throw new InvalidOperationException("La regla de negocio no permite completar la operación.");
    }

    // Simula un argumento inválido -> 400 Bad Request
    [HttpGet("argument")]
    public IActionResult GetArgument()
    {
        throw new ArgumentException("El identificador proporcionado no tiene un formato válido.");
    }

    // Simula un acceso no autorizado -> 401 Unauthorized
    [HttpGet("unauthorized")]
    public IActionResult GetUnauthorized()
    {
        throw new UnauthorizedAccessException("El usuario no tiene permisos para realizar esta acción.");
    }

    // Simula un fallo no controlado -> 500 Internal Server Error
    [HttpGet("server-error")]
    public IActionResult GetServerError()
    {
        throw new Exception("Error interno simulado para demostración del middleware.");
    }
}
