namespace Licoreria.Domain.Common;

/// <summary>
/// Indica que un recurso solicitado no existe (HTTP 404).
/// </summary>
public class NoEncontradoException : Exception
{
    public NoEncontradoException(string mensaje) : base(mensaje)
    {
    }
}

/// <summary>
/// Indica un conflicto con el estado actual del recurso, por ejemplo un duplicado (HTTP 409).
/// </summary>
public class ConflictoException : Exception
{
    public ConflictoException(string mensaje) : base(mensaje)
    {
    }
}

/// <summary>
/// Indica que una regla de negocio no se cumple (HTTP 422).
/// </summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje)
    {
    }
}

/// <summary>
/// Indica que no hay existencia disponible para atender lo solicitado (HTTP 422).
/// Permite al cliente distinguir un producto agotado de una carrera de concurrencia (409).
/// </summary>
public class ExistenciaAgotadaException : ReglaNegocioException
{
    public ExistenciaAgotadaException(string producto, decimal requerido, decimal disponible)
        : this($"Stock insuficiente para '{producto}': se requieren {requerido} y hay {disponible}.", producto, requerido, disponible)
    {
    }

    protected ExistenciaAgotadaException(string mensaje, string producto, decimal requerido, decimal disponible)
        : base(mensaje)
    {
        Producto = producto;
        Requerido = requerido;
        Disponible = disponible;
    }

    public string Producto { get; }

    public decimal Requerido { get; }

    public decimal Disponible { get; }
}

/// <summary>
/// Se pidió una porción (vaso, trago) y no hay envases abiertos que la cubran, pero sí presentaciones
/// cerradas que se pueden destapar: no está agotada, falta que la barra abra una botella (HTTP 422).
/// </summary>
public class PorcionesSinAbrirException : ExistenciaAgotadaException
{
    public PorcionesSinAbrirException(string producto, decimal requerido, decimal disponible, int envasesCerrados)
        : base(
            $"No hay {producto} abierto: se piden {requerido} y hay {disponible} porciones abiertas. " +
            $"Hay {envasesCerrados} envase(s) cerrado(s); pide a barra que destape una botella.",
            producto,
            requerido,
            disponible)
    {
        EnvasesCerrados = envasesCerrados;
    }

    public int EnvasesCerrados { get; }
}
