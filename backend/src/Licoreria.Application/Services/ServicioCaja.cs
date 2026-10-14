using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Services;

public sealed class ServicioCaja : IServicioCaja
{
    private readonly ISesionCajaRepository _sesionRepository;
    private readonly IRepository<Denominacion> _denominacionRepository;
    private readonly ICuentaRepository _cuentaRepository;
    private readonly IVentaRepository _ventaRepository;
    private readonly INotificadorComandas _notificador;
    private readonly IContextoUsuario _contextoUsuario;
    private readonly IRelojSistema _reloj;
    private readonly IServicioAuditoria _auditoria;
    private readonly IServicioKardex _kardex;
    private readonly IUnitOfWork _unitOfWork;

    public ServicioCaja(
        ISesionCajaRepository sesionRepository,
        IRepository<Denominacion> denominacionRepository,
        ICuentaRepository cuentaRepository,
        IVentaRepository ventaRepository,
        INotificadorComandas notificador,
        IContextoUsuario contextoUsuario,
        IRelojSistema reloj,
        IServicioAuditoria auditoria,
        IServicioKardex kardex,
        IUnitOfWork unitOfWork)
    {
        _sesionRepository = sesionRepository;
        _denominacionRepository = denominacionRepository;
        _cuentaRepository = cuentaRepository;
        _ventaRepository = ventaRepository;
        _notificador = notificador;
        _contextoUsuario = contextoUsuario;
        _reloj = reloj;
        _auditoria = auditoria;
        _kardex = kardex;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<DenominacionDto>> ObtenerDenominacionesAsync(CancellationToken cancellationToken = default)
    {
        var denominaciones = await _denominacionRepository.GetAllAsync(cancellationToken);
        return denominaciones
            .Where(d => !d.IsDeleted && d.Activo)
            .OrderBy(d => d.Moneda).ThenByDescending(d => d.Valor)
            .Select(d => new DenominacionDto(d.Id, d.Moneda, d.Tipo, d.Valor))
            .ToList();
    }

    public async Task<SesionCajaDto> AbrirSesionAsync(AbrirCajaDto dto, CancellationToken cancellationToken = default)
    {
        var abierta = await _sesionRepository.ObtenerAbiertaAsync(cancellationToken);
        if (abierta is not null)
        {
            throw new ConflictoException("Ya existe una sesión de caja abierta.");
        }

        if (dto.FondoInicial < 0)
        {
            throw new ReglaNegocioException("El fondo inicial no puede ser negativo.");
        }

        var usuarioId = _contextoUsuario.UsuarioId
            ?? throw new ReglaNegocioException("No se pudo identificar al usuario que abre la caja.");

        var sesion = new SesionCaja
        {
            UsuarioId = usuarioId,
            FondoInicial = dto.FondoInicial,
            AbiertaEn = _reloj.UtcNow
        };

        await _sesionRepository.AgregarAsync(sesion, cancellationToken);
        _auditoria.Registrar("abrir", "sesion-caja", sesion.Id, new { sesion.FondoInicial });
        await _sesionRepository.SaveChangesAsync(cancellationToken);

        await _notificador.TurnoAbiertoAsync(sesion.Id, cancellationToken);

        return await MapearAsync(sesion, cancellationToken);
    }

    public async Task<SesionCajaDto?> ObtenerSesionActivaAsync(CancellationToken cancellationToken = default)
    {
        var sesion = await _sesionRepository.ObtenerAbiertaAsync(cancellationToken);
        return sesion is null ? null : await MapearAsync(sesion, cancellationToken);
    }

    public async Task<TurnoAbiertoDto> ObtenerEstadoTurnoAsync(CancellationToken cancellationToken = default)
    {
        var sesion = await _sesionRepository.ObtenerAbiertaAsync(cancellationToken);
        return new TurnoAbiertoDto(sesion is not null, sesion?.AbiertaEn);
    }

    public async Task<ResultadoPaginado<SesionCajaDto>> ObtenerSesionesAsync(
        PaginacionRequest paginacion,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _sesionRepository.ObtenerPaginadoAsync(paginacion, cancellationToken);
        var items = new List<SesionCajaDto>();
        foreach (var sesion in pagina.Items)
        {
            items.Add(await MapearAsync(sesion, cancellationToken));
        }
        return ResultadoPaginado<SesionCajaDto>.Crear(items, pagina.Page, pagina.PageSize, pagina.TotalItems);
    }

    public async Task<SesionCajaDto?> RegistrarMovimientoAsync(
        Guid sesionId,
        MovimientoCajaCrearDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.Monto <= 0)
        {
            throw new ReglaNegocioException("El monto del movimiento debe ser mayor que cero.");
        }

        var registrado = false;

        // El bloqueo compartido del turno hace que un movimiento en curso termine antes de un cierre
        // y que, cerrado el turno, ya no se acepte.
        await _unitOfWork.EjecutarEnTransaccionAsync(async ct =>
        {
            registrado = false;

            var abierta = await _sesionRepository.ObtenerAbiertaBloqueadaAsync(ct);
            if (abierta is null || abierta.Id != sesionId)
            {
                return;
            }

            await _sesionRepository.AgregarMovimientoAsync(new MovimientoCaja
            {
                SesionCajaId = sesionId,
                Tipo = dto.Tipo,
                Monto = dto.Monto,
                Moneda = dto.Moneda,
                Motivo = dto.Motivo
            }, ct);
            registrado = true;
        }, cancellationToken);

        if (!registrado)
        {
            return null;
        }

        var actualizada = await _sesionRepository.ObtenerConDetalleAsync(sesionId, cancellationToken);
        return actualizada is null ? null : await MapearAsync(actualizada, cancellationToken);
    }

