using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Security;
using Licoreria.Domain.Entities;

namespace Licoreria.Application.Services;

public sealed class ServicioAutenticacion : IServicioAutenticacion
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IRelojSistema _reloj;
    private readonly IContextoUsuario _contextoUsuario;
    private readonly OpcionesAutenticacion _opciones;

    public ServicioAutenticacion(
        IUsuarioRepository usuarioRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IRelojSistema reloj,
        IContextoUsuario contextoUsuario,
        OpcionesAutenticacion opciones)
    {
        _usuarioRepository = usuarioRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _reloj = reloj;
        _contextoUsuario = contextoUsuario;
        _opciones = opciones;
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

        return await EmitirTokensAsync(usuario, cancellationToken);
    }

    public async Task<AuthResponseDto?> RefrescarAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var ahora = _reloj.UtcNow;
        var token = await _refreshTokenRepository.ObtenerPorTokenAsync(refreshToken, cancellationToken);

        if (token is null || !token.EstaVigente(ahora))
        {
            return null;
        }

        var usuario = await _usuarioRepository.GetByIdAsync(token.UsuarioId, cancellationToken);
        if (usuario is null || !usuario.Activo)
        {
            return null;
        }

        token.Revocar(ahora);
        _refreshTokenRepository.Update(token);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return await EmitirTokensAsync(usuario, cancellationToken);
    }

    public async Task<bool> CerrarSesionAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var token = await _refreshTokenRepository.ObtenerPorTokenAsync(refreshToken, cancellationToken);
        if (token is null)
        {
            return false;
        }

        token.Revocar(_reloj.UtcNow);
        _refreshTokenRepository.Update(token);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<UsuarioActualDto?> ObtenerUsuarioActualAsync(CancellationToken cancellationToken = default)
    {
        var usuarioId = _contextoUsuario.UsuarioId;
        if (usuarioId is null)
        {
            return null;
        }

        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId.Value, cancellationToken);
        if (usuario is null || !usuario.Activo)
        {
            return null;
        }

        return new UsuarioActualDto(
            usuario.Id,
            usuario.NombreCompleto,
            usuario.Email,
            usuario.Rol.ObtenerRoles().First(),
            usuario.Rol.ToString(),
            usuario.Rol.ObtenerPermisos());
    }

    private async Task<AuthResponseDto> EmitirTokensAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        var (accessToken, expira) = _tokenService.GenerarToken(usuario);
        var refreshToken = _tokenService.GenerarTokenRefresco();

        var entidad = new RefreshToken(
            usuario.Id,
            refreshToken,
            _reloj.UtcNow.AddDays(_opciones.RefreshExpiresDays));

        await _refreshTokenRepository.AddAsync(entidad, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            usuario.Id,
            usuario.NombreCompleto,
            usuario.Email,
            usuario.Rol.ObtenerRoles().First(),
            usuario.Rol.ToString(),
            usuario.Rol.ObtenerPermisos(),
            accessToken,
            refreshToken,
            expira);
    }
}
