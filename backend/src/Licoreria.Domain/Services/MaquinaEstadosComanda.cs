namespace Licoreria.Domain.Services;

/// <summary>
/// Estados por los que transita un ítem de comanda.
/// </summary>
public enum EstadoComanda
{
    Recibido = 1,
    Preparado = 2,
    Entregado = 3,
    Cancelado = 4
}

/// <summary>
/// Controla las transiciones válidas del estado de una comanda.
/// </summary>
public sealed class MaquinaEstadosComanda
{
    private static readonly IReadOnlyDictionary<EstadoComanda, EstadoComanda[]> Transiciones =
        new Dictionary<EstadoComanda, EstadoComanda[]>
        {
            [EstadoComanda.Recibido] = [EstadoComanda.Preparado, EstadoComanda.Cancelado],
            [EstadoComanda.Preparado] = [EstadoComanda.Entregado, EstadoComanda.Cancelado],
            [EstadoComanda.Entregado] = [],
            [EstadoComanda.Cancelado] = []
        };

    public bool PuedeTransicionar(EstadoComanda actual, EstadoComanda nuevo)
        => Transiciones[actual].Contains(nuevo);

    public EstadoComanda Transicionar(EstadoComanda actual, EstadoComanda nuevo)
    {
        if (!PuedeTransicionar(actual, nuevo))
        {
            throw new InvalidOperationException(
                $"Transición de comanda no permitida: {actual} → {nuevo}.");
        }

        return nuevo;
    }
}