    public async Task<SesionCajaDto?> CerrarSesionAsync(
        Guid sesionId,
        CerrarCajaDto dto,
        CancellationToken cancellationToken = default)
    {
        var cerrada = false;
        decimal esperado = 0, contado = 0;
        var mesasDesalojadas = new List<Guid>();
        SesionCaja? sesionCerrada = null;

        await _unitOfWork.EjecutarEnTransaccionAsync(async ct =>
        {
            cerrada = false;
            esperado = 0;
            contado = 0;
            mesasDesalojadas = new List<Guid>();

            // Bloqueo exclusivo: espera a las ventas y movimientos en curso y serializa cierres
            // simultáneos del mismo turno. Todo lo que sigue se lee ya con el turno estable.
            await _sesionRepository.BloquearParaCierreAsync(sesionId, ct);

            var sesion = await _sesionRepository.ObtenerConDetalleAsync(sesionId, ct);
            if (sesion is null || sesion.IsDeleted)
            {
                return;
            }

            if (sesion.Estado != EstadoSesionCaja.Abierta)
            {
                throw new ConflictoException("El turno ya fue cerrado.");
            }

            var totalIngresos = sesion.Movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Ingreso).Sum(m => m.Monto);
            var totalEgresos = sesion.Movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Egreso).Sum(m => m.Monto);
            esperado = sesion.FondoInicial + totalIngresos - totalEgresos;

            foreach (var linea in dto.Arqueo)
            {
                var denominacion = await _denominacionRepository.GetByIdAsync(linea.DenominacionId, ct)
                    ?? throw new NoEncontradoException($"No existe la denominación {linea.DenominacionId}.");

                if (linea.Cantidad < 0)
                {
                    throw new ReglaNegocioException("La cantidad de una denominación no puede ser negativa.");
                }

                var subtotal = denominacion.Valor * linea.Cantidad;
                contado += subtotal;

                await _sesionRepository.AgregarArqueoAsync(new ArqueoDenominacion
                {
                    SesionCajaId = sesionId,
                    DenominacionId = denominacion.Id,
                    Cantidad = linea.Cantidad,
                    Subtotal = subtotal
                }, ct);
            }

            // Desaloja las mesas que quedaron abiertas: con saldo quedan por cobrar,
            // sin saldo se cierran, y todas las mesas vuelven a libres.
            var abiertas = await _cuentaRepository.ObtenerAbiertasConSesionAsync(ct);
            foreach (var cuenta in abiertas)
            {
                cuenta.SesionMesa.CerradaEn = _reloj.UtcNow;
                cuenta.Estado = cuenta.Saldo > 0m ? EstadoCuenta.PorCobrar : EstadoCuenta.Cerrada;

                if (cuenta.SesionMesa.MesaId is Guid mesaId)
                {
                    mesasDesalojadas.Add(mesaId);
                }
            }

            // Las comandas que quedaron sin servir ni cobrar liberan la existencia que tenían reservada.
            await _kardex.LiberarReservasAsync(
                abiertas.SelectMany(c => c.Comandas.SelectMany(cd => cd.Detalles)),
                ct);

            sesion.VentasDelTurnoUSD = await _ventaRepository.ObtenerTotalVentasDelTurnoAsync(sesionId, ct);
            sesion.CuentasDesalojadas = mesasDesalojadas.Count;

            sesion.Cerrar(esperado, contado, _reloj.UtcNow);
            _auditoria.Registrar("cerrar", "sesion-caja", sesion.Id, new { esperado, contado, descuadre = sesion.Descuadre, ventas = sesion.VentasDelTurnoUSD, mesas = sesion.CuentasDesalojadas });
            sesionCerrada = sesion;
            cerrada = true;
        }, cancellationToken);

        if (!cerrada || sesionCerrada is null)
        {
            return null;
        }

        foreach (var mesaId in mesasDesalojadas.Distinct())
        {
            await _notificador.MesaActualizadaAsync(mesaId, "Libre", cancellationToken: cancellationToken);
        }

        await _notificador.TurnoCerradoAsync(sesionId, cancellationToken);

        var resultado = await _sesionRepository.ObtenerConDetalleAsync(sesionId, cancellationToken);
        return resultado is null ? null : await MapearAsync(resultado, cancellationToken);
    }

    private async Task<SesionCajaDto> MapearAsync(SesionCaja s, CancellationToken cancellationToken)
    {
        var ventasDelTurno = await _ventaRepository.ObtenerTotalVentasDelTurnoAsync(s.Id, cancellationToken);

        return new SesionCajaDto(
            s.Id,
            s.Estado,
            s.FondoInicial,
            s.MontoEsperado,
            s.MontoContado,
            s.Descuadre,
            ventasDelTurno,
            s.CuentasDesalojadas,
            s.AbiertaEn,
            s.CerradaEn,
            s.Movimientos.Select(MapearMovimiento).ToList(),
            s.Arqueos.Select(MapearArqueo).ToList());
    }

    private static MovimientoCajaDto MapearMovimiento(MovimientoCaja m)
        => new(m.Id, m.Tipo, m.Monto, m.Moneda, m.Motivo, m.CreatedAt);

    private static ArqueoLineaDto MapearArqueo(ArqueoDenominacion a)
        => new(a.DenominacionId, a.Denominacion?.Moneda ?? Moneda.USD, a.Denominacion?.Valor ?? 0m, a.Cantidad, a.Subtotal);
}
