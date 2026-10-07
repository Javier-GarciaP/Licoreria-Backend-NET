using Licoreria.Domain.Common;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Modificador o extra aplicable a un producto (por ejemplo "doble hielo", "extra limón").
/// </summary>
public class Modificador : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public decimal PrecioAdicional { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<ProductoModificador> Productos { get; set; } = new List<ProductoModificador>();

    public void Actualizar(string nombre, decimal precioAdicional, bool activo)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new InvalidOperationException("El nombre del modificador es obligatorio.");
        }

        if (precioAdicional < 0)
        {
            throw new InvalidOperationException("El precio adicional no puede ser negativo.");
        }

        Nombre = nombre;
        PrecioAdicional = precioAdicional;
        Activo = activo;
        MarcarModificado();
    }
}

/// <summary>
/// Asociación entre un producto y un modificador disponibles con sus límites de selección.
/// </summary>
public class ProductoModificador : BaseEntity
{
    public Guid ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    public Guid ModificadorId { get; set; }
    public Modificador Modificador { get; set; } = null!;

    public int Minimo { get; set; }
    public int Maximo { get; set; } = 1;
    public bool Requerido { get; set; }
}
