using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Services;

public sealed class ServicioClub : IServicioClub
{
    private readonly IRepository<Zona> _zonas;
    private readonly IRepository<Mesa> _mesas;
    private readonly IRepository<Plano> _planos;
    private readonly IRepository<PlanoElemento> _planoElementos;
    private readonly IRepository<Evento> _eventos;
    private readonly IReservaRepository _reservas;
    private readonly ICuentaRepository _cuentas;
    private readonly IRelojSistema _reloj;

    public ServicioClub(
        IRepository<Zona> zonas,
        IRepository<Mesa> mesas,
        IRepository<Plano> planos,
        IRepository<PlanoElemento> planoElementos,
        IRepository<Evento> eventos,
        IReservaRepository reservas,
        ICuentaRepository cuentas,
        IRelojSistema reloj)
    {
        _zonas = zonas;
        _mesas = mesas;
        _planos = planos;
        _planoElementos = planoElementos;
        _eventos = eventos;
        _reservas = reservas;
        _cuentas = cuentas;
        _reloj = reloj;
    }

    // ================= Zonas =================

    public async Task<IReadOnlyList<ZonaDto>> ObtenerZonasAsync(CancellationToken cancellationToken = default)
    {
        var zonas = await _zonas.GetAllAsync(cancellationToken);
        return zonas.Where(z => !z.IsDeleted).Select(MapearZona).ToList();
    }

    public async Task<ZonaDto> CrearZonaAsync(ZonaCrearDto dto, CancellationToken cancellationToken = default)
    {
        var zona = new Zona { Nombre = dto.Nombre, Tipo = dto.Tipo, Activo = dto.Activo };
        await _zonas.AddAsync(zona, cancellationToken);
        await _zonas.SaveChangesAsync(cancellationToken);
        return MapearZona(zona);
    }

    public async Task<ZonaDto?> EditarZonaAsync(ZonaEditarDto dto, CancellationToken cancellationToken = default)
    {
        var zona = await _zonas.GetByIdAsync(dto.Id, cancellationToken);
        if (zona is null || zona.IsDeleted)
        {
            return null;
        }

        zona.Nombre = dto.Nombre;
        zona.Tipo = dto.Tipo;
        zona.Activo = dto.Activo;
        _zonas.Update(zona);
        await _zonas.SaveChangesAsync(cancellationToken);
        return MapearZona(zona);
    }

    public async Task<bool> EliminarZonaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var zona = await _zonas.GetByIdAsync(id, cancellationToken);
        if (zona is null || zona.IsDeleted)
        {
            return false;
        }

        zona.EliminarLogico();
        _zonas.Update(zona);
        await _zonas.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ================= Mesas =================

    public async Task<IReadOnlyList<MesaDto>> ObtenerMesasAsync(Guid? zonaId = null, CancellationToken cancellationToken = default)
    {
        var mesas = await _mesas.FindAsync(m => !m.IsDeleted && (zonaId == null || m.ZonaId == zonaId), cancellationToken);
        var zonas = (await _zonas.GetAllAsync(cancellationToken)).ToDictionary(z => z.Id, z => z.Nombre);
        var ocupadas = (await _cuentas.ObtenerMesasOcupadasAsync(cancellationToken)).ToHashSet();

        return mesas
            .Select(m => MapearMesa(m, zonas.GetValueOrDefault(m.ZonaId, string.Empty), !ocupadas.Contains(m.Id)))
            .ToList();
    }

    public async Task<MesaDto> CrearMesaAsync(MesaCrearDto dto, CancellationToken cancellationToken = default)
    {
        var zona = await _zonas.GetByIdAsync(dto.ZonaId, cancellationToken)
            ?? throw new NoEncontradoException($"No existe la zona {dto.ZonaId}.");

        var mesa = new Mesa
        {
            ZonaId = dto.ZonaId,
            Numero = dto.Numero,
            Capacidad = dto.Capacidad,
            Forma = dto.Forma,
            PosX = dto.PosX,
            PosY = dto.PosY,
            Ancho = dto.Ancho,
            Alto = dto.Alto,
            Activa = true
        };

        await _mesas.AddAsync(mesa, cancellationToken);
        await _mesas.SaveChangesAsync(cancellationToken);
        return MapearMesa(mesa, zona.Nombre, true);
    }

