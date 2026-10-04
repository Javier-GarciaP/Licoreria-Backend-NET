using System.Text.Json;
using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;

namespace Licoreria.Application.Services;

public sealed class ServicioAuditoria : IServicioAuditoria
{
    private readonly IAuditoriaRepository _repositorio;
    private readonly IContextoUsuario _contextoUsuario;
    private readonly IRelojSistema _reloj;

    public ServicioAuditoria(
        IAuditoriaRepository repositorio,
        IContextoUsuario contextoUsuario,
        IRelojSistema reloj)
    {
        _repositorio = repositorio;
        _contextoUsuario = contextoUsuario;
        _reloj = reloj;
    }

    public async Task RegistrarAsync(
        string accion,
        string entidad,
        Guid? entidadId = null,
        object? datos = null,
        CancellationToken cancellationToken = default)
    {
        var log = new AuditLog
        {
            UsuarioId = _contextoUsuario.UsuarioId,
            Usuario = _contextoUsuario.Email,
            Accion = accion,
            Entidad = entidad,
            EntidadId = entidadId,
            Datos = datos is null ? null : JsonSerializer.Serialize(datos),
            Ip = _contextoUsuario.Ip
        };

        log.EstablecerCreador(_contextoUsuario.UsuarioId, _reloj.UtcNow);

        await _repositorio.AgregarAsync(log, cancellationToken);
        await _repositorio.SaveChangesAsync(cancellationToken);
    }

    public async Task<ResultadoPaginado<AuditLogDto>> ObtenerAsync(
        PaginacionRequest paginacion,
        string? entidad = null,
        Guid? usuarioId = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _repositorio.ObtenerPaginadoAsync(paginacion, entidad, usuarioId, desde, hasta, cancellationToken);
        var items = pagina.Items.Select(l => new AuditLogDto(
            l.Id, l.UsuarioId, l.Usuario, l.Accion, l.Entidad, l.EntidadId, l.Datos, l.Ip, l.CreatedAt)).ToList();
        return ResultadoPaginado<AuditLogDto>.Crear(items, pagina.Page, pagina.PageSize, pagina.TotalItems);
    }
}
