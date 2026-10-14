using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Casos de uso del club: zonas, mesas, planos, reservas y eventos.
/// </summary>
public interface IServicioClub
{
    Task<IReadOnlyList<ZonaDto>> ObtenerZonasAsync(CancellationToken cancellationToken = default);
    Task<ZonaDto> CrearZonaAsync(ZonaCrearDto dto, CancellationToken cancellationToken = default);
    Task<ZonaDto?> EditarZonaAsync(ZonaEditarDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarZonaAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MesaDto>> ObtenerMesasAsync(
        Guid? zonaId = null,
        DateTime? fechaHora = null,
        CancellationToken cancellationToken = default);
    Task<MesaDto> CrearMesaAsync(MesaCrearDto dto, CancellationToken cancellationToken = default);
    Task<MesaDto?> EditarMesaAsync(MesaEditarDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarMesaAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca la mesa como desalojada: cierra la sesión de mesa, deja la cuenta en
    /// <c>PorCobrar</c> si tiene saldo y notifica en tiempo real que la mesa quedó libre.
    /// </summary>
    Task<bool> DesalojarMesaAsync(Guid mesaId, CancellationToken cancellationToken = default);

    /// <summary>Lista los planos; las plantillas solo se incluyen si <paramref name="incluirPlantillas"/> es verdadero.</summary>
    Task<IReadOnlyList<PlanoDto>> ObtenerPlanosAsync(bool incluirPlantillas = false, CancellationToken cancellationToken = default);

    /// <summary>Obtiene un plano; una plantilla solo se devuelve si <paramref name="puedeVerPlantillas"/> es verdadero.</summary>
    Task<PlanoDto?> ObtenerPlanoAsync(Guid id, bool puedeVerPlantillas = false, CancellationToken cancellationToken = default);
    Task<PlanoDto> CrearPlanoAsync(PlanoCrearDto dto, CancellationToken cancellationToken = default);
    Task<PlanoDto?> EditarPlanoAsync(PlanoEditarDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarPlanoAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Asigna o reemplaza el croquis del plano (valida formato, tamaño y firma); null si no existe.</summary>
    Task<PlanoDto?> AsignarFondoPlanoAsync(
        Guid id,
        Stream contenido,
        string nombreArchivo,
        string? tipoContenido,
        long tamano,
        CancellationToken cancellationToken = default);

    Task<PlanoDto?> QuitarFondoPlanoAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Duplica un plano (inactivo, con su fondo copiado) o lo guarda/usa como plantilla; null si no existe.</summary>
    Task<PlanoDto?> DuplicarPlanoAsync(Guid id, PlanoDuplicarDto dto, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<ReservaDto>> ObtenerReservasAsync(
        PaginacionRequest paginacion,
        DateTime? desde = null,
        DateTime? hasta = null,
        EstadoReserva? estado = null,
        CancellationToken cancellationToken = default);

    Task<ReservaDto?> ObtenerReservaAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ReservaDto> CrearReservaAsync(ReservaCrearDto dto, CancellationToken cancellationToken = default);
    Task<ReservaDto?> RegistrarPagoReservaAsync(Guid reservaId, ReservaPagoCrearDto dto, CancellationToken cancellationToken = default);
    Task<ReservaDto?> ValidarPagoReservaAsync(Guid reservaId, Guid pagoId, ValidarReservaPagoDto dto, CancellationToken cancellationToken = default);
    Task<ReservaDto?> CambiarEstadoReservaAsync(Guid reservaId, CambiarEstadoReservaDto dto, CancellationToken cancellationToken = default);

    /// <summary>Marca una reserva como atendida (Finalizada) cuando el mesonero abre su mesa.</summary>
    Task<ReservaDto?> MarcarReservaAtendidaAsync(Guid reservaId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventoDto>> ObtenerEventosAsync(bool soloPublicados = false, CancellationToken cancellationToken = default);
    Task<EventoDto> CrearEventoAsync(EventoCrearDto dto, CancellationToken cancellationToken = default);
    Task<EventoDto?> EditarEventoAsync(EventoEditarDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarEventoAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ListaVipDto>> ObtenerListaVipAsync(CancellationToken cancellationToken = default);
    Task<ListaVipDto> CrearVipAsync(ListaVipCrearDto dto, CancellationToken cancellationToken = default);
    Task<ListaVipDto?> EditarVipAsync(ListaVipEditarDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarVipAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ResultadoPaginado<EntradaDto>> ObtenerEntradasAsync(
        PaginacionRequest paginacion,
        Guid? eventoId = null,
        EstadoEntrada? estado = null,
        CancellationToken cancellationToken = default);

    Task<EntradaDto> EmitirEntradaAsync(EmitirEntradaDto dto, CancellationToken cancellationToken = default);
    Task<EntradaDto?> ValidarEntradaAsync(string codigo, CancellationToken cancellationToken = default);
    Task<bool> CancelarEntradaAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PedidoAnticipadoDto>> ObtenerPedidosAsync(Guid reservaId, CancellationToken cancellationToken = default);
    Task<PedidoAnticipadoDto> AgregarPedidoAsync(Guid reservaId, PedidoAnticipadoCrearDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarPedidoAsync(Guid reservaId, Guid pedidoId, CancellationToken cancellationToken = default);
}
