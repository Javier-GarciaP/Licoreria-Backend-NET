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

    private ServicioClub CrearServicio()
    {
        _reloj.SetupGet(r => r.UtcNow).Returns(Ahora);
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
            Mock.Of<INotificadorComandas>());
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

        // La ventana cubre el intervalo presunto (2h) alrededor de la fecha solicitada.
        _reservas.Verify(
            r => r.ObtenerMesasReservadasAsync(
                fecha.AddHours(-2),
                fecha.AddHours(2),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ObtenerMesasAsync_SinFechaHora_MantieneVentanaFijaDeOperacion()
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

        _reservas.Verify(
            r => r.ObtenerMesasReservadasAsync(
                Ahora.AddHours(-2),
                Ahora.AddHours(12),
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
}
