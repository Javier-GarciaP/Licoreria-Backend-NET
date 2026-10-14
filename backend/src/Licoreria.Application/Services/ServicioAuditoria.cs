using System.Text.Json;
using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

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

    public void Registrar(
        string accion,
        string entidad,
        Guid? entidadId = null,
        object? datos = null)
        => _repositorio.Agregar(CrearEvento(accion, entidad, entidadId, datos));

    public async Task RegistrarAsync(
        string accion,
        string entidad,
        Guid? entidadId = null,
        object? datos = null,
        CancellationToken cancellationToken = default)
    {
        _repositorio.Agregar(CrearEvento(accion, entidad, entidadId, datos));
        await _repositorio.SaveChangesAsync(cancellationToken);
    }

    public async Task RegistrarAccesoAsync(
        string accion,
        Guid? usuarioId,
        string? correo,
        object? datos = null,
        CancellationToken cancellationToken = default)
    {
        _repositorio.Agregar(CrearEvento(accion, "sesion", usuarioId, datos, usuarioId, correo));
        await _repositorio.SaveChangesAsync(cancellationToken);
    }

    private AuditLog CrearEvento(
        string accion,
        string entidad,
        Guid? entidadId,
        object? datos,
        Guid? usuarioId = null,
        string? correo = null)
    {
        usuarioId ??= _contextoUsuario.UsuarioId;
        var log = new AuditLog
        {
            UsuarioId = usuarioId,
            Usuario = correo ?? _contextoUsuario.Email,
            Accion = accion,
            Entidad = entidad,
            EntidadId = entidadId,
            Datos = datos is null ? null : JsonSerializer.Serialize(datos),
            Ip = _contextoUsuario.Ip,
            Tipo = TipoAuditoria.Evento
        };

        log.EstablecerCreador(usuarioId, _reloj.UtcNow);
        return log;
    }

    public async Task<ResultadoPaginado<AuditLogDto>> ObtenerAsync(
        PaginacionRequest paginacion,
        string? entidad = null,
        Guid? usuarioId = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        string? accion = null,
        TipoAuditoria? tipo = null,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _repositorio.ObtenerPaginadoAsync(
            paginacion, entidad, usuarioId, AUtc(desde), AUtc(hasta), accion, tipo, cancellationToken);
        var items = pagina.Items.Select(Mapear).ToList();
        return ResultadoPaginado<AuditLogDto>.Crear(items, pagina.Page, pagina.PageSize, pagina.TotalItems);
    }

    public async Task<AuditLogDto?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var log = await _repositorio.ObtenerPorIdAsync(id, cancellationToken);
        return log is null ? null : Mapear(log);
    }

    public Task<IReadOnlyList<string>> ObtenerEntidadesAsync(CancellationToken cancellationToken = default)
        => _repositorio.ObtenerEntidadesAsync(cancellationToken);

    /// <summary>Un instante sin zona (p. ej. "2026-10-14") se interpreta como UTC; PostgreSQL no admite Unspecified.</summary>
    private static DateTime? AUtc(DateTime? fecha)
        => fecha is null ? null : fecha.Value.Kind switch
        {
            DateTimeKind.Utc => fecha,
            DateTimeKind.Local => fecha.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(fecha.Value, DateTimeKind.Utc)
        };

    private static AuditLogDto Mapear(AuditLog l)
        => new(l.Id, l.UsuarioId, l.Usuario, l.Accion, l.Entidad, l.EntidadId, l.Datos, l.Ip, l.CreatedAt, l.Tipo.ToString());
}
