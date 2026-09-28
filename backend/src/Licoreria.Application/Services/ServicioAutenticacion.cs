using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;

namespace Licoreria.Application.Services;

public sealed class ServicioAutenticacion : IServicioAutenticacion
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public ServicioAutenticacion(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto?> IniciarSesionAsync(
        LoginDto dto,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObtenerPorEmailAsync(dto.Username, cancellationToken);

        if (usuario is null || !usuario.Activo)
        {
            return null;
        }

        if (!_passwordHasher.Verify(dto.Password, usuario.PasswordHash))
        {
            return null;
        }

        var (token, expira) = _tokenService.GenerarToken(usuario);
        var rolSeguridad = usuario.Rol.ObtenerRoles().First();

        return new AuthResponseDto(
            token,
            usuario.NombreCompleto,
            usuario.Email,
            rolSeguridad,
            expira);
    }
}
