using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Services;

public sealed class ServicioContenido : IServicioContenido
{
    private readonly IRepository<Pagina> _paginas;
    private readonly IRepository<Seccion> _secciones;
    private readonly IRepository<HorarioAtencion> _horarios;
    private readonly IRepository<LocalInfo> _localInfo;
    private readonly IRepository<MediaAsset> _media;
    private readonly IProductoRepository _productos;
    private readonly IServicioFinanzas _finanzas;
    private readonly IRelojSistema _reloj;

    public ServicioContenido(
        IRepository<Pagina> paginas,
        IRepository<Seccion> secciones,
        IRepository<HorarioAtencion> horarios,
        IRepository<LocalInfo> localInfo,
        IRepository<MediaAsset> media,
        IProductoRepository productos,
        IServicioFinanzas finanzas,
        IRelojSistema reloj)
    {
        _paginas = paginas;
        _secciones = secciones;
        _horarios = horarios;
        _localInfo = localInfo;
        _media = media;
        _productos = productos;
        _finanzas = finanzas;
        _reloj = reloj;
    }

    // ================= Páginas =================

    public async Task<IReadOnlyList<PaginaDto>> ObtenerPaginasAsync(bool soloPublicadas = false, CancellationToken cancellationToken = default)
    {
        var paginas = await _paginas.FindAsync(p => !p.IsDeleted && (!soloPublicadas || p.Publicada), cancellationToken);
        var resultado = new List<PaginaDto>();
        foreach (var pagina in paginas)
        {
            var completa = await _paginas.GetByIdAsync(pagina.Id, cancellationToken);
            if (completa is not null)
            {
                resultado.Add(MapearPagina(completa));
            }
        }

        return resultado;
    }

