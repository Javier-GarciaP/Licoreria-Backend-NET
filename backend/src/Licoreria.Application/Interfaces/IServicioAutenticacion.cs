using Licoreria.Application.Dtos;

namespace Licoreria.Application.Interfaces;

public interface IServicioAutenticacion
{
    /// <summary>
    /// Valida las credenciales y devuelve los tokens, o null si las credenciales son inválidas.
    /// </summary>
    Task<AuthResponseDto?> IniciarSesionAsync(LoginDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renueva el par de tokens a partir de un token de refresco vigente.
    /// </summary>
    Task<AuthResponseDto?> RefrescarAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoca un token de refresco (cierre de sesión).
    /// </summary>
    Task<bool> CerrarSesionAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve los datos del usuario autenticado.
    /// </summary>
    Task<UsuarioActualDto?> ObtenerUsuarioActualAsync(CancellationToken cancellationToken = default);
}