    public async Task<MesaDto?> EditarMesaAsync(MesaEditarDto dto, CancellationToken cancellationToken = default)
    {
        var mesa = await _mesas.GetByIdAsync(dto.Id, cancellationToken);
        if (mesa is null || mesa.IsDeleted)
        {
            return null;
        }

        mesa.ZonaId = dto.ZonaId;
        mesa.Numero = dto.Numero;
        mesa.Capacidad = dto.Capacidad;
        mesa.Forma = dto.Forma;
        mesa.PosX = dto.PosX;
        mesa.PosY = dto.PosY;
        mesa.Ancho = dto.Ancho;
        mesa.Alto = dto.Alto;
        mesa.Activa = dto.Activa;
        _mesas.Update(mesa);
        await _mesas.SaveChangesAsync(cancellationToken);

        var zona = await _zonas.GetByIdAsync(dto.ZonaId, cancellationToken);
        return MapearMesa(mesa, zona?.Nombre ?? string.Empty, true);
    }

    public async Task<bool> EliminarMesaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mesa = await _mesas.GetByIdAsync(id, cancellationToken);
        if (mesa is null || mesa.IsDeleted)
        {
            return false;
        }

        mesa.EliminarLogico();
        _mesas.Update(mesa);
        await _mesas.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ================= Planos =================

    public async Task<IReadOnlyList<PlanoDto>> ObtenerPlanosAsync(CancellationToken cancellationToken = default)
    {
        var planos = await _planos.FindAsync(p => !p.IsDeleted, cancellationToken);
        var resultado = new List<PlanoDto>();
        foreach (var plano in planos)
        {
            var completo = await _planos.GetByIdAsync(plano.Id, cancellationToken);
            if (completo is not null)
            {
                resultado.Add(MapearPlano(completo));
            }
        }

        return resultado;
    }

