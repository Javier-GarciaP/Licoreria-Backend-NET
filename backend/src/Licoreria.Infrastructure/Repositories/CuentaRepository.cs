using Licoreria.Application.Common;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Repositories;

public class CuentaRepository : ICuentaRepository
{
    private readonly LicoreriaDbContext _context;

    public CuentaRepository(LicoreriaDbContext context) => _context = context;

    private IQueryable<Cuenta> ConDetalle()
        => _context.Cuentas
            .Include(c => c.SesionMesa)
            .Include(c => c.Comandas).ThenInclude(cd => cd.Detalles).ThenInclude(d => d.Variante).ThenInclude(v => v.Producto)
            .Include(c => c.Abonos).ThenInclude(a => a.MetodoPago)
            .Include(c => c.Divisiones);

    public Task<ResultadoPaginado<Cuenta>> ObtenerPaginadoAsync(
        PaginacionRequest paginacion,
        EstadoCuenta? estado = null,
        IReadOnlyList<EstadoCuenta>? estados = null,
        Guid? usuarioId = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = ConDetalle().AsNoTracking().Where(c => !c.IsDeleted);

        if (estado is not null)
        {
            consulta = consulta.Where(c => c.Estado == estado);
        }

        if (estados is { Count: > 0 })
        {
            consulta = consulta.Where(c => estados.Contains(c.Estado));
        }

        if (usuarioId is not null)
        {
            consulta = consulta.Where(c => c.CreatedBy == usuarioId);
        }

        return consulta.OrderByDescending(c => c.CreatedAt).PaginarAsync(paginacion, cancellationToken);
    }

    public Task<Cuenta?> ObtenerConDetalleAsync(Guid id, CancellationToken cancellationToken = default)
        => ConDetalle().FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

    public Task<Cuenta?> ObtenerCuentaAbiertaPorMesaAsync(Guid mesaId, CancellationToken cancellationToken = default)
        => ConDetalle().FirstOrDefaultAsync(
            c => !c.IsDeleted
                && c.SesionMesa.MesaId == mesaId
                && c.SesionMesa.CerradaEn == null
                && c.Estado != EstadoCuenta.Cerrada,
            cancellationToken);

    public async Task AgregarAsync(Cuenta cuenta, CancellationToken cancellationToken = default)
        => await _context.Cuentas.AddAsync(cuenta, cancellationToken);

    public async Task AgregarComandaAsync(Comanda comanda, CancellationToken cancellationToken = default)
        => await _context.Comandas.AddAsync(comanda, cancellationToken);

    public async Task AgregarAbonoAsync(Abono abono, CancellationToken cancellationToken = default)
        => await _context.Abonos.AddAsync(abono, cancellationToken);

    public Task<ComandaDetalle?> ObtenerDetalleAsync(Guid comandaId, Guid detalleId, CancellationToken cancellationToken = default)
        => _context.ComandaDetalles.FirstOrDefaultAsync(
            d => d.Id == detalleId && d.ComandaId == comandaId && !d.IsDeleted,
            cancellationToken);

    public async Task<IReadOnlyList<Guid>> ObtenerMesasOcupadasAsync(CancellationToken cancellationToken = default)
        => await _context.SesionesMesa
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.MesaId != null && s.CerradaEn == null)
            .Select(s => s.MesaId!.Value)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, Guid>> ObtenerCuentasAbiertasPorMesaAsync(
        CancellationToken cancellationToken = default)
    {
        var filas = await _context.Cuentas
            .AsNoTracking()
            .Where(c => !c.IsDeleted
                && c.Estado != EstadoCuenta.Cerrada
                && c.SesionMesa.MesaId != null
                && c.SesionMesa.CerradaEn == null)
            .Select(c => new { MesaId = c.SesionMesa.MesaId!.Value, CuentaId = c.Id })
            .ToListAsync(cancellationToken);

        return filas
            .GroupBy(f => f.MesaId)
            .ToDictionary(g => g.Key, g => g.First().CuentaId);
    }

    public async Task<IReadOnlyList<Cuenta>> ObtenerAbiertasConSesionAsync(CancellationToken cancellationToken = default)
        => await ConDetalle()
            .Where(c => !c.IsDeleted
                && c.Estado != EstadoCuenta.Cerrada
                && c.SesionMesa.CerradaEn == null)
            .ToListAsync(cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
