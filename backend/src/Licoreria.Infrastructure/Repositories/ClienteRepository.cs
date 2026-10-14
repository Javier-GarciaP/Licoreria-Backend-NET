using Licoreria.Application.Common;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly LicoreriaDbContext _context;

    public ClienteRepository(LicoreriaDbContext context) => _context = context;

    public Task<ResultadoPaginado<Cliente>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        string? busqueda = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.Clientes.AsNoTracking().Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var filtro = busqueda.Trim().ToLower();
            consulta = consulta.Where(c =>
                c.Nombre.ToLower().Contains(filtro) ||
                (c.Rif != null && c.Rif.ToLower().Contains(filtro)) ||
                (c.Ci != null && c.Ci.ToLower().Contains(filtro)) ||
                (c.Telefono != null && c.Telefono.ToLower().Contains(filtro)));
        }

        return consulta.OrderBy(c => c.Nombre).PaginarAsync(paginacion, cancellationToken);
    }

    public Task<Cliente?> ObtenerConDetalleAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Clientes
            .Include(c => c.Movimientos)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

    public async Task AgregarAsync(Cliente cliente, CancellationToken cancellationToken = default)
        => await _context.Clientes.AddAsync(cliente, cancellationToken);

    public async Task AgregarMovimientoAsync(PuntosMovimiento movimiento, CancellationToken cancellationToken = default)
        => await _context.PuntosMovimiento.AddAsync(movimiento, cancellationToken);

    public Task<ResultadoPaginado<CuentaPorCobrar>> ObtenerCuentasPorCobrarAsync(
        PaginacionRequest paginacion,
        Guid? clienteId = null,
        bool soloPendientes = false,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.CuentasPorCobrar
            .Include(c => c.Cliente)
            .AsNoTracking()
            .Where(c => !c.IsDeleted);

        if (clienteId is not null)
        {
            consulta = consulta.Where(c => c.ClienteId == clienteId);
        }

        if (soloPendientes)
        {
            consulta = consulta.Where(c => c.Estado != EstadoCuentaPorCobrar.Pagada);
        }

        return consulta.OrderBy(c => c.Vencimiento).PaginarAsync(paginacion, cancellationToken);
    }

    public Task<CuentaPorCobrar?> ObtenerCuentaPorCobrarAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.CuentasPorCobrar
            .Include(c => c.Cliente)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

    public async Task AgregarCuentaPorCobrarAsync(CuentaPorCobrar cuenta, CancellationToken cancellationToken = default)
        => await _context.CuentasPorCobrar.AddAsync(cuenta, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
