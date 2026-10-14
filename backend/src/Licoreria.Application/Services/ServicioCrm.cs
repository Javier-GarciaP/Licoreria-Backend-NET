using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Services;

public sealed class ServicioCrm : IServicioCrm
{
    private readonly IClienteRepository _clientes;

    public ServicioCrm(IClienteRepository clientes) => _clientes = clientes;

    public async Task<ResultadoPaginado<ClienteDto>> ObtenerClientesAsync(
        PaginacionRequest paginacion,
        string? busqueda = null,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _clientes.ObtenerPaginadoAsync(paginacion, busqueda, cancellationToken);
        var items = pagina.Items.Select(MapearCliente).ToList();
        return ResultadoPaginado<ClienteDto>.Crear(items, pagina.Page, pagina.PageSize, pagina.TotalItems);
    }

    public async Task<ClienteDto?> ObtenerClienteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await _clientes.ObtenerConDetalleAsync(id, cancellationToken);
        return cliente is null ? null : MapearCliente(cliente);
    }

    public async Task<ClienteDto> CrearClienteAsync(ClienteCrearDto dto, CancellationToken cancellationToken = default)
    {
        var cliente = new Cliente
        {
            Nombre = dto.Nombre,
            Rif = dto.Rif,
            Ci = dto.Ci,
            Email = dto.Email,
            Telefono = dto.Telefono,
            Direccion = dto.Direccion,
            Activo = true
        };

        await _clientes.AgregarAsync(cliente, cancellationToken);
        await _clientes.SaveChangesAsync(cancellationToken);
        return MapearCliente(cliente);
    }

    public async Task<ClienteDto?> EditarClienteAsync(ClienteEditarDto dto, CancellationToken cancellationToken = default)
    {
        var cliente = await _clientes.ObtenerConDetalleAsync(dto.Id, cancellationToken);
        if (cliente is null || cliente.IsDeleted)
        {
            return null;
        }

        cliente.Nombre = dto.Nombre;
        cliente.Rif = dto.Rif;
        cliente.Ci = dto.Ci;
        cliente.Email = dto.Email;
        cliente.Telefono = dto.Telefono;
        cliente.Direccion = dto.Direccion;
        cliente.Activo = dto.Activo;

        await _clientes.SaveChangesAsync(cancellationToken);
        return MapearCliente(cliente);
    }

    public async Task<bool> EliminarClienteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await _clientes.ObtenerConDetalleAsync(id, cancellationToken);
        if (cliente is null || cliente.IsDeleted)
        {
            return false;
        }

        cliente.EliminarLogico();
        await _clientes.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<PuntosMovimientoDto>> ObtenerPuntosAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        var cliente = await _clientes.ObtenerConDetalleAsync(clienteId, cancellationToken);
        if (cliente is null)
        {
            throw new NoEncontradoException($"No existe el cliente {clienteId}.");
        }

        return cliente.Movimientos
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new PuntosMovimientoDto(m.Id, m.Tipo, m.Puntos, m.Motivo, m.CreatedAt))
            .ToList();
    }

    public async Task<ClienteDto?> AcumularPuntosAsync(Guid clienteId, PuntosOperacionDto dto, CancellationToken cancellationToken = default)
    {
        var cliente = await _clientes.ObtenerConDetalleAsync(clienteId, cancellationToken);
        if (cliente is null || cliente.IsDeleted)
        {
            return null;
        }

        try
        {
            cliente.AcumularPuntos(dto.Puntos);
        }
        catch (InvalidOperationException ex)
        {
            throw new ReglaNegocioException(ex.Message);
        }

        await _clientes.AgregarMovimientoAsync(new PuntosMovimiento
        {
            ClienteId = clienteId,
            Tipo = TipoMovimientoPuntos.Acumulacion,
            Puntos = dto.Puntos,
            Motivo = dto.Motivo,
            ReferenciaTipo = dto.ReferenciaTipo,
            ReferenciaId = dto.ReferenciaId
        }, cancellationToken);

        await _clientes.SaveChangesAsync(cancellationToken);
        return MapearCliente(cliente);
    }

    public async Task<ClienteDto?> CanjearPuntosAsync(Guid clienteId, PuntosOperacionDto dto, CancellationToken cancellationToken = default)
    {
        var cliente = await _clientes.ObtenerConDetalleAsync(clienteId, cancellationToken);
        if (cliente is null || cliente.IsDeleted)
        {
            return null;
        }

        try
        {
            cliente.CanjearPuntos(dto.Puntos);
        }
        catch (InvalidOperationException ex)
        {
            throw new ReglaNegocioException(ex.Message);
        }

        await _clientes.AgregarMovimientoAsync(new PuntosMovimiento
        {
            ClienteId = clienteId,
            Tipo = TipoMovimientoPuntos.Canje,
            Puntos = dto.Puntos,
            Motivo = dto.Motivo,
            ReferenciaTipo = dto.ReferenciaTipo,
            ReferenciaId = dto.ReferenciaId
        }, cancellationToken);

        await _clientes.SaveChangesAsync(cancellationToken);
        return MapearCliente(cliente);
    }

    public async Task<ResultadoPaginado<CuentaPorCobrarDto>> ObtenerCuentasPorCobrarAsync(
        PaginacionRequest paginacion,
        Guid? clienteId = null,
        bool soloPendientes = false,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _clientes.ObtenerCuentasPorCobrarAsync(paginacion, clienteId, soloPendientes, cancellationToken);
        var items = pagina.Items.Select(MapearCuenta).ToList();
        return ResultadoPaginado<CuentaPorCobrarDto>.Crear(items, pagina.Page, pagina.PageSize, pagina.TotalItems);
    }

    public async Task<CuentaPorCobrarDto> RegistrarCuentaPorCobrarAsync(
        CuentaPorCobrarCrearDto dto,
        CancellationToken cancellationToken = default)
    {
        var cliente = await _clientes.ObtenerConDetalleAsync(dto.ClienteId, cancellationToken)
            ?? throw new NoEncontradoException($"No existe el cliente {dto.ClienteId}.");

        if (dto.MontoUSD <= 0)
        {
            throw new ReglaNegocioException("El monto del crédito debe ser mayor que cero.");
        }

        var cuenta = new CuentaPorCobrar
        {
            ClienteId = cliente.Id,
            Vencimiento = dto.Vencimiento,
            Estado = EstadoCuentaPorCobrar.Pendiente
        };
        cuenta.Inicializar(dto.MontoUSD);

        await _clientes.AgregarCuentaPorCobrarAsync(cuenta, cancellationToken);
        await _clientes.SaveChangesAsync(cancellationToken);

        cuenta.Cliente = cliente;
        return MapearCuenta(cuenta);
    }

    public async Task<CuentaPorCobrarDto?> RegistrarPagoCuentaPorCobrarAsync(
        Guid cuentaId,
        PagoCuentaPorCobrarDto dto,
        CancellationToken cancellationToken = default)
    {
        var cuenta = await _clientes.ObtenerCuentaPorCobrarAsync(cuentaId, cancellationToken);
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

        await _clientes.SaveChangesAsync(cancellationToken);
        return MapearCuenta(cuenta);
    }

    private static ClienteDto MapearCliente(Cliente c)
        => new(c.Id, c.Nombre, c.Rif, c.Ci, c.Email, c.Telefono, c.Direccion, c.Puntos, c.Activo);

    private static CuentaPorCobrarDto MapearCuenta(CuentaPorCobrar c)
        => new(c.Id, c.ClienteId, c.Cliente?.Nombre ?? string.Empty, c.MontoUSD, c.SaldoUSD, c.Vencimiento, c.Estado);
}
