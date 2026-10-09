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
        IInventarioRepository inventario)
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

        var items = pagina.Items.Select(p => MapearProducto(p, porVariante)).ToList();
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

        _productoRepository.Update(producto);
        await _productoRepository.SaveChangesAsync(cancellationToken);

        var actualizado = await _productoRepository.ObtenerConDetalleAsync(producto.Id, cancellationToken);
        return await MapearProductoAsync(actualizado!, cancellationToken);
    }

    public async Task<bool> EliminarProductoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var producto = await _productoRepository.GetByIdAsync(id, cancellationToken);
        if (producto is null || producto.IsDeleted)
        {
            return false;
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
        var stockInsumo = await _inventario.ObtenerStockAsync(insumo.Id, cancellationToken);
        if (!insumo.EsBase && stockInsumo is null)
        {
            throw new ReglaNegocioException(
                $"El insumo '{insumo.Nombre}' no guarda existencia propia; usa una presentación base o con stock.");
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
                variante.ActualizarDatos(dto.Nombre, dto.Sku, dto.UnidadMedidaId, dto.PrecioCompraUSD, dto.PrecioVentaUSD);
                variante.EsBase = dto.EsBase;
                variante.Activar();
                existentes.Remove(id);

                variante.CodigosBarras.Clear();
                foreach (var codigo in dto.CodigosBarras ?? [])
                {
                    variante.CodigosBarras.Add(new CodigoBarras { Codigo = codigo, Principal = false });
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
            sobrante.Desactivar();
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
        return MapearProducto(p, porVariante);
    }

    private static ProductoDto MapearProducto(Producto p, IReadOnlyDictionary<Guid, StockProducto> porVariante)
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
                    stock?.StockMaximo ?? 0m);
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
