using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;

namespace Licoreria.Application.Services;

public sealed class ServicioUsuarios : IServicioUsuarios
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ServicioUsuarios(IUsuarioRepository usuarioRepository, IPasswordHasher passwordHasher)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<ResultadoPaginado<UsuarioDto>> ObtenerUsuariosAsync(
        PaginacionRequest paginacion,
        string? busqueda = null,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _usuarioRepository.ObtenerPaginadoAsync(paginacion, busqueda, cancellationToken);
        var items = pagina.Items.Select(Mapear).ToList();

        return ResultadoPaginado<UsuarioDto>.Crear(items, pagina.Page, pagina.PageSize, pagina.TotalItems);
    }

    public async Task<UsuarioDto?> ObtenerUsuarioAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id, cancellationToken);
        return usuario is null || usuario.IsDeleted ? null : Mapear(usuario);
    }

    public async Task<UsuarioDto> CrearUsuarioAsync(
        UsuarioCrearDto dto,
        CancellationToken cancellationToken = default)
    {
        if (await _usuarioRepository.ExisteEmailAsync(dto.Email, null, cancellationToken))
        {
            throw new ConflictoException($"Ya existe un usuario con el correo '{dto.Email}'.");
        }

        var usuario = new Usuario
        {
            NombreCompleto = dto.NombreCompleto,
            Email = dto.Email,
            PasswordHash = _passwordHasher.Hash(dto.Password),
            Rol = dto.Rol,
            Activo = dto.Activo
        };

        await _usuarioRepository.AddAsync(usuario, cancellationToken);
        await _usuarioRepository.SaveChangesAsync(cancellationToken);

        return Mapear(usuario);
    }

    public async Task<UsuarioDto?> EditarUsuarioAsync(
        UsuarioEditarDto dto,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (usuario is null || usuario.IsDeleted)
        {
            return null;
        }

        if (await _usuarioRepository.ExisteEmailAsync(dto.Email, dto.Id, cancellationToken))
        {
            throw new ConflictoException($"Ya existe otro usuario con el correo '{dto.Email}'.");
        }

        usuario.ActualizarDatos(dto.NombreCompleto, dto.Email, dto.Rol);

        if (dto.Activo)
        {
            usuario.Activar();
        }
        else
        {
            usuario.Desactivar();
        }

        _usuarioRepository.Update(usuario);
        await _usuarioRepository.SaveChangesAsync(cancellationToken);

        return Mapear(usuario);
    }

    public async Task<bool> EliminarUsuarioAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id, cancellationToken);
        if (usuario is null || usuario.IsDeleted)
        {
            return false;
        }

        usuario.EliminarLogico();
        _usuarioRepository.Update(usuario);
        await _usuarioRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CambiarPasswordAsync(
        Guid id,
        CambiarPasswordDto dto,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id, cancellationToken);
        if (usuario is null || usuario.IsDeleted)
        {
            return false;
        }

        usuario.CambiarPasswordHash(_passwordHasher.Hash(dto.PasswordNueva));
        _usuarioRepository.Update(usuario);
        await _usuarioRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static UsuarioDto Mapear(Usuario usuario)
        => new(
            usuario.Id,
            usuario.NombreCompleto,
            usuario.Email,
            usuario.Rol.ToString(),
            usuario.Activo,
            usuario.CreatedAt);
}
