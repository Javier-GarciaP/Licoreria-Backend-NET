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
