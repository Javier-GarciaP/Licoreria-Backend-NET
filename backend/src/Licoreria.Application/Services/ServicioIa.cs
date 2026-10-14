using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Services;

public sealed class ServicioIa : IServicioIa
{
    private readonly IAiRepository _repositorio;
    private readonly IProveedorIa _proveedor;

    public ServicioIa(IAiRepository repositorio, IProveedorIa proveedor)
    {
        _repositorio = repositorio;
        _proveedor = proveedor;
    }

    public async Task<AiGeneracionDto> SolicitarPlanoAsync(string nombreArchivo, CancellationToken cancellationToken = default)
    {
        var generacion = await EjecutarAsync("plano", $"{{\"archivo\":\"{nombreArchivo}\"}}", cancellationToken);

        await _repositorio.AgregarPlanoAsync(new PlanoGenerado
        {
            AiGeneracionId = generacion.Id,
            Resultado = generacion.Salida ?? "{}"
        }, cancellationToken);

        await _repositorio.SaveChangesAsync(cancellationToken);
        return Mapear(generacion);
    }

    public async Task<AiGeneracionDto> SolicitarSeccionAsync(SolicitarSeccionIaDto dto, CancellationToken cancellationToken = default)
    {
        var entrada = $"{{\"prompt\":\"{dto.Prompt}\",\"plantilla\":\"{dto.Plantilla}\"}}";
        var generacion = await EjecutarAsync("seccion_web", entrada, cancellationToken);
        return Mapear(generacion);
    }

    public async Task<AiGeneracionDto> SolicitarImagenAsync(SolicitarImagenIaDto dto, CancellationToken cancellationToken = default)
    {
        var entrada = $"{{\"prompt\":\"{dto.Prompt}\",\"estilo\":\"{dto.Estilo}\"}}";
        var generacion = await EjecutarAsync("imagen", entrada, cancellationToken);
        return Mapear(generacion);
    }

    public async Task<ResultadoPaginado<AiGeneracionDto>> ObtenerGeneracionesAsync(
        PaginacionRequest paginacion,
        string? tipo = null,
        EstadoIa? estado = null,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _repositorio.ObtenerPaginadoAsync(paginacion, tipo, estado, cancellationToken);
        var items = pagina.Items.Select(Mapear).ToList();
        return ResultadoPaginado<AiGeneracionDto>.Crear(items, pagina.Page, pagina.PageSize, pagina.TotalItems);
    }

    public async Task<AiGeneracionDto?> ObtenerGeneracionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var generacion = await _repositorio.ObtenerConDetalleAsync(id, cancellationToken);
        return generacion is null ? null : Mapear(generacion);
    }

    public async Task<AiGeneracionDto?> AprobarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var generacion = await _repositorio.ObtenerConDetalleAsync(id, cancellationToken);
        if (generacion is null || generacion.IsDeleted)
        {
            return null;
        }

        generacion.Aprobar();
        await _repositorio.SaveChangesAsync(cancellationToken);
        return Mapear(generacion);
    }

    private async Task<AiGeneracion> EjecutarAsync(string tipo, string entrada, CancellationToken cancellationToken)
    {
        var resultado = await _proveedor.GenerarAsync(new TrabajoIa(tipo, entrada), cancellationToken);

        var generacion = new AiGeneracion
        {
            Tipo = tipo,
            Entrada = entrada
        };

        generacion.Completar(resultado.Salida, resultado.Modelo, resultado.Costo);

        await _repositorio.AgregarAsync(generacion, cancellationToken);
        await _repositorio.SaveChangesAsync(cancellationToken);
        return generacion;
    }

    private static AiGeneracionDto Mapear(AiGeneracion a)
        => new(a.Id, a.Tipo, a.Estado, a.Entrada, a.Salida, a.Modelo, a.Costo, a.Aprobado, a.CreatedAt);
}