    public async Task<PlanoDto?> ObtenerPlanoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plano = await _planos.GetByIdAsync(id, cancellationToken);
        return plano is null || plano.IsDeleted ? null : MapearPlano(plano);
    }

    public async Task<PlanoDto> CrearPlanoAsync(PlanoCrearDto dto, CancellationToken cancellationToken = default)
    {
        var plano = new Plano { Nombre = dto.Nombre, Version = 1, Activo = true };

        foreach (var elemento in dto.Elementos)
        {
            plano.Elementos.Add(CrearElemento(elemento));
        }

        await _planos.AddAsync(plano, cancellationToken);
        await _planos.SaveChangesAsync(cancellationToken);
        return MapearPlano(plano);
    }

    public async Task<PlanoDto?> EditarPlanoAsync(PlanoEditarDto dto, CancellationToken cancellationToken = default)
    {
        var plano = await _planos.GetByIdAsync(dto.Id, cancellationToken);
        if (plano is null || plano.IsDeleted)
        {
            return null;
        }

        plano.Nombre = dto.Nombre;
        plano.Activo = dto.Activo;
        plano.Version += 1;

        var existentes = await _planoElementos.FindAsync(e => e.PlanoId == dto.Id, cancellationToken);
        foreach (var elemento in existentes)
        {
            var tracked = await _planoElementos.GetByIdAsync(elemento.Id, cancellationToken);
            if (tracked is not null)
            {
                _planoElementos.Remove(tracked);
            }
        }

        foreach (var elemento in dto.Elementos)
        {
            var nuevo = CrearElemento(elemento);
            nuevo.PlanoId = dto.Id;
            await _planoElementos.AddAsync(nuevo, cancellationToken);
        }

        _planos.Update(plano);
        await _planos.SaveChangesAsync(cancellationToken);

        var actualizado = await _planos.GetByIdAsync(dto.Id, cancellationToken);
        return actualizado is null ? null : MapearPlano(actualizado);
    }

    public async Task<bool> EliminarPlanoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plano = await _planos.GetByIdAsync(id, cancellationToken);
        if (plano is null || plano.IsDeleted)
        {
            return false;
        }

        plano.EliminarLogico();
        _planos.Update(plano);
        await _planos.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ================= Reservas =================

    public async Task<ResultadoPaginado<ReservaDto>> ObtenerReservasAsync(
        PaginacionRequest paginacion,
        DateTime? desde = null,
        DateTime? hasta = null,
        EstadoReserva? estado = null,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _reservas.ObtenerPaginadoAsync(paginacion, desde, hasta, estado, cancellationToken);
        var items = pagina.Items.Select(MapearReserva).ToList();
        return ResultadoPaginado<ReservaDto>.Crear(items, pagina.Page, pagina.PageSize, pagina.TotalItems);
    }

    public async Task<ReservaDto?> ObtenerReservaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var reserva = await _reservas.ObtenerConDetalleAsync(id, cancellationToken);
        return reserva is null ? null : MapearReserva(reserva);
    }

    public async Task<ReservaDto> CrearReservaAsync(ReservaCrearDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Personas <= 0)
        {
            throw new ReglaNegocioException("El número de personas debe ser mayor que cero.");
        }

        if (dto.Mesas.Count == 0)
        {
            throw new ReglaNegocioException("La reserva debe incluir al menos una mesa.");
        }

        if (dto.FechaHora < _reloj.UtcNow)
        {
            throw new ReglaNegocioException("La fecha de la reserva no puede estar en el pasado.");
        }

        var reserva = new Reserva
        {
            FechaHora = dto.FechaHora,
            Personas = dto.Personas,
            Origen = dto.Origen,
            NombreContacto = dto.NombreContacto,
            Telefono = dto.Telefono,
            Notas = dto.Notas,
            Estado = EstadoReserva.Pendiente
        };

        foreach (var mesaId in dto.Mesas.Distinct())
        {
            var mesa = await _mesas.GetByIdAsync(mesaId, cancellationToken)
                ?? throw new NoEncontradoException($"No existe la mesa {mesaId}.");
            reserva.Mesas.Add(new ReservaMesa { MesaId = mesa.Id });
        }

        await _reservas.AgregarAsync(reserva, cancellationToken);
        await _reservas.SaveChangesAsync(cancellationToken);

        var creada = await _reservas.ObtenerConDetalleAsync(reserva.Id, cancellationToken);
        return MapearReserva(creada!);
    }

    public async Task<ReservaDto?> RegistrarPagoReservaAsync(
        Guid reservaId,
        ReservaPagoCrearDto dto,
        CancellationToken cancellationToken = default)
    {
        var reserva = await _reservas.ObtenerConDetalleAsync(reservaId, cancellationToken);
        if (reserva is null || reserva.IsDeleted)
        {
            return null;
        }

        if (dto.Monto <= 0)
        {
            throw new ReglaNegocioException("El monto de la seña debe ser mayor que cero.");
        }

        await _reservas.AgregarPagoAsync(new ReservaPago
        {
            ReservaId = reservaId,
            MetodoPagoId = dto.MetodoPagoId,
            Monto = dto.Monto,
            Moneda = dto.Moneda,
            ComprobanteUrl = dto.ComprobanteUrl,
            Estado = EstadoReservaPago.Pendiente
        }, cancellationToken);

        await _reservas.SaveChangesAsync(cancellationToken);

        var actualizada = await _reservas.ObtenerConDetalleAsync(reservaId, cancellationToken);
        return actualizada is null ? null : MapearReserva(actualizada);
    }

    public async Task<ReservaDto?> ValidarPagoReservaAsync(
        Guid reservaId,
        Guid pagoId,
        ValidarReservaPagoDto dto,
        CancellationToken cancellationToken = default)
    {
        var reserva = await _reservas.ObtenerConDetalleAsync(reservaId, cancellationToken);
        if (reserva is null || reserva.IsDeleted)
        {
            return null;
        }

        var pago = await _reservas.ObtenerPagoAsync(reservaId, pagoId, cancellationToken);
        if (pago is null)
        {
            return null;
        }

        if (dto.Aprobar)
        {
            pago.Validar();
            reserva.Confirmar();
        }
        else
        {
            pago.Rechazar();
        }

        await _reservas.SaveChangesAsync(cancellationToken);

        var actualizada = await _reservas.ObtenerConDetalleAsync(reservaId, cancellationToken);
        return actualizada is null ? null : MapearReserva(actualizada);
    }

    public async Task<ReservaDto?> CambiarEstadoReservaAsync(
        Guid reservaId,
        CambiarEstadoReservaDto dto,
        CancellationToken cancellationToken = default)
    {
        var reserva = await _reservas.ObtenerConDetalleAsync(reservaId, cancellationToken);
        if (reserva is null || reserva.IsDeleted)
        {
            return null;
        }

        switch (dto.Estado)
        {
            case EstadoReserva.Confirmada:
                reserva.Confirmar();
                break;
            case EstadoReserva.Cancelada:
                reserva.Cancelar();
                break;
            case EstadoReserva.Asistio:
                reserva.MarcarAsistencia();
                break;
            default:
                reserva.Estado = dto.Estado;
                break;
        }

        await _reservas.SaveChangesAsync(cancellationToken);
        return MapearReserva(reserva);
    }

    // ================= Eventos =================

    public async Task<IReadOnlyList<EventoDto>> ObtenerEventosAsync(bool soloPublicados = false, CancellationToken cancellationToken = default)
    {
        var eventos = await _eventos.FindAsync(
            e => !e.IsDeleted && (!soloPublicados || e.Publicado),
            cancellationToken);

        return eventos.OrderBy(e => e.FechaInicio).Select(MapearEvento).ToList();
    }

    public async Task<EventoDto> CrearEventoAsync(EventoCrearDto dto, CancellationToken cancellationToken = default)
    {
        var evento = new Evento
        {
            Titulo = dto.Titulo,
            Descripcion = dto.Descripcion,
            FechaInicio = dto.FechaInicio,
            FechaFin = dto.FechaFin,
            ImagenUrl = dto.ImagenUrl,
            Publicado = dto.Publicado,
            Activo = true
        };

        await _eventos.AddAsync(evento, cancellationToken);
        await _eventos.SaveChangesAsync(cancellationToken);
        return MapearEvento(evento);
    }

    public async Task<EventoDto?> EditarEventoAsync(EventoEditarDto dto, CancellationToken cancellationToken = default)
    {
        var evento = await _eventos.GetByIdAsync(dto.Id, cancellationToken);
        if (evento is null || evento.IsDeleted)
        {
            return null;
        }

        evento.Titulo = dto.Titulo;
        evento.Descripcion = dto.Descripcion;
        evento.FechaInicio = dto.FechaInicio;
        evento.FechaFin = dto.FechaFin;
        evento.ImagenUrl = dto.ImagenUrl;
        evento.Publicado = dto.Publicado;
        evento.Activo = dto.Activo;
        _eventos.Update(evento);
        await _eventos.SaveChangesAsync(cancellationToken);
        return MapearEvento(evento);
    }

    public async Task<bool> EliminarEventoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var evento = await _eventos.GetByIdAsync(id, cancellationToken);
        if (evento is null || evento.IsDeleted)
        {
            return false;
        }

        evento.EliminarLogico();
        _eventos.Update(evento);
        await _eventos.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ================= Mapeos =================

    private static PlanoElemento CrearElemento(PlanoElementoCrearDto dto) => new()
    {
        ZonaId = dto.ZonaId,
        Tipo = dto.Tipo,
        Etiqueta = dto.Etiqueta,
        PosX = dto.PosX,
        PosY = dto.PosY,
        Ancho = dto.Ancho,
        Alto = dto.Alto,
        Rotacion = dto.Rotacion
    };

    private static ZonaDto MapearZona(Zona z) => new(z.Id, z.Nombre, z.Tipo, z.Activo);

    private static MesaDto MapearMesa(Mesa m, string zonaNombre, bool disponible)
        => new(m.Id, m.ZonaId, zonaNombre, m.Numero, m.Capacidad, m.Forma, m.PosX, m.PosY, m.Ancho, m.Alto, m.Activa, disponible);

    private static PlanoDto MapearPlano(Plano p)
        => new(p.Id, p.Nombre, p.Version, p.Activo,
            p.Elementos.Where(e => !e.IsDeleted).Select(e => new PlanoElementoDto(
                e.Id, e.ZonaId, e.Tipo, e.Etiqueta, e.PosX, e.PosY, e.Ancho, e.Alto, e.Rotacion)).ToList());

    private static ReservaDto MapearReserva(Reserva r)
        => new(
            r.Id,
            r.FechaHora,
            r.Personas,
            r.Estado,
            r.Origen,
            r.NombreContacto,
            r.Telefono,
            r.Notas,
            r.Mesas.Select(rm => new ReservaMesaResumenDto(
                rm.MesaId,
                rm.Mesa?.Numero ?? string.Empty,
                rm.Mesa?.Zona?.Nombre ?? string.Empty)).ToList(),
            r.Pagos.Select(p => new ReservaPagoDto(
                p.Id,
                p.MetodoPagoId,
                p.MetodoPago?.Nombre ?? string.Empty,
                p.Monto,
                p.Moneda,
                p.ComprobanteUrl,
                p.Estado)).ToList());

    private static EventoDto MapearEvento(Evento e)
        => new(e.Id, e.Titulo, e.Descripcion, e.FechaInicio, e.FechaFin, e.ImagenUrl, e.Publicado, e.Activo);
}