    public async Task<PaginaDto?> ObtenerPaginaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pagina = await _paginas.GetByIdAsync(id, cancellationToken);
        return pagina is null || pagina.IsDeleted ? null : MapearPagina(pagina);
    }

    public async Task<PaginaDto> CrearPaginaAsync(PaginaCrearDto dto, CancellationToken cancellationToken = default)
    {
        var pagina = new Pagina
        {
            Titulo = dto.Titulo,
            Slug = dto.Slug,
            Publicada = dto.Publicada,
            Activo = true
        };

        foreach (var seccion in dto.Secciones)
        {
            pagina.Secciones.Add(CrearSeccion(seccion));
        }

        await _paginas.AddAsync(pagina, cancellationToken);
        await _paginas.SaveChangesAsync(cancellationToken);
        return MapearPagina(pagina);
    }

    public async Task<PaginaDto?> EditarPaginaAsync(PaginaEditarDto dto, CancellationToken cancellationToken = default)
    {
        var pagina = await _paginas.GetByIdAsync(dto.Id, cancellationToken);
        if (pagina is null || pagina.IsDeleted)
        {
            return null;
        }

        pagina.Titulo = dto.Titulo;
        pagina.Slug = dto.Slug;
        pagina.Publicada = dto.Publicada;
        pagina.Activo = dto.Activo;

        var existentes = await _secciones.FindAsync(s => s.PaginaId == dto.Id, cancellationToken);
        foreach (var seccion in existentes)
        {
            var tracked = await _secciones.GetByIdAsync(seccion.Id, cancellationToken);
            if (tracked is not null)
            {
                _secciones.Remove(tracked);
            }
        }

        foreach (var seccion in dto.Secciones)
        {
            var nueva = CrearSeccion(seccion);
            nueva.PaginaId = dto.Id;
            await _secciones.AddAsync(nueva, cancellationToken);
        }

        _paginas.Update(pagina);
        await _paginas.SaveChangesAsync(cancellationToken);

        var actualizada = await _paginas.GetByIdAsync(dto.Id, cancellationToken);
        return actualizada is null ? null : MapearPagina(actualizada);
    }

    public async Task<bool> EliminarPaginaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pagina = await _paginas.GetByIdAsync(id, cancellationToken);
        if (pagina is null || pagina.IsDeleted)
        {
            return false;
        }

        pagina.EliminarLogico();
        _paginas.Update(pagina);
        await _paginas.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ================= Horarios =================

    public async Task<IReadOnlyList<HorarioDto>> ObtenerHorariosAsync(CancellationToken cancellationToken = default)
    {
        var horarios = await _horarios.GetAllAsync(cancellationToken);
        return horarios.Where(h => !h.IsDeleted).OrderBy(h => h.DiaSemana).Select(MapearHorario).ToList();
    }

    public async Task<HorarioDto> GuardarHorarioAsync(HorarioGuardarDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.DiaSemana is < 1 or > 7)
        {
            throw new ReglaNegocioException("El día de la semana debe estar entre 1 (lunes) y 7 (domingo).");
        }

        var existentes = await _horarios.FindAsync(h => h.DiaSemana == dto.DiaSemana && !h.IsDeleted, cancellationToken);
        var horario = existentes.FirstOrDefault();

        if (horario is null)
        {
            horario = new HorarioAtencion { DiaSemana = dto.DiaSemana };
            horario.Abierto = dto.Abierto;
            horario.HoraApertura = dto.HoraApertura;
            horario.HoraCierre = dto.HoraCierre;
            await _horarios.AddAsync(horario, cancellationToken);
        }
        else
        {
            horario.Abierto = dto.Abierto;
            horario.HoraApertura = dto.HoraApertura;
            horario.HoraCierre = dto.HoraCierre;
            _horarios.Update(horario);
        }

        await _horarios.SaveChangesAsync(cancellationToken);
        return MapearHorario(horario);
    }

    // ================= Local info =================

    public async Task<LocalInfoDto> ObtenerLocalInfoAsync(CancellationToken cancellationToken = default)
    {
        var infos = await _localInfo.FindAsync(l => !l.IsDeleted, cancellationToken);
        var info = infos.FirstOrDefault();

        if (info is null)
        {
            info = new LocalInfo { Nombre = "Licorería / Discoteca" };
            await _localInfo.AddAsync(info, cancellationToken);
            await _localInfo.SaveChangesAsync(cancellationToken);
        }

        return MapearLocalInfo(info);
    }

    public async Task<LocalInfoDto> ActualizarLocalInfoAsync(LocalInfoEditarDto dto, CancellationToken cancellationToken = default)
    {
        var infos = await _localInfo.FindAsync(l => !l.IsDeleted, cancellationToken);
        var existente = infos.FirstOrDefault();

        if (existente is null)
        {
            var nuevo = new LocalInfo();
            Asignar(nuevo, dto);
            await _localInfo.AddAsync(nuevo, cancellationToken);
            await _localInfo.SaveChangesAsync(cancellationToken);
            return MapearLocalInfo(nuevo);
        }

        var info = await _localInfo.GetByIdAsync(existente.Id, cancellationToken) ?? existente;
        Asignar(info, dto);
        _localInfo.Update(info);
        await _localInfo.SaveChangesAsync(cancellationToken);
        return MapearLocalInfo(info);
    }

    private static void Asignar(LocalInfo info, LocalInfoEditarDto dto)
    {
        info.Nombre = dto.Nombre;
        info.Descripcion = dto.Descripcion;
        info.Direccion = dto.Direccion;
        info.Telefono = dto.Telefono;
        info.Whatsapp = dto.Whatsapp;
        info.Email = dto.Email;
        info.Instagram = dto.Instagram;
        info.Facebook = dto.Facebook;
        info.MapaUrl = dto.MapaUrl;
        info.LogoUrl = dto.LogoUrl;
    }

    // ================= Media =================

    public async Task<IReadOnlyList<MediaAssetDto>> ObtenerMediaAsync(CancellationToken cancellationToken = default)
    {
        var media = await _media.GetAllAsync(cancellationToken);
        return media.Where(m => !m.IsDeleted).Select(MapearMedia).ToList();
    }

    public async Task<MediaAssetDto> RegistrarMediaAsync(
        string nombre,
        string rutaRelativa,
        string url,
        string tipo,
        long tamano,
        CancellationToken cancellationToken = default)
    {
        var asset = new MediaAsset
        {
            Nombre = nombre,
            RutaRelativa = rutaRelativa,
            Url = url,
            Tipo = tipo,
            Tamano = tamano,
            Activo = true
        };

        await _media.AddAsync(asset, cancellationToken);
        await _media.SaveChangesAsync(cancellationToken);
        return MapearMedia(asset);
    }

    public async Task<bool> EliminarMediaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var asset = await _media.GetByIdAsync(id, cancellationToken);
        if (asset is null || asset.IsDeleted)
        {
            return false;
        }

        asset.EliminarLogico();
        _media.Update(asset);
        await _media.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ================= Menú digital =================

    public async Task<MenuDigitalDto> ObtenerMenuDigitalAsync(CancellationToken cancellationToken = default)
    {
        var tasa = await _finanzas.ObtenerValorVigenteAsync(TipoTasa.Paralelo, cancellationToken);
        var productos = await _productos.ObtenerTodosConDetalleAsync(cancellationToken);

        var secciones = productos
            .Where(p => p.Activo)
            .GroupBy(p => new { p.CategoriaId, Nombre = p.Categoria?.Nombre ?? "Sin categoría" })
            .Select(g => new MenuSeccionDto(
                g.Key.CategoriaId,
                g.Key.Nombre,
                g.SelectMany(p => p.Variantes.Where(v => v.Activo).Select(v => new MenuItemDto(
                    v.Id,
                    $"{p.Nombre} · {v.Nombre}",
                    v.Sku,
                    v.PrecioVentaUSD,
                    Math.Round(v.PrecioVentaUSD * tasa, 2),
                    p.ImagenUrl)))
                    .OrderBy(i => i.Nombre)
                    .ToList()))
            .OrderBy(s => s.Nombre)
            .ToList();

        return new MenuDigitalDto("Menú digital", tasa, _reloj.UtcNow, secciones);
    }

    public Task<QrMenuDto> ObtenerQrMenuAsync(string baseUrl, CancellationToken cancellationToken = default)
    {
        var url = $"{baseUrl.TrimEnd('/')}/menu";
        return Task.FromResult(new QrMenuDto(url, url));
    }

    // ================= Mapeos =================

    private static Seccion CrearSeccion(SeccionCrearDto dto)
    {
        var seccion = new Seccion
        {
            Titulo = dto.Titulo,
            Tipo = dto.Tipo,
            Orden = dto.Orden,
            Activa = dto.Activa
        };

        foreach (var bloque in dto.Bloques)
        {
            seccion.Bloques.Add(new BloqueContenido
            {
                Tipo = bloque.Tipo,
                Orden = bloque.Orden,
                Contenido = bloque.Contenido,
                Activo = bloque.Activo
            });
        }

        return seccion;
    }

    private static PaginaDto MapearPagina(Pagina p)
        => new(p.Id, p.Titulo, p.Slug, p.Publicada, p.Activo,
            p.Secciones.Where(s => !s.IsDeleted).OrderBy(s => s.Orden).Select(s => new SeccionDto(
                s.Id, s.Titulo, s.Tipo, s.Orden, s.Activa,
                s.Bloques.Where(b => !b.IsDeleted).OrderBy(b => b.Orden).Select(b => new BloqueDto(
                    b.Id, b.Tipo, b.Orden, b.Contenido, b.Activo)).ToList())).ToList());

    private static HorarioDto MapearHorario(HorarioAtencion h)
        => new(h.Id, h.DiaSemana, h.Abierto, h.HoraApertura, h.HoraCierre);

    private static LocalInfoDto MapearLocalInfo(LocalInfo l)
        => new(l.Id, l.Nombre, l.Descripcion, l.Direccion, l.Telefono, l.Whatsapp, l.Email,
            l.Instagram, l.Facebook, l.MapaUrl, l.LogoUrl);

    private static MediaAssetDto MapearMedia(MediaAsset m)
        => new(m.Id, m.Nombre, m.Url, m.Tipo, m.Tamano);
}
