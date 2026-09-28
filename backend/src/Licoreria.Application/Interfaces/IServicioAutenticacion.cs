using Licoreria.Application.Dtos;

namespace Licoreria.Application.Interfaces;

public interface IServicioAutenticacion
{
    /// <summary>
    /// Valida las credenciales y devuelve la respuesta con el token,
    /// o null si las credenciales son inválidas.
    /// </summary>
    Task<AuthResponseDto?> IniciarSesionAsync(LoginDto dto, CancellationToken cancellationToken = default);
}
