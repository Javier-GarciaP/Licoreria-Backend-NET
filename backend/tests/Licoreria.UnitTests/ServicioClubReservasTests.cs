using System.Linq.Expressions;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Domain.Services;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>
/// Pruebas deterministas de la disponibilidad por fecha y del rechazo de doble reserva
/// en <see cref="ServicioClub"/>.
/// </summary>
public sealed class ServicioClubReservasTests
{
    private static readonly Guid ZonaId = Guid.NewGuid();
    private static readonly DateTime Ahora = new(2026, 10, 8, 12, 0, 0);

    private readonly Mock<IRepository<Mesa>> _mesas = new();
    private readonly Mock<IRepository<Zona>> _zonas = new();
    private readonly Mock<ICuentaRepository> _cuentas = new();
    private readonly Mock<IReservaRepository> _reservas = new();
    private readonly Mock<IRelojSistema> _reloj = new();
    private readonly Mock<IServicioKardex> _kardex = new();
    private readonly Mock<INotificadorComandas> _notificador = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private ServicioClub CrearServicio()
    {
        _reloj.SetupGet(r => r.UtcNow).Returns(Ahora);
        _unitOfWork
            .Setup(u => u.EjecutarEnTransaccionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> operacion, CancellationToken ct) => operacion(ct));
        _zonas
            .Setup(z => z.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _cuentas
            .Setup(c => c.ObtenerCuentasAbiertasPorMesaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, Guid>());
        return new ServicioClub(
            _zonas.Object,
            _mesas.Object,
            Mock.Of<IRepository<Plano>>(),
            Mock.Of<IRepository<PlanoElemento>>(),
            Mock.Of<IRepository<Evento>>(),
            Mock.Of<IRepository<ListaVip>>(),
            Mock.Of<IRepository<Entrada>>(),
            Mock.Of<IRepository<PedidoAnticipado>>(),
            Mock.Of<IRepository<ProductoVariante>>(),
            _reservas.Object,
            _cuentas.Object,
            new DetectorConflictosReserva(),
            _reloj.Object,
            _notificador.Object,
            _kardex.Object,
            _unitOfWork.Object,
            new JornadaOperativa());
    }

    private static Mesa Mesa() => new() { ZonaId = ZonaId, Numero = "M1", Capacidad = 4, Activa = true };

    private static CancellationToken Token() => CancellationToken.None;

    // ================= Disponibilidad por fecha =================

