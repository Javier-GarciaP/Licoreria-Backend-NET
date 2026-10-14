using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Services;

public sealed class ServicioCatalogo : IServicioCatalogo
{
    private readonly IProductoRepository _productoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IRepository<Marca> _marcaRepository;
    private readonly IRepository<UnidadMedida> _unidadRepository;
    private readonly IRepository<Impuesto> _impuestoRepository;
    private readonly IRepository<ListaPrecio> _listaPrecioRepository;
    private readonly IRepository<ProductoVariante> _variantes;
    private readonly IRepository<Modificador> _modificadorRepository;
    private readonly IRepository<ProductoModificador> _productoModificadorRepository;
    private readonly IInventarioRepository _inventario;
    private readonly IPorcionRepository _porciones;

    public ServicioCatalogo(
        IProductoRepository productoRepository,
        ICategoriaRepository categoriaRepository,
        IRepository<Marca> marcaRepository,
        IRepository<UnidadMedida> unidadRepository,
        IRepository<Impuesto> impuestoRepository,
        IRepository<ListaPrecio> listaPrecioRepository,
        IRepository<ProductoVariante> variantes,
        IRepository<Modificador> modificadorRepository,
        IRepository<ProductoModificador> productoModificadorRepository,
        IInventarioRepository inventario,
        IPorcionRepository porciones)
    {
        _productoRepository = productoRepository;
        _categoriaRepository = categoriaRepository;
        _marcaRepository = marcaRepository;
        _unidadRepository = unidadRepository;
        _impuestoRepository = impuestoRepository;
        _listaPrecioRepository = listaPrecioRepository;
        _variantes = variantes;
        _modificadorRepository = modificadorRepository;
        _productoModificadorRepository = productoModificadorRepository;
        _inventario = inventario;
        _porciones = porciones;
    }

    // ================= Categorías =================

    public async Task<IReadOnlyList<CategoriaDto>> ObtenerCategoriasAsync(CancellationToken cancellationToken = default)
    {
        var categorias = await _categoriaRepository.GetAllAsync(cancellationToken);
        return categorias.Where(c => !c.IsDeleted).Select(MapearCategoria).ToList();
    }

