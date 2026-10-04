using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>Cliente en la lista VIP del local.</summary>
public class ListaVip : BaseEntity
{
    public Guid? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string? Documento { get; set; }
    public string? Telefono { get; set; }
    public string? Notas { get; set; }
    public bool Activo { get; set; } = true;
}

/// <summary>Entrada de acceso (cover) con código para control por QR.</summary>
public class Entrada : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;

    public Guid? EventoId { get; set; }
    public Evento? Evento { get; set; }

    public Guid? ReservaId { get; set; }
    public Reserva? Reserva { get; set; }

    public Guid? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public decimal Precio { get; set; }
    public Moneda Moneda { get; set; } = Moneda.USD;
    public EstadoEntrada Estado { get; set; } = EstadoEntrada.Valida;
    public DateTime EmitidaEn { get; set; } = DateTime.UtcNow;
    public DateTime? UsadaEn { get; private set; }

    public void Validar(DateTime ahora)
    {
        if (Estado != EstadoEntrada.Valida)
        {
            throw new InvalidOperationException("La entrada no es válida o ya fue usada.");
        }

        Estado = EstadoEntrada.Usada;
        UsadaEn = ahora;
        MarcarModificado();
    }

    public void Cancelar()
    {
        Estado = EstadoEntrada.Cancelada;
        MarcarModificado();
    }
}

/// <summary>Ítem pedido anticipadamente al reservar.</summary>
public class PedidoAnticipado : BaseEntity
{
    public Guid ReservaId { get; set; }
    public Reserva Reserva { get; set; } = null!;

    public Guid VarianteId { get; set; }
    public ProductoVariante Variante { get; set; } = null!;

    public decimal Cantidad { get; set; }
    public decimal PrecioUnitarioUSD { get; set; }
}
