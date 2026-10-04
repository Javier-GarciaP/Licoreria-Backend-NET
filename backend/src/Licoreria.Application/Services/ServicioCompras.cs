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
    private readonly IServicioKardex _kardex;
    private readonly IContextoUsuario _contextoUsuario;
    private readonly IRelojSistema _reloj;
    private readonly IServicioAuditoria _auditoria;

    public ServicioCompras(
        ICompraRepository compras,
        IRepository<Proveedor> proveedores,
        IRepository<ProductoVariante> variantes,
        IServicioKardex kardex,
        IContextoUsuario contextoUsuario,
        IRelojSistema reloj,
        IServicioAuditoria auditoria)
    {
        _compras = compras;
        _proveedores = proveedores;
        _variantes = variantes;
        _kardex = kardex;
        _contextoUsuario = contextoUsuario;
        _reloj = reloj;
        _auditoria = auditoria;
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

    // ================= Recepciones =================

    public async Task<ResultadoPaginado<RecepcionDto>> ObtenerRecepcionesAsync(
        PaginacionRequest paginacion,
        Guid? ordenCompraId = null,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _compras.ObtenerRecepcionesPaginadoAsync(paginacion, ordenCompraId, cancellationToken);
        var items = pagina.Items.Select(MapearRecepcion).ToList();
        return ResultadoPaginado<RecepcionDto>.Crear(items, pagina.Page, pagina.PageSize, pagina.TotalItems);
    }

    public async Task<RecepcionDto?> ObtenerRecepcionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var recepcion = await _compras.ObtenerRecepcionConDetalleAsync(id, cancellationToken);
        return recepcion is null ? null : MapearRecepcion(recepcion);
    }

    public async Task<RecepcionDto> RegistrarRecepcionAsync(RegistrarRecepcionDto dto, CancellationToken cancellationToken = default)
    {
        var orden = await _compras.ObtenerOrdenConDetalleAsync(dto.OrdenCompraId, cancellationToken)
            ?? throw new NoEncontradoException($"No existe la orden {dto.OrdenCompraId}.");

        if (orden.Estado is EstadoOrdenCompra.Cancelada or EstadoOrdenCompra.Recibida or EstadoOrdenCompra.Borrador)
        {
            throw new ReglaNegocioException("La orden no está en un estado que permita recibir mercancía.");
        }

        if (dto.Detalles.Count == 0)
        {
            throw new ReglaNegocioException("La recepción debe tener al menos una línea.");
        }

        var usuarioId = _contextoUsuario.UsuarioId
            ?? throw new ReglaNegocioException("No se pudo identificar al usuario que recibe.");

        var recepcion = new Recepcion
        {
            OrdenCompraId = orden.Id,
            UsuarioId = usuarioId,
            Fecha = _reloj.UtcNow,
            Observaciones = dto.Observaciones
        };

        foreach (var linea in dto.Detalles)
        {
            var detalleOrden = orden.Detalles.FirstOrDefault(d => d.Id == linea.OrdenCompraDetalleId)
                ?? throw new NoEncontradoException($"La línea {linea.OrdenCompraDetalleId} no pertenece a la orden.");

            var costo = linea.CostoUnitarioUSD ?? detalleOrden.CostoUnitarioUSD;

            try
            {
                detalleOrden.Recibir(linea.Cantidad);
            }
            catch (InvalidOperationException ex)
            {
                throw new ReglaNegocioException(ex.Message);
            }

            recepcion.Detalles.Add(new RecepcionDetalle
            {
                OrdenCompraDetalleId = detalleOrden.Id,
                VarianteId = detalleOrden.VarianteId,
                Cantidad = linea.Cantidad,
                CostoUnitarioUSD = costo
            });

            if (detalleOrden.Variante is not null)
            {
                detalleOrden.Variante.PrecioCompraUSD = costo;
            }

            await _kardex.AplicarAsync(
                detalleOrden.VarianteId,
                TipoMovimientoInventario.Compra,
                linea.Cantidad,
                "recepcion",
                recepcion.Id,
                "Recepción de compra",
                cancellationToken);
        }

        recepcion.RecalcularTotal();
        orden.ActualizarEstadoRecepcion();

        var vencimiento = recepcion.Fecha.AddDays(orden.Proveedor?.DiasCredito ?? 0);
        var cuenta = new CuentaPorPagar
        {
            ProveedorId = orden.ProveedorId,
            OrdenCompraId = orden.Id,
            RecepcionId = recepcion.Id,
            Vencimiento = vencimiento,
            Estado = EstadoCuentaPorPagar.Pendiente
        };
        cuenta.Inicializar(recepcion.TotalUSD);
        await _compras.AgregarCuentaPorPagarAsync(cuenta, cancellationToken);

        await _compras.AgregarRecepcionAsync(recepcion, cancellationToken);
        await _compras.SaveChangesAsync(cancellationToken);

        var creada = await _compras.ObtenerRecepcionConDetalleAsync(recepcion.Id, cancellationToken);
        await _auditoria.RegistrarAsync("recepcion", "orden-compra", orden.Id, new { recepcion.TotalUSD, Lineas = recepcion.Detalles.Count }, cancellationToken);
        return MapearRecepcion(creada!);
    }

    // ================= Cuentas por pagar =================

    public async Task<ResultadoPaginado<CuentaPorPagarDto>> ObtenerCuentasPorPagarAsync(
        PaginacionRequest paginacion,
        Guid? proveedorId = null,
        bool soloPendientes = false,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _compras.ObtenerCuentasPorPagarPaginadoAsync(paginacion, proveedorId, soloPendientes, cancellationToken);
        var items = pagina.Items.Select(MapearCuentaPorPagar).ToList();
        return ResultadoPaginado<CuentaPorPagarDto>.Crear(items, pagina.Page, pagina.PageSize, pagina.TotalItems);
    }

    public async Task<CuentaPorPagarDto?> RegistrarPagoCuentaAsync(
        Guid cuentaId,
        RegistrarPagoProveedorDto dto,
        CancellationToken cancellationToken = default)
    {
        var cuenta = await _compras.ObtenerCuentaPorPagarConDetalleAsync(cuentaId, cancellationToken);
        if (cuenta is null || cuenta.IsDeleted)
        {
            return null;
        }

        try
        {
            cuenta.RegistrarPago(dto.Monto);
        }
        catch (InvalidOperationException ex)
        {
            throw new ReglaNegocioException(ex.Message);
        }

        await _compras.AgregarPagoProveedorAsync(new PagoProveedor
        {
            CuentaPorPagarId = cuentaId,
            Monto = dto.Monto,
            Moneda = dto.Moneda,
            MetodoPagoId = dto.MetodoPagoId,
            Referencia = dto.Referencia
        }, cancellationToken);

        await _compras.SaveChangesAsync(cancellationToken);
        return MapearCuentaPorPagar(cuenta);
    }

    private static RecepcionDto MapearRecepcion(Recepcion r)
        => new(
            r.Id,
            r.OrdenCompraId,
            r.OrdenCompra?.Numero ?? string.Empty,
            r.Fecha,
            r.TotalUSD,
            r.Observaciones,
            r.Detalles.Select(d => new RecepcionDetalleDto(
                d.Id,
                d.OrdenCompraDetalleId,
                d.VarianteId,
                d.Variante?.Sku ?? string.Empty,
                d.Cantidad,
                d.CostoUnitarioUSD)).ToList());

    private static CuentaPorPagarDto MapearCuentaPorPagar(CuentaPorPagar c)
        => new(c.Id, c.ProveedorId, c.Proveedor?.Nombre ?? string.Empty, c.OrdenCompraId, c.MontoUSD, c.SaldoUSD, c.Vencimiento, c.Estado);

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
