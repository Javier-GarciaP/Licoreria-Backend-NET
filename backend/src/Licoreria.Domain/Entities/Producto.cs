using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Definición base de un producto del catálogo. Lo que se vende es una
/// <see cref="ProductoVariante"/> (presentación), que concentra SKU y precios.
/// </summary>
public class Producto : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? ImagenUrl { get; set; }
    public TipoProducto Tipo { get; set; } = TipoProducto.Simple;
    public AreaDestino AreaDestino { get; set; } = AreaDestino.Barra;
    public decimal? GradoAlcoholico { get; set; }
    public bool Activo { get; set; } = true;

    public Guid CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;

    public Guid? MarcaId { get; set; }
    public Marca? Marca { get; set; }

    public Guid? ImpuestoId { get; set; }
    public Impuesto? Impuesto { get; set; }

    public ICollection<ProductoVariante> Variantes { get; set; } = new List<ProductoVariante>();
    public ICollection<ProductoModificador> Modificadores { get; set; } = new List<ProductoModificador>();

    public void ActualizarDatos(
        string nombre,
        string? descripcion,
        Guid categoriaId,
        Guid? marcaId,
        Guid? impuestoId,
        TipoProducto tipo,
        AreaDestino areaDestino,
        decimal? gradoAlcoholico,
        string? imagenUrl)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new InvalidOperationException("El nombre del producto es obligatorio.");
        }

        Nombre = nombre;
        Descripcion = descripcion;
        CategoriaId = categoriaId;
        MarcaId = marcaId;
        ImpuestoId = impuestoId;
        Tipo = tipo;
        AreaDestino = areaDestino;
        GradoAlcoholico = gradoAlcoholico;
        ImagenUrl = imagenUrl;
        MarcarModificado();
    }

    public void Activar()
    {
        Activo = true;
        MarcarModificado();
    }

    public void Desactivar()
    {
        Activo = false;
        MarcarModificado();
    }
}