    [Fact]
    public async Task ObtenerMesasAsync_ConFechaHora_UsaVentanaAlrededorDeLaFecha()
    {
        var mesa = Mesa();
        _mesas
            .Setup(m => m.FindAsync(It.IsAny<Expression<Func<Mesa, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([mesa]);
        _reservas
            .Setup(r => r.ObtenerMesasReservadasAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var servicio = CrearServicio();
        var fecha = new DateTime(2026, 10, 10, 20, 0, 0);

        await servicio.ObtenerMesasAsync(null, fecha, Token());

        // La ventana cubre el intervalo presunto (2h) alrededor de la fecha solicitada, sin los extremos:
        // dos reservas de 2 h que se tocan en el borde no se solapan.
        _reservas.Verify(
            r => r.ObtenerMesasReservadasAsync(
                fecha.AddHours(-2).AddMicroseconds(1),
                fecha.AddHours(2).AddMicroseconds(-1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ObtenerMesasAsync_SinFechaHora_UsaLaJornadaEnCurso()
    {
        var mesa = Mesa();
        _mesas
            .Setup(m => m.FindAsync(It.IsAny<Expression<Func<Mesa, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([mesa]);
        _reservas
            .Setup(r => r.ObtenerMesasReservadasAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var servicio = CrearServicio();

        await servicio.ObtenerMesasAsync(null, null, Token());

        // Ahora = 08:00 hora local (UTC-4): la jornada en curso es la que empieza hoy a las 12:00 local
        // (16:00 UTC) y termina mañana a las 06:00 local (10:00 UTC). Marca las reservas de toda la noche.
        _reservas.Verify(
            r => r.ObtenerMesasReservadasAsync(
                new DateTime(2026, 10, 8, 16, 0, 0),
                new DateTime(2026, 10, 9, 10, 0, 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ================= Doble reserva =================

    [Fact]
    public async Task CrearReservaAsync_ConMesaYaReservadaEnHorario_LanzaConflicto()
    {
        var mesa = Mesa();
        _mesas
            .Setup(m => m.GetByIdAsync(mesa.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);
        var fecha = Ahora.AddHours(2);
        var existente = IntervaloReserva.Presunto(mesa.Id, fecha.AddHours(-1), DetectorConflictosReserva.DuracionReservaPresuntaHoras);
        _reservas
            .Setup(r => r.ObtenerIntervalosActivosAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([existente]);

        var servicio = CrearServicio();

        var dto = new ReservaCrearDto(fecha, 2, [mesa.Id], "Cliente", "0414-0000000");

        await Assert.ThrowsAsync<ConflictoException>(() => servicio.CrearReservaAsync(dto, Token()));
        _reservas.Verify(r => r.AgregarAsync(It.IsAny<Reserva>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CrearReservaAsync_SinSolapamiento_CreaLaReserva()
    {
        var mesa = Mesa();
        _mesas
            .Setup(m => m.GetByIdAsync(mesa.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);
        _reservas
            .Setup(r => r.ObtenerIntervalosActivosAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _reservas
            .Setup(r => r.ObtenerConDetalleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new Reserva());
        _reservas.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var servicio = CrearServicio();
        var dto = new ReservaCrearDto(Ahora.AddHours(2), 2, [mesa.Id], "Cliente", "0414-0000000");

        var creada = await servicio.CrearReservaAsync(dto, Token());

        Assert.NotNull(creada);
        _reservas.Verify(r => r.AgregarAsync(It.IsAny<Reserva>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CrearReservaAsync_BloqueaLasMesasAntesDeComprobarSolapamientos()
    {
        var mesaA = Mesa();
        var mesaB = Mesa();
        _mesas.Setup(m => m.GetByIdAsync(mesaA.Id, It.IsAny<CancellationToken>())).ReturnsAsync(mesaA);
        _mesas.Setup(m => m.GetByIdAsync(mesaB.Id, It.IsAny<CancellationToken>())).ReturnsAsync(mesaB);
        var orden = new List<string>();
        IEnumerable<Guid>? bloqueadas = null;
        _reservas
            .Setup(r => r.BloquearMesasAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Guid>, CancellationToken>((ids, _) => { orden.Add("bloqueo"); bloqueadas = ids.ToList(); })
            .Returns(Task.CompletedTask);
        _reservas
            .Setup(r => r.ObtenerIntervalosActivosAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback(() => orden.Add("comprobacion"))
            .ReturnsAsync([]);
        _reservas
            .Setup(r => r.ObtenerConDetalleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new Reserva());

        await CrearServicio().CrearReservaAsync(
            new ReservaCrearDto(Ahora.AddHours(2), 2, [mesaA.Id, mesaB.Id, mesaA.Id], "Cliente", "0414-0000000"), Token());

        // El bloqueo va primero: quien llega después espera y ve la reserva ya confirmada.
        Assert.Equal(["bloqueo", "comprobacion"], orden);
        Assert.Equal(new[] { mesaA.Id, mesaB.Id }.OrderBy(i => i), bloqueadas!.OrderBy(i => i));
    }

    [Fact]
    public async Task CrearReservaAsync_CuandoHayConflicto_NoGuardaNada()
    {
        var mesa = Mesa();
        _mesas.Setup(m => m.GetByIdAsync(mesa.Id, It.IsAny<CancellationToken>())).ReturnsAsync(mesa);
        var fecha = Ahora.AddHours(2);
        _reservas
            .Setup(r => r.ObtenerIntervalosActivosAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([IntervaloReserva.Presunto(mesa.Id, fecha, DetectorConflictosReserva.DuracionReservaPresuntaHoras)]);

        await Assert.ThrowsAsync<ConflictoException>(() => CrearServicio().CrearReservaAsync(
            new ReservaCrearDto(fecha, 2, [mesa.Id], "Cliente", "0414-0000000"), Token()));

        _reservas.Verify(r => r.AgregarAsync(It.IsAny<Reserva>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ================= Desalojar mesa =================

    private static Cuenta CuentaConItemReservado(decimal total, out ComandaDetalle detalle)
    {
        var cuenta = new Cuenta { SesionMesa = new SesionMesa { NombreMesa = "Mesa 1", MesaId = Guid.NewGuid() } };
        cuenta.Acumular(total);
        var comanda = new Comanda { Area = AreaDestino.Barra };
        detalle = new ComandaDetalle { VarianteId = Guid.NewGuid(), Cantidad = 1m, Reservado = true };
        comanda.Detalles.Add(detalle);
        cuenta.Comandas.Add(comanda);
        return cuenta;
    }

    [Fact]
    public async Task DesalojarMesaAsync_LiberaLasReservasDeLoPedidoYNoServido()
    {
        var cuenta = CuentaConItemReservado(10m, out var detalle);
        var mesaId = cuenta.SesionMesa.MesaId!.Value;
        _cuentas
            .Setup(c => c.ObtenerCuentaAbiertaPorMesaAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cuenta);

        var desalojada = await CrearServicio().DesalojarMesaAsync(mesaId);

        Assert.True(desalojada);
        _kardex.Verify(
            k => k.LiberarReservasAsync(
                It.Is<IEnumerable<ComandaDetalle>>(d => d.Contains(detalle)), It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal(EstadoCuenta.PorCobrar, cuenta.Estado); // quedó saldo por cobrar
        Assert.NotNull(cuenta.SesionMesa.CerradaEn);
        _notificador.Verify(
            n => n.MesaActualizadaAsync(mesaId, "Libre", cuenta.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DesalojarMesaAsync_SinCuentaAbierta_RetornaFalseSinLiberarNada()
    {
        _cuentas
            .Setup(c => c.ObtenerCuentaAbiertaPorMesaAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cuenta?)null);

        var desalojada = await CrearServicio().DesalojarMesaAsync(Guid.NewGuid());

        Assert.False(desalojada);
        _kardex.Verify(
            k => k.LiberarReservasAsync(It.IsAny<IEnumerable<ComandaDetalle>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
