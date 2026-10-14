using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Services;

public sealed class ServicioEnvases : IServicioEnvases
{
    private readonly IPorcionRepository _porciones;
    private readonly IInventarioRepository _inventario;
    private readonly IServicioKardex _kardex;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IContextoUsuario _contextoUsuario;
    private readonly IRelojSistema _reloj;
    private readonly IServicioAuditoria _auditoria;
    private readonly INotificadorComandas _notificador;

    public ServicioEnvases(
        IPorcionRepository porciones,
        IInventarioRepository inventario,
        IServicioKardex kardex,
        IUnitOfWork unitOfWork,
        IContextoUsuario contextoUsuario,
        IRelojSistema reloj,
        IServicioAuditoria auditoria,
        INotificadorComandas notificador)
    {
        _porciones = porciones;
        _inventario = inventario;
        _kardex = kardex;
        _unitOfWork = unitOfWork;
        _contextoUsuario = contextoUsuario;
        _reloj = reloj;
        _auditoria = auditoria;
        _notificador = notificador;
    }

    public async Task<EnvaseAbiertoDto> DestaparAsync(DestaparEnvaseDto dto, CancellationToken cancellationToken = default)
    {
        EnvaseAbierto? creado = null;

        // Descontar la unidad y crear el envase se confirman juntos; si dos barras destapan la última
        // botella a la vez, una pierde por conflicto de versión, relee y recibe «existencia agotada».
        await _unitOfWork.EjecutarEnTransaccionAsync(async ct =>
        {
            creado = null;

            var porcion = await _inventario.ObtenerVarianteConRecetasAsync(dto.PorcionVarianteId, ct);
            if (porcion is null || porcion.IsDeleted)
            {
                throw new NoEncontradoException($"No existe la porción {dto.PorcionVarianteId}.");
            }

            var origenes = await _porciones.ObtenerOrigenesAsync(dto.PorcionVarianteId, ct);
            if (origenes.Count == 0)
            {
                throw new ReglaNegocioException($"'{porcion.Nombre}' no es una porción: no tiene presentaciones de origen.");
            }

            var origen = origenes.FirstOrDefault(o => o.PresentacionVarianteId == dto.PresentacionVarianteId)
                ?? throw new ReglaNegocioException(
                    $"Esa presentación no es un origen de '{porcion.Nombre}'. Elige una de las definidas en el catálogo.");

            var stock = await _inventario.ObtenerStockAsync(dto.PresentacionVarianteId, ct);
            var disponible = stock?.Disponible ?? 0m;
            if (disponible < 1m)
            {
                throw new ExistenciaAgotadaException(origen.PresentacionVariante?.Nombre ?? "la presentación", 1m, disponible);
            }

            var envase = EnvaseAbierto.Destapar(
                dto.PorcionVarianteId,
                dto.PresentacionVarianteId,
                origen.PorcionesPorUnidad,
                _contextoUsuario.UsuarioId,
                _reloj.UtcNow);

            await _kardex.AplicarAsync(
                dto.PresentacionVarianteId,
                TipoMovimientoInventario.Apertura,
                -1m,
                "envase",
                envase.Id,
                $"Apertura para {porcion.Nombre}",
                ct);

            await _porciones.AgregarEnvaseAsync(envase, ct);
            _auditoria.Registrar(
                "destapar", "envase", envase.Id,
                new { dto.PorcionVarianteId, dto.PresentacionVarianteId, envase.PorcionesIniciales });
            creado = envase;
        }, cancellationToken);

        await _notificador.EnvaseActualizadoAsync(creado!.Id, creado.PorcionVarianteId, creado.Estado.ToString(), cancellationToken);

        var detalle = await _porciones.ObtenerEnvaseAsync(creado.Id, cancellationToken);
        return Mapear(detalle ?? creado);
    }

    public async Task<EnvaseAbiertoDto?> DescartarAsync(Guid id, DescartarEnvaseDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Motivo))
        {
            throw new ReglaNegocioException("El motivo del descarte es obligatorio.");
        }

        var encontrado = false;
        var perdidas = 0;

        await _unitOfWork.EjecutarEnTransaccionAsync(async ct =>
        {
            encontrado = false;

            var envase = await _porciones.ObtenerEnvaseAsync(id, ct);
            if (envase is null)
            {
                return;
            }

            encontrado = true;

            if (envase.Estado != EstadoEnvase.Abierto)
            {
                throw new ReglaNegocioException("El envase ya está cerrado: no se puede descartar.");
            }

            // Lo reservado por comandas pendientes depende de las porciones abiertas: no se puede dejar
            // sin cubrirlas.
            var abiertos = await _porciones.ObtenerAbiertosAsync(envase.PorcionVarianteId, ct);
            var restantesDeLosDemas = abiertos.Where(e => e.Id != envase.Id).Sum(e => e.PorcionesRestantes);
            var reservadas = (await _porciones.ObtenerStockPorcionAsync(envase.PorcionVarianteId, ct))?.PorcionesReservadas ?? 0;
            if (reservadas > restantesDeLosDemas)
            {
                throw new ConflictoException(
                    $"Hay {reservadas - restantesDeLosDemas} porción(es) reservada(s) por pedidos pendientes que dependen de este envase: " +
                    "sirve o cancela esos pedidos antes de descartarlo.");
            }

            perdidas = envase.Descartar(dto.Motivo, _reloj.UtcNow);

            if (perdidas > 0)
            {
                await _inventario.AgregarMovimientoAsync(new MovimientoInventario
                {
                    VarianteId = envase.PorcionVarianteId,
                    Tipo = TipoMovimientoInventario.Merma,
                    Cantidad = -perdidas,
                    ReferenciaTipo = "envase",
                    ReferenciaId = envase.Id,
                    Motivo = $"Envase descartado: {envase.MotivoDescarte}"
                }, ct);
            }

            _auditoria.Registrar("descartar", "envase", id, new { dto.Motivo, perdidas });
        }, cancellationToken);

        if (!encontrado)
        {
            return null;
        }

        var actualizado = await _porciones.ObtenerEnvaseAsync(id, cancellationToken);
        if (actualizado is null) return null;

        await _notificador.EnvaseActualizadoAsync(id, actualizado.PorcionVarianteId, actualizado.Estado.ToString(), cancellationToken);
        return Mapear(actualizado);
    }

    public async Task<IReadOnlyList<EnvaseAbiertoDto>> ListarAsync(
        Guid? porcionVarianteId = null,
        EstadoEnvase? estado = null,
        int limite = 100,
        CancellationToken cancellationToken = default)
    {
        var envases = await _porciones.ListarAsync(porcionVarianteId, estado, Math.Clamp(limite, 1, 500), cancellationToken);
        return envases.Select(Mapear).ToList();
    }

    private static EnvaseAbiertoDto Mapear(EnvaseAbierto e)
        => new(
            e.Id,
            e.PorcionVarianteId,
            NombreCompleto(e.PorcionVariante),
            e.PresentacionVarianteId,
            e.PresentacionVariante?.Nombre ?? string.Empty,
            e.PorcionesIniciales,
            e.PorcionesRestantes,
            e.Estado,
            e.AbiertoEn,
            e.AbiertoPorUsuarioId,
            e.CerradoEn,
            e.MotivoDescarte);

    private static string NombreCompleto(ProductoVariante? variante)
    {
        if (variante is null) return string.Empty;
        var producto = variante.Producto?.Nombre;
        return string.IsNullOrWhiteSpace(producto) ? variante.Nombre : $"{producto} · {variante.Nombre}";
    }
}
