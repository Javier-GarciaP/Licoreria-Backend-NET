using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Services;

public sealed class ServicioCompras : IServicioCompras
{
    private readonly ICompraRepository _compras;
    private readonly IRepository<Proveedor> _proveedores;
    private readonly IRepository<ProductoVariante> _variantes;

    public ServicioCompras(
        ICompraRepository compras,
        IRepository<Proveedor> proveedores,
        IRepository<ProductoVariante> variantes)
    {
        _compras = compras;
        _proveedores = proveedores;
        _variantes = variantes;
    }

    // ================= Proveedores =================

    public async Task<IReadOnlyList<ProveedorDto>> ObtenerProveedoresAsync(string? busqueda = null, CancellationToken cancellationToken = default)
    {
        var proveedores = await _proveedores.GetAllAsync(cancellationToken);
        var consulta = proveedores.Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var filtro = busqueda.Trim().ToLower();
            consulta = consulta.Where(p =>
                p.Nombre.ToLower().Contains(filtro) ||
                (p.Rif != null && p.Rif.ToLower().Contains(filtro)));
        }

        return consulta.OrderBy(p => p.Nombre).Select(MapearProveedor).ToList();
    }

    public async Task<ProveedorDto?> ObtenerProveedorAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var proveedor = await _proveedores.GetByIdAsync(id, cancellationToken);
        return proveedor is null || proveedor.IsDeleted ? null : MapearProveedor(proveedor);
    }

    public async Task<ProveedorDto> CrearProveedorAsync(ProveedorCrearDto dto, CancellationToken cancellationToken = default)
    {
        var proveedor = new Proveedor
        {
            Nombre = dto.Nombre,
            Rif = dto.Rif,
            Contacto = dto.Contacto,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Direccion = dto.Direccion,
            DiasCredito = dto.DiasCredito,
            Activo = true
        };

        await _proveedores.AddAsync(proveedor, cancellationToken);
        await _proveedores.SaveChangesAsync(cancellationToken);
        return MapearProveedor(proveedor);
    }

    public async Task<ProveedorDto?> EditarProveedorAsync(ProveedorEditarDto dto, CancellationToken cancellationToken = default)
    {
        var proveedor = await _proveedores.GetByIdAsync(dto.Id, cancellationToken);
        if (proveedor is null || proveedor.IsDeleted)
        {
            return null;
        }

        proveedor.Nombre = dto.Nombre;
        proveedor.Rif = dto.Rif;
        proveedor.Contacto = dto.Contacto;
        proveedor.Telefono = dto.Telefono;
        proveedor.Email = dto.Email;
        proveedor.Direccion = dto.Direccion;
        proveedor.DiasCredito = dto.DiasCredito;
        proveedor.Activo = dto.Activo;
        _proveedores.Update(proveedor);
        await _proveedores.SaveChangesAsync(cancellationToken);
        return MapearProveedor(proveedor);
    }

    public async Task<bool> EliminarProveedorAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var proveedor = await _proveedores.GetByIdAsync(id, cancellationToken);
        if (proveedor is null || proveedor.IsDeleted)
        {
            return false;
        }

        proveedor.EliminarLogico();
        _proveedores.Update(proveedor);
        await _proveedores.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ================= Órdenes de compra =================

    public async Task<ResultadoPaginado<OrdenCompraDto>> ObtenerOrdenesAsync(
        PaginacionRequest paginacion,
        EstadoOrdenCompra? estado = null,
        Guid? proveedorId = null,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _compras.ObtenerOrdenesPaginadoAsync(paginacion, estado, proveedorId, cancellationToken);
        var items = pagina.Items.Select(MapearOrden).ToList();
        return ResultadoPaginado<OrdenCompraDto>.Crear(items, pagina.Page, pagina.PageSize, pagina.TotalItems);
    }

    public async Task<OrdenCompraDto?> ObtenerOrdenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var orden = await _compras.ObtenerOrdenConDetalleAsync(id, cancellationToken);
        return orden is null ? null : MapearOrden(orden);
    }

    public async Task<OrdenCompraDto> CrearOrdenAsync(OrdenCompraCrearDto dto, CancellationToken cancellationToken = default)
    {
        var proveedor = await _proveedores.GetByIdAsync(dto.ProveedorId, cancellationToken)
            ?? throw new NoEncontradoException($"No existe el proveedor {dto.ProveedorId}.");

        if (dto.Detalles.Count == 0)
        {
            throw new ReglaNegocioException("La orden debe tener al menos una línea.");
        }

        var correlativo = await _compras.ContarOrdenesAsync(cancellationToken) + 1;
        var orden = new OrdenCompra
        {
            Numero = $"OC-{correlativo:00000000}",
            ProveedorId = proveedor.Id,
            Observaciones = dto.Observaciones,
            Estado = EstadoOrdenCompra.Borrador
        };

        foreach (var linea in dto.Detalles)
        {
            if (linea.Cantidad <= 0)
            {
                throw new ReglaNegocioException("La cantidad de cada línea debe ser mayor que cero.");
            }

            if (linea.CostoUnitarioUSD < 0)
            {
                throw new ReglaNegocioException("El costo unitario no puede ser negativo.");
            }

            var variante = await _variantes.GetByIdAsync(linea.VarianteId, cancellationToken)
                ?? throw new NoEncontradoException($"No existe la variante {linea.VarianteId}.");

            orden.Detalles.Add(new OrdenCompraDetalle
            {
                VarianteId = variante.Id,
                Cantidad = linea.Cantidad,
                CostoUnitarioUSD = linea.CostoUnitarioUSD
            });
        }

        orden.RecalcularTotal();

        await _compras.AgregarOrdenAsync(orden, cancellationToken);
        await _compras.SaveChangesAsync(cancellationToken);

        var creada = await _compras.ObtenerOrdenConDetalleAsync(orden.Id, cancellationToken);
        return MapearOrden(creada!);
    }

    public async Task<OrdenCompraDto?> AprobarOrdenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var orden = await _compras.ObtenerOrdenConDetalleAsync(id, cancellationToken);
        if (orden is null || orden.IsDeleted)
        {
            return null;
        }

        if (orden.Estado != EstadoOrdenCompra.Borrador)
        {
            throw new ReglaNegocioException("Solo se pueden aprobar órdenes en estado Borrador.");
        }

        orden.Aprobar();
        await _compras.SaveChangesAsync(cancellationToken);
        return MapearOrden(orden);
    }

    public async Task<OrdenCompraDto?> EnviarOrdenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var orden = await _compras.ObtenerOrdenConDetalleAsync(id, cancellationToken);
        if (orden is null || orden.IsDeleted)
        {
            return null;
        }

        if (orden.Estado != EstadoOrdenCompra.Aprobada)
        {
            throw new ReglaNegocioException("Solo se pueden enviar órdenes aprobadas.");
        }

        orden.Enviar();
        await _compras.SaveChangesAsync(cancellationToken);
        return MapearOrden(orden);
    }

    public async Task<OrdenCompraDto?> CancelarOrdenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var orden = await _compras.ObtenerOrdenConDetalleAsync(id, cancellationToken);
        if (orden is null || orden.IsDeleted)
        {
            return null;
        }

        if (orden.Estado is EstadoOrdenCompra.Recibida or EstadoOrdenCompra.RecibidaParcial)
        {
            throw new ReglaNegocioException("No se puede cancelar una orden ya recibida.");
        }

        orden.Cancelar();
        await _compras.SaveChangesAsync(cancellationToken);
        return MapearOrden(orden);
    }

    private static ProveedorDto MapearProveedor(Proveedor p)
        => new(p.Id, p.Nombre, p.Rif, p.Contacto, p.Telefono, p.Email, p.Direccion, p.DiasCredito, p.Activo);

    private static OrdenCompraDto MapearOrden(OrdenCompra o)
        => new(
            o.Id,
            o.Numero,
            o.ProveedorId,
            o.Proveedor?.Nombre ?? string.Empty,
            o.Fecha,
            o.Estado,
            o.Observaciones,
            o.TotalUSD,
            o.Detalles.Select(d => new OrdenCompraDetalleDto(
                d.Id,
                d.VarianteId,
                d.Variante?.Sku ?? string.Empty,
                d.Variante?.Producto?.Nombre ?? d.Variante?.Nombre ?? string.Empty,
                d.Cantidad,
                d.CostoUnitarioUSD,
                d.CantidadRecibida,
                d.SubtotalUSD)).ToList());
}
