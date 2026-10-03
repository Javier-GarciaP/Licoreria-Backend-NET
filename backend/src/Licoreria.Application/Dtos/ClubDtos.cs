using Licoreria.Domain.Enums;

namespace Licoreria.Application.Dtos;

public sealed record ZonaDto(Guid Id, string Nombre, TipoZona Tipo, bool Activo);

public sealed record ZonaCrearDto(string Nombre, TipoZona Tipo, bool Activo = true);

public sealed record ZonaEditarDto(Guid Id, string Nombre, TipoZona Tipo, bool Activo);

public sealed record MesaDto(
    Guid Id,
    Guid ZonaId,
    string ZonaNombre,
    string Numero,
    int Capacidad,
    string Forma,
    decimal PosX,
    decimal PosY,
    decimal Ancho,
    decimal Alto,
    bool Activa,
    bool Disponible);

public sealed record MesaCrearDto(
    Guid ZonaId,
    string Numero,
    int Capacidad,
    string Forma = "redonda",
    decimal PosX = 0,
    decimal PosY = 0,
    decimal Ancho = 1,
    decimal Alto = 1);

public sealed record MesaEditarDto(
    Guid Id,
    Guid ZonaId,
    string Numero,
    int Capacidad,
    string Forma,
    decimal PosX,
    decimal PosY,
    decimal Ancho,
    decimal Alto,
    bool Activa);

public sealed record PlanoElementoDto(
    Guid Id,
    Guid? ZonaId,
    string Tipo,
    string? Etiqueta,
    decimal PosX,
    decimal PosY,
    decimal Ancho,
    decimal Alto,
    decimal Rotacion);

public sealed record PlanoElementoCrearDto(
    Guid? ZonaId,
    string Tipo,
    string? Etiqueta,
    decimal PosX,
    decimal PosY,
    decimal Ancho,
    decimal Alto,
    decimal Rotacion = 0);

public sealed record PlanoDto(
    Guid Id,
    string Nombre,
    int Version,
    bool Activo,
    IReadOnlyList<PlanoElementoDto> Elementos);

public sealed record PlanoCrearDto(
    string Nombre,
    IReadOnlyList<PlanoElementoCrearDto> Elementos);

public sealed record PlanoEditarDto(
    Guid Id,
    string Nombre,
    bool Activo,
    IReadOnlyList<PlanoElementoCrearDto> Elementos);

public sealed record ReservaMesaResumenDto(Guid MesaId, string Numero, string Zona);

public sealed record ReservaPagoDto(
    Guid Id,
    Guid MetodoPagoId,
    string MetodoPago,
    decimal Monto,
    Moneda Moneda,
    string? ComprobanteUrl,
    EstadoReservaPago Estado);

public sealed record ReservaDto(
    Guid Id,
    DateTime FechaHora,
    int Personas,
    EstadoReserva Estado,
    OrigenReserva Origen,
    string NombreContacto,
    string Telefono,
    string? Notas,
    IReadOnlyList<ReservaMesaResumenDto> Mesas,
    IReadOnlyList<ReservaPagoDto> Pagos);

public sealed record ReservaCrearDto(
    DateTime FechaHora,
    int Personas,
    IReadOnlyList<Guid> Mesas,
    string NombreContacto,
    string Telefono,
    OrigenReserva Origen = OrigenReserva.Web,
    string? Notas = null);

public sealed record ReservaPagoCrearDto(
    Guid MetodoPagoId,
    decimal Monto,
    Moneda Moneda = Moneda.USD,
    string? ComprobanteUrl = null);

public sealed record ValidarReservaPagoDto(bool Aprobar);

public sealed record CambiarEstadoReservaDto(EstadoReserva Estado);

public sealed record EventoDto(
    Guid Id,
    string Titulo,
    string Descripcion,
    DateTime FechaInicio,
    DateTime? FechaFin,
    string? ImagenUrl,
    bool Publicado,
    bool Activo);

public sealed record EventoCrearDto(
    string Titulo,
    string Descripcion,
    DateTime FechaInicio,
    DateTime? FechaFin,
    string? ImagenUrl,
    bool Publicado = false);

public sealed record EventoEditarDto(
    Guid Id,
    string Titulo,
    string Descripcion,
    DateTime FechaInicio,
    DateTime? FechaFin,
    string? ImagenUrl,
    bool Publicado,
    bool Activo);
