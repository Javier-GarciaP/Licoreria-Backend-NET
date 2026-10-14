using Licoreria.Application.Common;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Services;

public sealed class ServicioClienteVenta : IServicioClienteVenta
{
    private readonly IClienteRepository _clientes;
    private readonly OpcionesFidelidad _fidelidad;
    private readonly OpcionesCredito _credito;

    public ServicioClienteVenta(IClienteRepository clientes, OpcionesFidelidad fidelidad, OpcionesCredito credito)
    {
        _clientes = clientes;
        _fidelidad = fidelidad;
        _credito = credito;
    }

    public async Task<Cliente> ValidarClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        var cliente = await _clientes.ObtenerConDetalleAsync(clienteId, cancellationToken);
        if (cliente is null || cliente.IsDeleted)
        {
            throw new NoEncontradoException($"No existe el cliente {clienteId}.");
        }

        if (!cliente.Activo)
        {
            throw new ReglaNegocioException("El cliente está inactivo y no puede asociarse a la venta.");
        }

        return cliente;
    }

    public async Task<int> AcumularPorVentaAsync(Venta venta, decimal creditoUSD, CancellationToken cancellationToken = default)
    {
        if (venta.ClienteId is not Guid clienteId || _fidelidad.PuntosPorUSD <= 0)
        {
            return 0;
        }

        var baseUsd = Math.Max(0m, venta.TotalUSD - creditoUSD);
        var puntos = (int)Math.Floor(baseUsd * _fidelidad.PuntosPorUSD);
        if (puntos <= 0)
        {
            return 0;
        }

        var cliente = await _clientes.ObtenerConDetalleAsync(clienteId, cancellationToken)
            ?? throw new NoEncontradoException($"No existe el cliente {clienteId}.");

        cliente.AcumularPuntos(puntos);
        await _clientes.AgregarMovimientoAsync(new PuntosMovimiento
        {
            ClienteId = clienteId,
            Tipo = TipoMovimientoPuntos.Acumulacion,
            Puntos = puntos,
            Motivo = "Puntos por venta",
            ReferenciaTipo = "venta",
            ReferenciaId = venta.Id
        }, cancellationToken);

        return puntos;
    }

    public async Task<decimal> RegistrarCreditoAsync(
        Venta venta,
        IReadOnlyList<Pago> pagosCredito,
        DateTime? vencimiento,
        CancellationToken cancellationToken = default)
    {
        if (pagosCredito.Count == 0)
        {
            return 0m;
        }

        if (venta.ClienteId is not Guid clienteId)
        {
            throw new ReglaNegocioException("La venta a crédito requiere un cliente.");
        }

        var tasa = venta.TasaCambio;
        var montoUsd = pagosCredito.Sum(p => p.Moneda == Moneda.USD ? p.Monto : Math.Round(p.Monto / tasa, 2));
        montoUsd = Math.Min(montoUsd, venta.TotalUSD);
        if (montoUsd <= 0)
        {
            return 0m;
        }

        var vence = vencimiento ?? venta.Fecha.AddDays(_credito.DiasVencimiento);
        if (vence <= venta.Fecha)
        {
            throw new ReglaNegocioException("El vencimiento del crédito debe ser posterior a la fecha de la venta.");
        }

        var cuenta = new CuentaPorCobrar
        {
            ClienteId = clienteId,
            VentaId = venta.Id,
            Vencimiento = vence,
            Estado = EstadoCuentaPorCobrar.Pendiente
        };
        cuenta.Inicializar(montoUsd);

        await _clientes.AgregarCuentaPorCobrarAsync(cuenta, cancellationToken);
        return montoUsd;
    }

    public async Task<int> RevertirPorDevolucionAsync(Venta venta, decimal montoDevueltoUSD, CancellationToken cancellationToken = default)
    {
        if (venta.ClienteId is not Guid clienteId || _fidelidad.PuntosPorUSD <= 0)
        {
            return 0;
        }

        var puntos = (int)Math.Floor(montoDevueltoUSD * _fidelidad.PuntosPorUSD);
        if (puntos <= 0)
        {
            return 0;
        }

        var cliente = await _clientes.ObtenerConDetalleAsync(clienteId, cancellationToken);
        if (cliente is null)
        {
            return 0;
        }

        var revertidos = cliente.RevertirPuntos(puntos);
        if (revertidos <= 0)
        {
            return 0;
        }

        await _clientes.AgregarMovimientoAsync(new PuntosMovimiento
        {
            ClienteId = clienteId,
            Tipo = TipoMovimientoPuntos.Reversion,
            Puntos = revertidos,
            Motivo = "Reversión por devolución",
            ReferenciaTipo = "venta",
            ReferenciaId = venta.Id
        }, cancellationToken);

        return revertidos;
    }

    public async Task<bool> TieneCuentaPendienteAsync(Guid ventaId, CancellationToken cancellationToken = default)
    {
        var cuenta = await _clientes.ObtenerCuentaPorCobrarDeVentaAsync(ventaId, cancellationToken);
        return cuenta is { SaldoUSD: > 0 };
    }
}
