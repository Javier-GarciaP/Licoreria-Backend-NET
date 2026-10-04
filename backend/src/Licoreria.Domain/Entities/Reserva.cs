using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>Reserva de una fecha y hora para un grupo de personas.</summary>
public class Reserva : BaseEntity
{
    public DateTime FechaHora { get; set; }
    public int Personas { get; set; }
    public EstadoReserva Estado { get; set; } = EstadoReserva.Pendiente;
    public OrigenReserva Origen { get; set; } = OrigenReserva.Web;
    public string NombreContacto { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Notas { get; set; }

    public ICollection<ReservaMesa> Mesas { get; set; } = new List<ReservaMesa>();
    public ICollection<ReservaPago> Pagos { get; set; } = new List<ReservaPago>();
    public ICollection<PedidoAnticipado> Pedidos { get; set; } = new List<PedidoAnticipado>();

    public void Confirmar() => Estado = EstadoReserva.Confirmada;
    public void Cancelar() => Estado = EstadoReserva.Cancelada;
    public void MarcarAsistencia() => Estado = EstadoReserva.Asistio;
}

/// <summary>Mesa incluida en una reserva.</summary>
public class ReservaMesa : BaseEntity
{
    public Guid ReservaId { get; set; }
    public Reserva Reserva { get; set; } = null!;

    public Guid MesaId { get; set; }
    public Mesa Mesa { get; set; } = null!;
}

/// <summary>Seña de una reserva con comprobante y validación manual.</summary>
public class ReservaPago : BaseEntity
{
    public Guid ReservaId { get; set; }
    public Reserva Reserva { get; set; } = null!;

    public Guid MetodoPagoId { get; set; }
    public MetodoPago MetodoPago { get; set; } = null!;

    public decimal Monto { get; set; }
    public Moneda Moneda { get; set; } = Moneda.USD;
    public string? ComprobanteUrl { get; set; }
    public EstadoReservaPago Estado { get; set; } = EstadoReservaPago.Pendiente;

    public void Validar() => Estado = EstadoReservaPago.Validado;
    public void Rechazar() => Estado = EstadoReservaPago.Rechazado;
}

/// <summary>Evento del local publicable en la web.</summary>
public class Evento : BaseEntity
{
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Publicado { get; set; }
    public bool Activo { get; set; } = true;
}