    public async Task<CategoriaDto?> ObtenerCategoriaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var categoria = await _categoriaRepository.GetByIdAsync(id, cancellationToken);
        return categoria is null || categoria.IsDeleted ? null : MapearCategoria(categoria);
    }

    public async Task<CategoriaDto> CrearCategoriaAsync(CategoriaCrearDto dto, CancellationToken cancellationToken = default)
    {
        if (await _categoriaRepository.ExisteNombreAsync(dto.Nombre, null, cancellationToken))
        {
            throw new ConflictoException($"Ya existe una categoría con el nombre '{dto.Nombre}'.");
        }

        var categoria = new Categoria
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            CategoriaPadreId = dto.CategoriaPadreId,
            Activo = dto.Activo
        };

        await _categoriaRepository.AddAsync(categoria, cancellationToken);
        await _categoriaRepository.SaveChangesAsync(cancellationToken);
        return MapearCategoria(categoria);
    }

    public async Task<CategoriaDto?> EditarCategoriaAsync(CategoriaEditarDto dto, CancellationToken cancellationToken = default)
    {
        var categoria = await _categoriaRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (categoria is null || categoria.IsDeleted)
        {
            return null;
        }

        if (await _categoriaRepository.ExisteNombreAsync(dto.Nombre, dto.Id, cancellationToken))
        {
            throw new ConflictoException($"Ya existe otra categoría con el nombre '{dto.Nombre}'.");
        }

        if (dto.CategoriaPadreId == dto.Id)
        {
            throw new ReglaNegocioException("Una categoría no puede ser su propia categoría padre.");
        }

        categoria.Nombre = dto.Nombre;
        categoria.Descripcion = dto.Descripcion;
        categoria.CategoriaPadreId = dto.CategoriaPadreId;
        categoria.Activo = dto.Activo;

        _categoriaRepository.Update(categoria);
        await _categoriaRepository.SaveChangesAsync(cancellationToken);
        return MapearCategoria(categoria);
    }

    public async Task<bool> EliminarCategoriaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var categoria = await _categoriaRepository.GetByIdAsync(id, cancellationToken);
        if (categoria is null || categoria.IsDeleted)
        {
            return false;
        }

        categoria.EliminarLogico();
        _categoriaRepository.Update(categoria);
        await _categoriaRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ================= Marcas =================

    public async Task<IReadOnlyList<MarcaDto>> ObtenerMarcasAsync(CancellationToken cancellationToken = default)
    {
        var marcas = await _marcaRepository.GetAllAsync(cancellationToken);
        return marcas.Where(m => !m.IsDeleted).Select(MapearMarca).ToList();
    }

    public async Task<MarcaDto> CrearMarcaAsync(MarcaCrearDto dto, CancellationToken cancellationToken = default)
    {
        var marca = new Marca { Nombre = dto.Nombre, Descripcion = dto.Descripcion, Activo = dto.Activo };
        await _marcaRepository.AddAsync(marca, cancellationToken);
        await _marcaRepository.SaveChangesAsync(cancellationToken);
        return MapearMarca(marca);
    }

    public async Task<MarcaDto?> EditarMarcaAsync(MarcaEditarDto dto, CancellationToken cancellationToken = default)
    {
        var marca = await _marcaRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (marca is null || marca.IsDeleted)
        {
            return null;
        }

        marca.Nombre = dto.Nombre;
        marca.Descripcion = dto.Descripcion;
        marca.Activo = dto.Activo;
        _marcaRepository.Update(marca);
        await _marcaRepository.SaveChangesAsync(cancellationToken);
        return MapearMarca(marca);
    }

    public async Task<bool> EliminarMarcaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var marca = await _marcaRepository.GetByIdAsync(id, cancellationToken);
        if (marca is null || marca.IsDeleted)
        {
            return false;
        }

        marca.EliminarLogico();
        _marcaRepository.Update(marca);
        await _marcaRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ================= Unidades de medida =================

    public async Task<IReadOnlyList<UnidadMedidaDto>> ObtenerUnidadesAsync(CancellationToken cancellationToken = default)
    {
        var unidades = await _unidadRepository.GetAllAsync(cancellationToken);
        return unidades.Where(u => !u.IsDeleted).Select(u => new UnidadMedidaDto(u.Id, u.Nombre, u.Abreviatura)).ToList();
    }

    public async Task<UnidadMedidaDto> CrearUnidadAsync(UnidadMedidaCrearDto dto, CancellationToken cancellationToken = default)
    {
        var unidad = new UnidadMedida { Nombre = dto.Nombre, Abreviatura = dto.Abreviatura };
        await _unidadRepository.AddAsync(unidad, cancellationToken);
        await _unidadRepository.SaveChangesAsync(cancellationToken);
        return new UnidadMedidaDto(unidad.Id, unidad.Nombre, unidad.Abreviatura);
    }

    public async Task<UnidadMedidaDto?> EditarUnidadAsync(UnidadMedidaEditarDto dto, CancellationToken cancellationToken = default)
    {
        var unidad = await _unidadRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (unidad is null || unidad.IsDeleted)
        {
            return null;
        }

        unidad.Nombre = dto.Nombre;
        unidad.Abreviatura = dto.Abreviatura;
        _unidadRepository.Update(unidad);
        await _unidadRepository.SaveChangesAsync(cancellationToken);
        return new UnidadMedidaDto(unidad.Id, unidad.Nombre, unidad.Abreviatura);
    }

    public async Task<bool> EliminarUnidadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var unidad = await _unidadRepository.GetByIdAsync(id, cancellationToken);
        if (unidad is null || unidad.IsDeleted)
        {
            return false;
        }

        unidad.EliminarLogico();
        _unidadRepository.Update(unidad);
        await _unidadRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ================= Impuestos =================

    public async Task<IReadOnlyList<ImpuestoDto>> ObtenerImpuestosAsync(CancellationToken cancellationToken = default)
    {
        var impuestos = await _impuestoRepository.GetAllAsync(cancellationToken);
        return impuestos.Where(i => !i.IsDeleted).Select(i => new ImpuestoDto(i.Id, i.Nombre, i.Porcentaje, i.Activo)).ToList();
    }

    public async Task<ImpuestoDto> CrearImpuestoAsync(ImpuestoCrearDto dto, CancellationToken cancellationToken = default)
    {
        var impuesto = new Impuesto { Nombre = dto.Nombre, Porcentaje = dto.Porcentaje, Activo = dto.Activo };
        await _impuestoRepository.AddAsync(impuesto, cancellationToken);
        await _impuestoRepository.SaveChangesAsync(cancellationToken);
        return new ImpuestoDto(impuesto.Id, impuesto.Nombre, impuesto.Porcentaje, impuesto.Activo);
    }

    public async Task<ImpuestoDto?> EditarImpuestoAsync(ImpuestoEditarDto dto, CancellationToken cancellationToken = default)
    {
        var impuesto = await _impuestoRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (impuesto is null || impuesto.IsDeleted)
        {
            return null;
        }

        impuesto.Nombre = dto.Nombre;
        impuesto.Porcentaje = dto.Porcentaje;
        impuesto.Activo = dto.Activo;
        _impuestoRepository.Update(impuesto);
        await _impuestoRepository.SaveChangesAsync(cancellationToken);
        return new ImpuestoDto(impuesto.Id, impuesto.Nombre, impuesto.Porcentaje, impuesto.Activo);
    }

    public async Task<bool> EliminarImpuestoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var impuesto = await _impuestoRepository.GetByIdAsync(id, cancellationToken);
        if (impuesto is null || impuesto.IsDeleted)
        {
            return false;
        }

        impuesto.EliminarLogico();
        _impuestoRepository.Update(impuesto);
        await _impuestoRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ================= Listas de precio =================

    public async Task<IReadOnlyList<ListaPrecioDto>> ObtenerListasPrecioAsync(CancellationToken cancellationToken = default)
    {
        var listas = await _listaPrecioRepository.GetAllAsync(cancellationToken);
        return listas.Where(l => !l.IsDeleted).Select(MapearListaPrecio).ToList();
    }

    public async Task<ListaPrecioDto> CrearListaPrecioAsync(ListaPrecioCrearDto dto, CancellationToken cancellationToken = default)
    {
        var lista = new ListaPrecio
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            EsPredeterminada = dto.EsPredeterminada,
            Activo = dto.Activo
        };

        await _listaPrecioRepository.AddAsync(lista, cancellationToken);
        await _listaPrecioRepository.SaveChangesAsync(cancellationToken);
        return MapearListaPrecio(lista);
    }

    public async Task<ListaPrecioDto?> EditarListaPrecioAsync(ListaPrecioEditarDto dto, CancellationToken cancellationToken = default)
    {
        var lista = await _listaPrecioRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (lista is null || lista.IsDeleted)
        {
            return null;
        }

        lista.Nombre = dto.Nombre;
        lista.Descripcion = dto.Descripcion;
        lista.EsPredeterminada = dto.EsPredeterminada;
        lista.Activo = dto.Activo;
        _listaPrecioRepository.Update(lista);
        await _listaPrecioRepository.SaveChangesAsync(cancellationToken);
        return MapearListaPrecio(lista);
    }

    public async Task<bool> EliminarListaPrecioAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lista = await _listaPrecioRepository.GetByIdAsync(id, cancellationToken);
        if (lista is null || lista.IsDeleted)
        {
            return false;
        }

        lista.EliminarLogico();
        _listaPrecioRepository.Update(lista);
        await _listaPrecioRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ================= Productos =================

    public async Task<ResultadoPaginado<ProductoDto>> ObtenerProductosAsync(
        PaginacionRequest paginacion,
        Guid? categoriaId = null,
        string? busqueda = null,
        bool? activo = null,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _productoRepository.ObtenerPaginadoAsync(paginacion, categoriaId, busqueda, activo, cancellationToken);

        var varianteIds = new List<Guid>();
        foreach (var producto in pagina.Items)
        {
            foreach (var variante in producto.Variantes)
            {
                if (variante.Activo && !variante.IsDeleted) varianteIds.Add(variante.Id);
            }
        }

        var porVariante = (await _inventario.ObtenerStockDeVariantesAsync(varianteIds, cancellationToken))
            .ToDictionary(s => s.VarianteId);
        var porciones = await CargarDatosPorcionAsync(varianteIds, cancellationToken);

        var items = pagina.Items.Select(p => MapearProducto(p, porVariante, porciones)).ToList();
        return ResultadoPaginado<ProductoDto>.Crear(items, pagina.Page, pagina.PageSize, pagina.TotalItems);
    }

    public async Task<ProductoDto?> ObtenerProductoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var producto = await _productoRepository.ObtenerConDetalleAsync(id, cancellationToken);
        return producto is null || producto.IsDeleted ? null : await MapearProductoAsync(producto, cancellationToken);
    }

    public async Task<ProductoDto> CrearProductoAsync(ProductoCrearDto dto, CancellationToken cancellationToken = default)
    {
        await ValidarCategoriaAsync(dto.CategoriaId, cancellationToken);

        if (dto.Variantes.Count == 0)
        {
            throw new ReglaNegocioException("El producto debe tener al menos una variante.");
        }

        await ValidarSkusYCodigosAsync(dto.Variantes, null, cancellationToken);
        ValidarConsistenciaArea(dto.Tipo, dto.AreaDestino, dto.Variantes);

        var producto = new Producto
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            CategoriaId = dto.CategoriaId,
            MarcaId = dto.MarcaId,
            ImpuestoId = dto.ImpuestoId,
            Tipo = dto.Tipo,
            AreaDestino = dto.AreaDestino,
            GradoAlcoholico = dto.GradoAlcoholico,
            ImagenUrl = dto.ImagenUrl,
            Activo = true
        };

        var creadas = new List<ProductoVariante>();
        foreach (var varianteDto in dto.Variantes)
        {
            var variante = CrearVariante(varianteDto);
            producto.Variantes.Add(variante);
            creadas.Add(variante);
        }

        await _productoRepository.AddAsync(producto, cancellationToken);
        await _productoRepository.SaveChangesAsync(cancellationToken);

        for (var i = 0; i < dto.Variantes.Count; i += 1)
        {
            await AsegurarStockAsync(
                creadas[i].Id,
                creadas[i].EsBase,
                dto.Variantes[i].StockInicial,
                dto.Variantes[i].StockMinimo,
                dto.Variantes[i].StockMaximo,
                cancellationToken);
        }

        var creado = await _productoRepository.ObtenerConDetalleAsync(producto.Id, cancellationToken);
        return await MapearProductoAsync(creado!, cancellationToken);
    }

    public async Task<ProductoDto?> EditarProductoAsync(ProductoEditarDto dto, CancellationToken cancellationToken = default)
    {
        var producto = await _productoRepository.ObtenerConDetalleAsync(dto.Id, cancellationToken);
        if (producto is null || producto.IsDeleted)
        {
            return null;
        }

        await ValidarCategoriaAsync(dto.CategoriaId, cancellationToken);

        if (dto.Variantes.Count == 0)
        {
            throw new ReglaNegocioException("El producto debe tener al menos una variante.");
        }

        await ValidarSkusYCodigosAsync(dto.Variantes, dto.Id, cancellationToken);
        ValidarConsistenciaArea(dto.Tipo, dto.AreaDestino, dto.Variantes);

        producto.ActualizarDatos(
            dto.Nombre, dto.Descripcion, dto.CategoriaId, dto.MarcaId,
            dto.ImpuestoId, dto.Tipo, dto.AreaDestino, dto.GradoAlcoholico, dto.ImagenUrl);

        if (dto.Activo)
        {
            producto.Activar();
        }
        else
        {
            producto.Desactivar();
        }

        await SincronizarVariantesAsync(producto, dto.Variantes, cancellationToken);

        // El producto ya está rastreado: no se llama a Update, que marcaría como modificadas las entidades
        // nuevas con clave asignada (los códigos de barras recreados) y fallaría con un conflicto de versión.
        await _productoRepository.SaveChangesAsync(cancellationToken);

        var actualizado = await _productoRepository.ObtenerConDetalleAsync(producto.Id, cancellationToken);
        return await MapearProductoAsync(actualizado!, cancellationToken);
    }

    public async Task<bool> EliminarProductoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var producto = await _productoRepository.ObtenerConDetalleAsync(id, cancellationToken);
        if (producto is null || producto.IsDeleted)
        {
            return false;
        }

        foreach (var variante in producto.Variantes.Where(v => v.Activo && !v.IsDeleted))
        {
            await ExigirSinVinculosDePorcionAsync(variante, "eliminar el producto", cancellationToken);
        }

        producto.EliminarLogico();
        _productoRepository.Update(producto);
        await _productoRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ================= Recetas =================

    public async Task<IReadOnlyList<RecetaDto>> ObtenerRecetasAsync(Guid varianteVendidaId, CancellationToken cancellationToken = default)
    {
        var recetas = await _productoRepository.ObtenerRecetasAsync(varianteVendidaId, cancellationToken);
        return recetas.Select(r => new RecetaDto(
            r.Id,
            r.VarianteInsumoId,
            r.VarianteInsumo?.Nombre ?? string.Empty,
            r.VarianteInsumo?.Sku ?? string.Empty,
            r.Cantidad)).ToList();
    }

    public async Task<RecetaDto> AgregarRecetaAsync(Guid varianteVendidaId, RecetaCrearDto dto, CancellationToken cancellationToken = default)
    {
        var vendida = await _variantes.GetByIdAsync(varianteVendidaId, cancellationToken);
        if (vendida is null || vendida.IsDeleted)
        {
            throw new NoEncontradoException($"No existe la variante {varianteVendidaId}.");
        }

        if (dto.Cantidad <= 0)
        {
            throw new ReglaNegocioException("La cantidad de la receta debe ser mayor que cero.");
        }

        if (dto.VarianteInsumoId == varianteVendidaId)
        {
            throw new ReglaNegocioException("Una variante no puede consumirse a sí misma.");
        }

        if (await _porciones.TieneOrigenesAsync(varianteVendidaId, cancellationToken))
        {
            throw new ReglaNegocioException(
                "Una porción no lleva receta: se sirve de sus presentaciones de origen. Elige una sola naturaleza para la variante.");
        }

        var insumo = await _variantes.GetByIdAsync(dto.VarianteInsumoId, cancellationToken);
        if (insumo is null || insumo.IsDeleted)
        {
            throw new NoEncontradoException($"No existe la variante insumo {dto.VarianteInsumoId}.");
        }

        if (!insumo.Activo)
        {
            throw new ReglaNegocioException($"El insumo '{insumo.Nombre}' no está activo.");
        }

        // Los insumos deben tener existencia propia (base o con fila de stock) para
        // que la salida por receta pueda descontarse del inventario.
        // Un insumo puede ser una porción (un trago de ron): entonces se consumen porciones enteras.
        var insumoEsPorcion = await _porciones.TieneOrigenesAsync(insumo.Id, cancellationToken);
        if (insumoEsPorcion)
        {
            if (dto.Cantidad != Math.Truncate(dto.Cantidad))
            {
                throw new ReglaNegocioException(
                    $"'{insumo.Nombre}' es una porción: la cantidad de la receta debe ser un número entero de porciones.");
            }
        }
        else
        {
            var stockInsumo = await _inventario.ObtenerStockAsync(insumo.Id, cancellationToken);
            if (!insumo.EsBase && stockInsumo is null)
            {
                throw new ReglaNegocioException(
                    $"El insumo '{insumo.Nombre}' no guarda existencia propia; usa una presentación base, una porción o una variante con stock.");
            }
        }

        var receta = new Receta
        {
            VarianteVendidaId = varianteVendidaId,
            VarianteInsumoId = dto.VarianteInsumoId,
            Cantidad = dto.Cantidad
        };

        await _productoRepository.AgregarRecetaAsync(receta, cancellationToken);
        await _productoRepository.SaveChangesAsync(cancellationToken);

        var recetas = await _productoRepository.ObtenerRecetasAsync(varianteVendidaId, cancellationToken);
        var creada = recetas.First(r => r.Id == receta.Id);
        return new RecetaDto(creada.Id, creada.VarianteInsumoId, creada.VarianteInsumo?.Nombre ?? string.Empty, creada.VarianteInsumo?.Sku ?? string.Empty, creada.Cantidad);
    }

    public async Task<bool> EliminarRecetaAsync(Guid varianteVendidaId, Guid recetaId, CancellationToken cancellationToken = default)
    {
        var receta = await _productoRepository.ObtenerRecetaAsync(varianteVendidaId, recetaId, cancellationToken);
        if (receta is null)
        {
            return false;
        }

        _productoRepository.EliminarReceta(receta);
        await _productoRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ================= Orígenes de porción =================

    public async Task<IReadOnlyList<OrigenPorcionDto>> ObtenerOrigenesAsync(Guid porcionVarianteId, CancellationToken cancellationToken = default)
    {
        var variante = await _variantes.GetByIdAsync(porcionVarianteId, cancellationToken);
        if (variante is null || variante.IsDeleted)
        {
            throw new NoEncontradoException($"No existe la variante {porcionVarianteId}.");
        }

        var origenes = await _porciones.ObtenerOrigenesAsync(porcionVarianteId, cancellationToken);
        return origenes.Select(MapearOrigen).ToList();
    }

    public async Task<IReadOnlyList<OrigenPorcionDto>> ReemplazarOrigenesAsync(
        Guid porcionVarianteId,
        IReadOnlyList<OrigenPorcionCrearDto> origenes,
        CancellationToken cancellationToken = default)
    {
        var porcion = await _inventario.ObtenerVarianteConRecetasAsync(porcionVarianteId, cancellationToken);
        if (porcion is null || porcion.IsDeleted)
        {
            throw new NoEncontradoException($"No existe la variante {porcionVarianteId}.");
        }

        if (porcion.Producto.AreaDestino == AreaDestino.Cocina || porcion.Producto.Tipo == TipoProducto.Preparado)
        {
            throw new ReglaNegocioException("Solo los productos simples de Barra pueden tener porciones.");
        }

        if (porcion.EsBase)
        {
            throw new ReglaNegocioException(
                $"'{porcion.Nombre}' es una presentación con existencia propia: no puede ser además una porción. Elige una sola naturaleza.");
        }

        if (origenes.Count > 0 && porcion.RecetasVendidas.Any(r => !r.IsDeleted))
        {
            throw new ReglaNegocioException(
                $"'{porcion.Nombre}' ya tiene una receta: no puede ser además una porción. Elige una sola naturaleza.");
        }

        var nuevos = new List<PorcionOrigen>();
        var vistos = new HashSet<Guid>();
        foreach (var dto in origenes)
        {
            if (dto.PorcionesPorUnidad < 1)
            {
                throw new ReglaNegocioException("Las porciones por unidad deben ser un número entero mayor que cero.");
            }

            if (dto.PresentacionVarianteId == porcionVarianteId)
            {
                throw new ReglaNegocioException("Una porción no puede salir de sí misma.");
            }

            if (!vistos.Add(dto.PresentacionVarianteId))
            {
                throw new ReglaNegocioException("Una presentación solo puede aparecer una vez como origen.");
            }

            var presentacion = await _inventario.ObtenerVarianteConRecetasAsync(dto.PresentacionVarianteId, cancellationToken);
            if (presentacion is null || presentacion.IsDeleted)
            {
                throw new NoEncontradoException($"No existe la presentación {dto.PresentacionVarianteId}.");
            }

            if (presentacion.ProductoId != porcion.ProductoId)
            {
                throw new ReglaNegocioException(
                    $"'{presentacion.Nombre}' pertenece a otro producto: el origen debe ser una presentación del mismo producto.");
            }

            if (!presentacion.Activo || !presentacion.EsBase)
            {
                throw new ReglaNegocioException(
                    $"'{presentacion.Nombre}' no es una presentación activa con existencia propia.");
            }

            nuevos.Add(new PorcionOrigen
            {
                PorcionVarianteId = porcionVarianteId,
                PresentacionVarianteId = presentacion.Id,
                PresentacionVariante = presentacion,
                PorcionesPorUnidad = dto.PorcionesPorUnidad
            });
        }

        // No se puede quitar un origen del que hay un envase abierto sirviéndose.
        var actuales = await _porciones.ObtenerOrigenesAsync(porcionVarianteId, cancellationToken);
        var quitados = actuales.Select(o => o.PresentacionVarianteId).Except(vistos).ToList();
        if (quitados.Count > 0)
        {
            var abiertos = await _porciones.ObtenerAbiertosAsync(porcionVarianteId, cancellationToken);
            if (abiertos.Any(e => quitados.Contains(e.PresentacionVarianteId)))
            {
                throw new ConflictoException(
                    "Hay envases abiertos de un origen que intentas quitar: ciérralos o descártalos primero.");
            }
        }

        await _porciones.ReemplazarOrigenesAsync(porcionVarianteId, nuevos, cancellationToken);

        // La fila de reservas de la porción se crea al definirla: así la primera reserva simultánea no compite
        // por insertarla.
        if (nuevos.Count > 0 && await _porciones.ObtenerStockPorcionAsync(porcionVarianteId, cancellationToken) is null)
        {
            await _porciones.AgregarStockPorcionAsync(new StockPorcion { VarianteId = porcionVarianteId }, cancellationToken);
        }

        await _porciones.SaveChangesAsync(cancellationToken);

        return nuevos.Select(MapearOrigen).ToList();
    }

    private static OrigenPorcionDto MapearOrigen(PorcionOrigen o)
        => new(o.Id, o.PresentacionVarianteId, o.PresentacionVariante?.Nombre ?? string.Empty, o.PresentacionVariante?.Sku ?? string.Empty, o.PorcionesPorUnidad);

    // ================= Modificadores / extras =================

    public async Task<IReadOnlyList<ModificadorDto>> ObtenerModificadoresAsync(CancellationToken cancellationToken = default)
    {
        var modificadores = await _modificadorRepository.GetAllAsync(cancellationToken);
        return modificadores.Where(m => !m.IsDeleted).Select(MapearModificador).ToList();
    }

    public async Task<ModificadorDto> CrearModificadorAsync(ModificadorCrearDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.PrecioAdicional < 0)
        {
            throw new ReglaNegocioException("El precio adicional no puede ser negativo.");
        }

        var modificador = new Modificador
        {
            Nombre = dto.Nombre,
            PrecioAdicional = dto.PrecioAdicional,
            Activo = dto.Activo
        };

        await _modificadorRepository.AddAsync(modificador, cancellationToken);
        await _modificadorRepository.SaveChangesAsync(cancellationToken);
        return MapearModificador(modificador);
    }

    public async Task<ModificadorDto?> EditarModificadorAsync(ModificadorEditarDto dto, CancellationToken cancellationToken = default)
    {
        var modificador = await _modificadorRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (modificador is null || modificador.IsDeleted)
        {
            return null;
        }

        modificador.Actualizar(dto.Nombre, dto.PrecioAdicional, dto.Activo);
        _modificadorRepository.Update(modificador);
        await _modificadorRepository.SaveChangesAsync(cancellationToken);
        return MapearModificador(modificador);
    }

    public async Task<bool> EliminarModificadorAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var modificador = await _modificadorRepository.GetByIdAsync(id, cancellationToken);
        if (modificador is null || modificador.IsDeleted)
        {
            return false;
        }

        modificador.EliminarLogico();
        _modificadorRepository.Update(modificador);
        await _modificadorRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<ProductoModificadorDto>> ObtenerModificadoresProductoAsync(Guid productoId, CancellationToken cancellationToken = default)
    {
        var asignaciones = await _productoModificadorRepository.FindAsync(
            pm => !pm.IsDeleted && pm.ProductoId == productoId,
            cancellationToken);

        if (asignaciones.Count == 0)
        {
            return [];
        }

        var modificadores = (await _modificadorRepository.GetAllAsync(cancellationToken))
            .ToDictionary(m => m.Id);

        return asignaciones
            .Where(pm => modificadores.ContainsKey(pm.ModificadorId))
            .Select(pm =>
            {
                var modificador = modificadores[pm.ModificadorId];
                return new ProductoModificadorDto(
                    pm.Id,
                    modificador.Id,
                    modificador.Nombre,
                    modificador.PrecioAdicional,
                    pm.Minimo,
                    pm.Maximo,
                    pm.Requerido);
            })
            .ToList();
    }

    public async Task<ProductoModificadorDto> AsignarModificadorAsync(Guid productoId, AsignarModificadorDto dto, CancellationToken cancellationToken = default)
    {
        var producto = await _productoRepository.GetByIdAsync(productoId, cancellationToken);
        if (producto is null || producto.IsDeleted)
        {
            throw new NoEncontradoException($"No existe el producto {productoId}.");
        }

        var modificador = await _modificadorRepository.GetByIdAsync(dto.ModificadorId, cancellationToken);
        if (modificador is null || modificador.IsDeleted)
        {
            throw new NoEncontradoException($"No existe el modificador {dto.ModificadorId}.");
        }

        if (dto.Minimo < 0 || dto.Maximo < 1 || dto.Maximo < dto.Minimo)
        {
            throw new ReglaNegocioException("Los límites de selección del modificador no son válidos.");
        }

        var existentes = await _productoModificadorRepository.FindAsync(
            pm => !pm.IsDeleted && pm.ProductoId == productoId && pm.ModificadorId == dto.ModificadorId,
            cancellationToken);

        if (existentes.Count > 0)
        {
            throw new ConflictoException($"El modificador '{modificador.Nombre}' ya está asignado al producto.");
        }

        var asignacion = new ProductoModificador
        {
            ProductoId = productoId,
            ModificadorId = dto.ModificadorId,
            Minimo = dto.Minimo,
            Maximo = dto.Maximo,
            Requerido = dto.Requerido
        };

        await _productoModificadorRepository.AddAsync(asignacion, cancellationToken);
        await _productoModificadorRepository.SaveChangesAsync(cancellationToken);

        return new ProductoModificadorDto(
            asignacion.Id,
            modificador.Id,
            modificador.Nombre,
            modificador.PrecioAdicional,
            asignacion.Minimo,
            asignacion.Maximo,
            asignacion.Requerido);
    }

    public async Task<bool> QuitarModificadorAsync(Guid productoId, Guid productoModificadorId, CancellationToken cancellationToken = default)
    {
        var asignacion = await _productoModificadorRepository.GetByIdAsync(productoModificadorId, cancellationToken);
        if (asignacion is null || asignacion.IsDeleted || asignacion.ProductoId != productoId)
        {
            return false;
        }

        asignacion.EliminarLogico();
        _productoModificadorRepository.Update(asignacion);
        await _productoModificadorRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ================= Auxiliares =================

    private async Task ValidarCategoriaAsync(Guid categoriaId, CancellationToken cancellationToken)
    {
        var categoria = await _categoriaRepository.GetByIdAsync(categoriaId, cancellationToken);
        if (categoria is null || categoria.IsDeleted)
        {
            throw new NoEncontradoException($"No existe la categoría {categoriaId}.");
        }
    }

    /// <summary>
    /// Valida la coherencia del modelo por área: la Cocina se vende sin stock ni
    /// recetas (presentaciones Simple), mientras que la Barra simple exige al menos
    /// una presentación base para poder ordenarse al proveedor.
    /// </summary>
    private static void ValidarConsistenciaArea(
        TipoProducto tipo,
        AreaDestino areaDestino,
        IReadOnlyList<VarianteCrearDto> variantes)
    {
        if (areaDestino == AreaDestino.Cocina)
        {
            if (tipo != TipoProducto.Simple)
            {
                throw new ReglaNegocioException("Los productos de Cocina deben ser Simple: no llevan receta ni stock, solo se venden.");
            }

            if (variantes.Any(v => v.EsBase))
            {
                throw new ReglaNegocioException("Los productos de Cocina no se ordenan al proveedor; quita la presentación base.");
            }

            if (variantes.Any(v =>
                (v.StockInicial ?? 0m) > 0 ||
                (v.StockMinimo ?? 0m) > 0 ||
                (v.StockMaximo ?? 0m) > 0))
            {
                throw new ReglaNegocioException("Los productos de Cocina no llevan stock; solo se venden.");
            }

            return;
        }

        // Barra: un producto simple se vende como unidad y se repone al proveedor,
        // por lo que necesita al menos una presentación base.
        if (tipo == TipoProducto.Simple && !variantes.Any(v => v.EsBase))
        {
            throw new ReglaNegocioException(
                "El producto de Barra simple debe tener al menos una presentación base activa para ordenarlo al proveedor.");
        }

        // Barra: un preparado se elabora con insumos, nunca se compra terminado.
        if (tipo == TipoProducto.Preparado)
        {
            if (variantes.Any(v => v.EsBase))
            {
                throw new ReglaNegocioException(
                    "Los productos preparados no se ordenan al proveedor: quita la presentación base y asigna la receta de consumo.");
            }

            if (variantes.Any(v =>
                (v.StockInicial ?? 0m) > 0 ||
                (v.StockMinimo ?? 0m) > 0 ||
                (v.StockMaximo ?? 0m) > 0))
            {
                throw new ReglaNegocioException(
                    "Los productos preparados no llevan stock propio: al vender consumen sus insumos según la receta.");
            }
        }
    }

    private async Task ValidarSkusYCodigosAsync(
        IReadOnlyList<VarianteCrearDto> variantes,
        Guid? productoId,
        CancellationToken cancellationToken)
    {
        var skusVistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var codigosVistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var variante in variantes)
        {
            if (!skusVistos.Add(variante.Sku))
            {
                throw new ConflictoException($"El SKU '{variante.Sku}' está repetido en la petición.");
            }

            if (await _productoRepository.ExisteSkuAsync(variante.Sku, variante.Id, cancellationToken))
            {
                throw new ConflictoException($"Ya existe una variante con el SKU '{variante.Sku}'.");
            }

            foreach (var codigo in variante.CodigosBarras ?? [])
            {
                if (!codigosVistos.Add(codigo))
                {
                    throw new ConflictoException($"El código de barras '{codigo}' está repetido en la petición.");
                }

                if (await _productoRepository.ExisteCodigoBarrasAsync(codigo, variante.Id, cancellationToken))
                {
                    throw new ConflictoException($"Ya existe una variante con el código de barras '{codigo}'.");
                }
            }
        }
    }

    private static ProductoVariante CrearVariante(VarianteCrearDto dto)
    {
        var variante = new ProductoVariante
        {
            Nombre = dto.Nombre,
            Sku = dto.Sku,
            UnidadMedidaId = dto.UnidadMedidaId,
            PrecioCompraUSD = dto.PrecioCompraUSD,
            PrecioVentaUSD = dto.PrecioVentaUSD,
            EsBase = dto.EsBase,
            Activo = true
        };

        foreach (var codigo in dto.CodigosBarras ?? [])
        {
            variante.CodigosBarras.Add(new CodigoBarras { Codigo = codigo, Principal = false });
        }

        return variante;
    }

    private async Task SincronizarVariantesAsync(Producto producto, IReadOnlyList<VarianteCrearDto> variantes, CancellationToken cancellationToken)
    {
        var existentes = producto.Variantes.ToDictionary(v => v.Id);

        foreach (var dto in variantes)
        {
            if (dto.Id is Guid id && existentes.TryGetValue(id, out var variante))
            {
                if (variante.EsBase && !dto.EsBase)
                {
                    await ExigirSinVinculosDePorcionAsync(variante, "dejar de ser presentación", cancellationToken);
                }

                if (dto.EsBase && !variante.EsBase && await _porciones.TieneOrigenesAsync(variante.Id, cancellationToken))
                {
                    throw new ReglaNegocioException(
                        $"'{variante.Nombre}' es una porción: quita sus orígenes antes de convertirla en presentación con existencia propia.");
                }

                variante.ActualizarDatos(dto.Nombre, dto.Sku, dto.UnidadMedidaId, dto.PrecioCompraUSD, dto.PrecioVentaUSD);
                variante.EsBase = dto.EsBase;
                variante.Activar();
                existentes.Remove(id);

                variante.CodigosBarras.Clear();
                foreach (var codigo in dto.CodigosBarras ?? [])
                {
                    await _productoRepository.AgregarCodigoBarrasAsync(
                        new CodigoBarras { VarianteId = variante.Id, Codigo = codigo, Principal = false },
                        cancellationToken);
                }

                await AsegurarStockAsync(variante.Id, dto.EsBase, null, dto.StockMinimo, dto.StockMaximo, cancellationToken);
            }
            else
            {
                var nueva = CrearVariante(dto);
                nueva.ProductoId = producto.Id;
                await _productoRepository.AgregarVarianteAsync(nueva, cancellationToken);
                await AsegurarStockAsync(nueva.Id, nueva.EsBase, dto.StockInicial, dto.StockMinimo, dto.StockMaximo, cancellationToken);
            }
        }

        foreach (var sobrante in existentes.Values)
        {
            await ExigirSinVinculosDePorcionAsync(sobrante, "eliminarla", cancellationToken);
            sobrante.Desactivar();
        }
    }

    /// <summary>
    /// Una presentación que es origen de una porción, o con envases abiertos, no puede eliminarse ni
    /// dejar de ser presentación sin retirar antes esos vínculos o cerrar los envases.
    /// </summary>
    private async Task ExigirSinVinculosDePorcionAsync(ProductoVariante variante, string accion, CancellationToken cancellationToken)
    {
        if (await _porciones.EsOrigenAsync(variante.Id, cancellationToken))
        {
            throw new ConflictoException(
                $"'{variante.Nombre}' es origen de una porción: quita ese vínculo antes de {accion}.");
        }

        if (await _porciones.HayEnvasesAbiertosAsync(variante.Id, cancellationToken))
        {
            throw new ConflictoException(
                $"'{variante.Nombre}' tiene envases abiertos: ciérralos o descártalos antes de {accion}.");
        }
    }

    private static CategoriaDto MapearCategoria(Categoria c)
        => new(c.Id, c.Nombre, c.Descripcion, c.CategoriaPadreId, c.Activo);

    private static MarcaDto MapearMarca(Marca m)
        => new(m.Id, m.Nombre, m.Descripcion, m.Activo);

    private static ListaPrecioDto MapearListaPrecio(ListaPrecio l)
        => new(l.Id, l.Nombre, l.Descripcion, l.EsPredeterminada, l.Activo);

    private static ModificadorDto MapearModificador(Modificador m)
        => new(m.Id, m.Nombre, m.PrecioAdicional, m.Activo);

    private async Task<ProductoDto> MapearProductoAsync(Producto p, CancellationToken cancellationToken = default)
    {
        var varianteIds = new List<Guid>();
        foreach (var variante in p.Variantes)
        {
            if (variante.Activo && !variante.IsDeleted) varianteIds.Add(variante.Id);
        }

        var porVariante = (await _inventario.ObtenerStockDeVariantesAsync(varianteIds, cancellationToken))
            .ToDictionary(s => s.VarianteId);
        var porciones = await CargarDatosPorcionAsync(varianteIds, cancellationToken);
        return MapearProducto(p, porVariante, porciones);
    }

    /// <summary>Datos de porciones de un lote de variantes, para no consultar variante por variante.</summary>
    private sealed record DatosPorcion(
        ILookup<Guid, PorcionOrigen> OrigenesPorPorcion,
        IReadOnlyDictionary<Guid, ResumenPorcion> Resumen,
        IReadOnlySet<Guid> ConReceta);

    private async Task<DatosPorcion> CargarDatosPorcionAsync(IReadOnlyList<Guid> varianteIds, CancellationToken cancellationToken)
    {
        var origenes = await _porciones.ObtenerOrigenesDeVariantesAsync(varianteIds, cancellationToken);
        var porcionIds = origenes.Select(o => o.PorcionVarianteId).Distinct().ToList();
        var resumen = porcionIds.Count == 0
            ? new Dictionary<Guid, ResumenPorcion>()
            : (await _porciones.ObtenerResumenAsync(porcionIds, cancellationToken)).ToDictionary(x => x.Key, x => x.Value);
        var conReceta = await _porciones.ObtenerIdsConRecetaAsync(varianteIds, cancellationToken);
        return new DatosPorcion(origenes.ToLookup(o => o.PorcionVarianteId), resumen, conReceta);
    }

    private static ProductoDto MapearProducto(
        Producto p,
        IReadOnlyDictionary<Guid, StockProducto> porVariante,
        DatosPorcion porciones)
        => new(
            p.Id,
            p.Nombre,
            p.Descripcion,
            p.CategoriaId,
            p.Categoria?.Nombre ?? string.Empty,
            p.MarcaId,
            p.Marca?.Nombre,
            p.ImpuestoId,
            p.Tipo,
            p.AreaDestino,
            p.GradoAlcoholico,
            p.ImagenUrl,
            p.Activo,
            p.Variantes.Where(v => v.Activo && !v.IsDeleted).Select(v =>
            {
                var stock = porVariante.GetValueOrDefault(v.Id);
                var origenes = porciones.OrigenesPorPorcion[v.Id].ToList();
                var esPorcion = origenes.Count > 0;
                var naturaleza = esPorcion ? NaturalezaVariante.Porcion
                    : v.EsBase ? NaturalezaVariante.Presentacion
                    : porciones.ConReceta.Contains(v.Id) ? NaturalezaVariante.Receta
                    : p.AreaDestino == AreaDestino.Cocina ? NaturalezaVariante.Directa
                    : NaturalezaVariante.SinDefinir;

                var abiertas = esPorcion ? porciones.Resumen.GetValueOrDefault(v.Id) : null;
                var disponibles = abiertas is null ? 0 : Math.Max(0, abiertas.PorcionesAbiertas - abiertas.PorcionesReservadas);

                // Presentaciones cerradas que se pueden destapar para esta porción y las porciones que darían.
                var cerradas = 0;
                var potenciales = 0;
                foreach (var origen in origenes)
                {
                    var existencia = porVariante.GetValueOrDefault(origen.PresentacionVarianteId);
                    var libres = existencia is null ? 0 : (int)Math.Max(0m, Math.Floor(existencia.Disponible));
                    cerradas += libres;
                    potenciales += libres * origen.PorcionesPorUnidad;
                }

                return new ProductoVarianteDto(
                    v.Id,
                    v.Nombre,
                    v.Sku,
                    v.PrecioCompraUSD,
                    v.PrecioVentaUSD,
                    v.UnidadMedidaId,
                    v.UnidadMedida?.Nombre ?? string.Empty,
                    v.Activo,
                    v.EsBase,
                    v.CodigosBarras.Select(c => c.Codigo).ToList(),
                    stock?.Cantidad ?? 0m,
                    stock?.CantidadReservada ?? 0m,
                    stock?.StockMinimo ?? 0m,
                    stock?.StockMaximo ?? 0m,
                    naturaleza,
                    disponibles,
                    potenciales,
                    cerradas);
            }).ToList());

    /// <summary>
    /// Asegura la fila de existencias de una variante y aplica su stock inicial (kardex).
    /// Las presentaciones base siempre guardan fila; las demás solo si aportan valores de
    /// stock (inicial o mín/máx), pues una variante con receta consume de sus insumos.
    /// </summary>
    private async Task AsegurarStockAsync(
        Guid varianteId,
        bool esBase,
        decimal? stockInicial,
        decimal? stockMinimo,
        decimal? stockMaximo,
        CancellationToken cancellationToken = default)
    {
        var inicial = stockInicial ?? 0m;
        var hayMinimo = stockMinimo is not null && stockMinimo.Value > 0;
        var hayMaximo = stockMaximo is not null && stockMaximo.Value > 0;
        if (!esBase && inicial <= 0 && !hayMinimo && !hayMaximo)
        {
            return;
        }

        var stock = await _inventario.ObtenerStockAsync(varianteId, cancellationToken);
        if (stock is null)
        {
            stock = new StockProducto { VarianteId = varianteId };
            await _inventario.AgregarStockAsync(stock, cancellationToken);
        }

        if (stockMinimo != null) stock.StockMinimo = stockMinimo.Value;
        if (stockMaximo != null) stock.StockMaximo = stockMaximo.Value;

        if (inicial > 0)
        {
            stock.AplicarMovimiento(inicial);
            await _inventario.AgregarMovimientoAsync(new MovimientoInventario
            {
                VarianteId = varianteId,
                Tipo = TipoMovimientoInventario.Ajuste,
                Cantidad = inicial,
                ReferenciaTipo = "ajuste",
                Motivo = "Stock inicial"
            }, cancellationToken);
        }

        await _inventario.SaveChangesAsync(cancellationToken);
    }
}
