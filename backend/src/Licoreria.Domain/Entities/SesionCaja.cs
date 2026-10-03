using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Sesión de caja por turno: apertura, movimientos y cierre con arqueo.
/// </summary>
public class SesionCaja : BaseEntity
{
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public EstadoSesionCaja Estado { get; set; } = EstadoSesionCaja.Abierta;
    public decimal FondoInicial { get; set; }
    public decimal MontoEsperado { get; set; }
    public decimal MontoContado { get; set; }
    public decimal Descuadre { get; set; }
    public DateTime AbiertaEn { get; set; } = DateTime.UtcNow;
    public DateTime? CerradaEn { get; set; }

    public ICollection<MovimientoCaja> Movimientos { get; set; } = new List<MovimientoCaja>();
    public ICollection<ArqueoDenominacion> Arqueos { get; set; } = new List<ArqueoDenominacion>();

    public void Cerrar(decimal montoEsperado, decimal montoContado, DateTime cerradaEn)
    {
        MontoEsperado = montoEsperado;
        MontoContado = montoContado;
        Descuadre = montoContado - montoEsperado;
        CerradaEn = cerradaEn;
        Estado = EstadoSesionCaja.Cerrada;
        MarcarModificado();
    }
}

/// <summary>
/// Movimiento de ingreso o egreso de caja.
/// </summary>
public class MovimientoCaja : BaseEntity
{
    public Guid SesionCajaId { get; set; }
    public SesionCaja SesionCaja { get; set; } = null!;

    public TipoMovimientoCaja Tipo { get; set; }
    public decimal Monto { get; set; }
    public Moneda Moneda { get; set; } = Moneda.USD;
    public string Motivo { get; set; } = string.Empty;
}

/// <summary>
/// Conteo de una denominación durante el arqueo de cierre.
/// </summary>
public class ArqueoDenominacion : BaseEntity
{
    public Guid SesionCajaId { get; set; }
    public SesionCaja SesionCaja { get; set; } = null!;

    public Guid DenominacionId { get; set; }
    public Denominacion Denominacion { get; set; } = null!;

    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
}
