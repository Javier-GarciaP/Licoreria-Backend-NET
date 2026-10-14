using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Services;

/// <summary>
/// Controla las transiciones válidas del estado de una línea de comanda
/// (<see cref="EstadoItemComanda"/>): Recibido → EnProceso → Preparado → Entregado.
/// Recibido → Preparado directo se permite para no romper el arrastre del KDS;
/// Entregado y Cancelado son estados terminales.
/// </summary>
public sealed class MaquinaEstadosComanda
{
    private static readonly IReadOnlyDictionary<EstadoItemComanda, EstadoItemComanda[]> Transiciones =
        new Dictionary<EstadoItemComanda, EstadoItemComanda[]>
        {
            [EstadoItemComanda.Recibido] =
            [
                EstadoItemComanda.EnProceso,
                EstadoItemComanda.Preparado,
                EstadoItemComanda.Cancelado
            ],
            [EstadoItemComanda.EnProceso] =
            [
                EstadoItemComanda.Preparado,
                EstadoItemComanda.Cancelado
            ],
            [EstadoItemComanda.Preparado] =
            [
                EstadoItemComanda.Entregado,
                EstadoItemComanda.Cancelado
            ],
            [EstadoItemComanda.Entregado] = [],
            [EstadoItemComanda.Cancelado] = []
        };

    public bool PuedeTransicionar(EstadoItemComanda actual, EstadoItemComanda nuevo)
        => Transiciones[actual].Contains(nuevo);

    public EstadoItemComanda Transicionar(EstadoItemComanda actual, EstadoItemComanda nuevo)
    {
        if (!PuedeTransicionar(actual, nuevo))
        {
            throw new InvalidOperationException(
                $"Transición de comanda no permitida: {actual} → {nuevo}.");
        }

        return nuevo;
    }
}
