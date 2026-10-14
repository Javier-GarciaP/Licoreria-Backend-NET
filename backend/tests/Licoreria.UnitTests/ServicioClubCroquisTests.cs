using System.Linq.Expressions;
using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Services;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>Croquis de fondo, duplicado y plantillas de plano en <see cref="ServicioClub"/>.</summary>
public sealed class ServicioClubCroquisTests
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];

    private readonly Mock<IRepository<Plano>> _planos = new();
    private readonly Mock<IRepository<PlanoElemento>> _elementos = new();
    private readonly Mock<IAlmacenamientoArchivos> _almacenamiento = new();
    private readonly List<Plano> _todos = [];
    private readonly List<PlanoElemento> _todosElementos = [];

    public ServicioClubCroquisTests()
    {
        // Los repositorios aplican de verdad el filtro recibido sobre las listas en memoria.
        _planos
            .Setup(p => p.FindAsync(It.IsAny<Expression<Func<Plano, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<Plano, bool>> f, CancellationToken _) => _todos.Where(f.Compile()).ToList());
        _planos
            .Setup(p => p.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => _todos.FirstOrDefault(p => p.Id == id));
        _elementos
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<PlanoElemento, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<PlanoElemento, bool>> f, CancellationToken _) => _todosElementos.Where(f.Compile()).ToList());
        _planos
            .Setup(p => p.AddAsync(It.IsAny<Plano>(), It.IsAny<CancellationToken>()))
            .Callback((Plano p, CancellationToken _) =>
            {
                _todos.Add(p);
                foreach (var e in p.Elementos)
                {
                    e.PlanoId = p.Id;
                    _todosElementos.Add(e);
                }
            })
            .Returns(Task.CompletedTask);
        _almacenamiento
            .Setup(a => a.GuardarAsync(It.IsAny<Stream>(), It.IsAny<string>(), "planos", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ArchivoAlmacenado("planos/nuevo.png", "/uploads/planos/nuevo.png", "a.png", 12));
        _almacenamiento
            .Setup(a => a.CopiarAsync("planos/original.png", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ArchivoAlmacenado("planos/copia.png", "/uploads/planos/copia.png", "original.png", 12));
    }

    private ServicioClub Crear()
        => new(
            Mock.Of<IRepository<Zona>>(),
            Mock.Of<IRepository<Mesa>>(),
            _planos.Object,
            _elementos.Object,
            Mock.Of<IRepository<Evento>>(),
            Mock.Of<IRepository<ListaVip>>(),
            Mock.Of<IRepository<Entrada>>(),
            Mock.Of<IRepository<PedidoAnticipado>>(),
            Mock.Of<IRepository<ProductoVariante>>(),
            Mock.Of<IReservaRepository>(),
            Mock.Of<ICuentaRepository>(),
            new DetectorConflictosReserva(),
            Mock.Of<IRelojSistema>(),
            Mock.Of<INotificadorComandas>(),
            Mock.Of<IServicioKardex>(),
            Mock.Of<IUnitOfWork>(),
            new JornadaOperativa(),
            _almacenamiento.Object);

    private Plano Plano(string nombre = "Salón", bool activo = false, bool plantilla = false, string? fondo = null, int elementos = 0, Guid? mesaId = null)
    {
        var plano = new Plano { Nombre = nombre, Activo = activo, EsPlantilla = plantilla, FondoImagenUrl = fondo, FondoOpacidad = 0.3m, FondoAjuste = "cubrir" };
        for (var i = 0; i < elementos; i++)
        {
            var e = new PlanoElemento { PlanoId = plano.Id, Tipo = "mesa", PosX = i, MesaId = mesaId, ZonaId = Guid.NewGuid() };
            plano.Elementos.Add(e);
            _todosElementos.Add(e);
        }

        _todos.Add(plano);
        return plano;
    }

    private static MemoryStream ImagenPng() => new(Png);

    // ================= Fondo =================

    [Fact]
    public async Task AsignarFondo_Correcto_GuardaYDevuelveLaDireccion()
    {
        var plano = Plano();

        var resultado = await Crear().AsignarFondoPlanoAsync(plano.Id, ImagenPng(), "a.png", "image/png", Png.Length);

        Assert.Equal("/uploads/planos/nuevo.png", resultado!.FondoImagenUrl);
        Assert.Equal("/uploads/planos/nuevo.png", plano.FondoImagenUrl);
        _almacenamiento.Verify(a => a.EliminarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AsignarFondo_PlanoInexistente_DevuelveNullSinGuardarArchivo()
    {
        var resultado = await Crear().AsignarFondoPlanoAsync(Guid.NewGuid(), ImagenPng(), "a.png", "image/png", Png.Length);

        Assert.Null(resultado);
        _almacenamiento.Verify(a => a.GuardarAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AsignarFondo_ArchivoDisfrazado_LanzaArgumentExceptionSinGuardar()
    {
        var plano = Plano();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            Crear().AsignarFondoPlanoAsync(plano.Id, new MemoryStream("<html>"u8.ToArray()), "a.png", "image/png", 6));

        _almacenamiento.Verify(a => a.GuardarAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Null(plano.FondoImagenUrl);
    }

    [Fact]
    public async Task AsignarFondo_Reemplazo_EliminaElArchivoAnterior()
    {
        var plano = Plano(fondo: "/uploads/planos/viejo.png");

        await Crear().AsignarFondoPlanoAsync(plano.Id, ImagenPng(), "a.png", "image/png", Png.Length);

        Assert.Equal("/uploads/planos/nuevo.png", plano.FondoImagenUrl);
        _almacenamiento.Verify(a => a.EliminarAsync("planos/viejo.png", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task QuitarFondo_DejaElPlanoSinImagenYBorraElArchivo()
    {
        var plano = Plano(fondo: "/uploads/planos/viejo.png");

        var resultado = await Crear().QuitarFondoPlanoAsync(plano.Id);

        Assert.Null(resultado!.FondoImagenUrl);
        _almacenamiento.Verify(a => a.EliminarAsync("planos/viejo.png", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EliminarPlano_ConFondo_BorraElArchivo()
    {
        var plano = Plano(fondo: "/uploads/planos/viejo.png");

        Assert.True(await Crear().EliminarPlanoAsync(plano.Id));

        _almacenamiento.Verify(a => a.EliminarAsync("planos/viejo.png", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EliminarPlano_SiFallaElBorradoDelArchivo_IgualEliminaElPlano()
    {
        var plano = Plano(fondo: "/uploads/planos/viejo.png");
        _almacenamiento
            .Setup(a => a.EliminarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("bloqueado"));

        Assert.True(await Crear().EliminarPlanoAsync(plano.Id));
        Assert.True(plano.IsDeleted);
    }

    [Fact]
    public async Task EditarPlano_NoCambiaLaDireccionDelFondo()
    {
        var plano = Plano(activo: false, fondo: "/uploads/planos/viejo.png");

        var resultado = await Crear().EditarPlanoAsync(
            new PlanoEditarDto(plano.Id, "Nuevo", false, [], FondoOpacidad: 0.3m, FondoAjuste: "estirar"));

        Assert.Equal("/uploads/planos/viejo.png", resultado!.FondoImagenUrl);
        Assert.Equal(0.3m, resultado.FondoOpacidad);
        Assert.Equal("estirar", resultado.FondoAjuste);
    }

    // ================= Duplicar =================

    [Fact]
    public async Task Duplicar_CopiaElementosConIdNuevoYSuPropioFondo()
    {
        var original = Plano("Salón", fondo: "/uploads/planos/original.png", elementos: 12);

        var copia = await Crear().DuplicarPlanoAsync(original.Id, new PlanoDuplicarDto("Evento Halloween"));

        Assert.Equal("Evento Halloween", copia!.Nombre);
        Assert.Equal(12, copia.Elementos.Count);
        Assert.Empty(copia.Elementos.Select(e => e.Id).Intersect(original.Elementos.Select(e => e.Id)));
        Assert.Equal("/uploads/planos/copia.png", copia.FondoImagenUrl);
        Assert.Equal(0.3m, copia.FondoOpacidad);
        Assert.Equal("cubrir", copia.FondoAjuste);
        Assert.Equal("/uploads/planos/original.png", original.FondoImagenUrl);
    }

    [Fact]
    public async Task Duplicar_SinNombre_UsaElOriginalConCopia()
    {
        var original = Plano("Terraza");

        var copia = await Crear().DuplicarPlanoAsync(original.Id, new PlanoDuplicarDto());

        Assert.Equal("Terraza (copia)", copia!.Nombre);
    }

    [Fact]
    public async Task Duplicar_ElPlanoActivo_NoLoDesactivaYLaCopiaNaceInactiva()
    {
        var activo = Plano("Salón principal", activo: true);

        var copia = await Crear().DuplicarPlanoAsync(activo.Id, new PlanoDuplicarDto());

        Assert.True(activo.Activo);
        Assert.False(copia!.Activo);
        _planos.Verify(p => p.Update(It.IsAny<Plano>()), Times.Never);
    }

    [Fact]
    public async Task Duplicar_PlanoInexistente_DevuelveNull()
        => Assert.Null(await Crear().DuplicarPlanoAsync(Guid.NewGuid(), new PlanoDuplicarDto()));

    [Fact]
    public async Task Duplicar_PlanoANormal_ConservaMesaYZona()
    {
        var mesa = Guid.NewGuid();
        var original = Plano(elementos: 1, mesaId: mesa);

        var copia = await Crear().DuplicarPlanoAsync(original.Id, new PlanoDuplicarDto("Otra"));

        Assert.Equal(mesa, copia!.Elementos[0].MesaId);
        Assert.NotNull(copia.Elementos[0].ZonaId);
        Assert.False(copia.EsPlantilla);
    }

    [Fact]
    public async Task Duplicar_ComoPlantilla_LimpiaLasMesasYConservaLaZona()
    {
        var original = Plano(elementos: 2, mesaId: Guid.NewGuid());

        var plantilla = await Crear().DuplicarPlanoAsync(original.Id, new PlanoDuplicarDto("Terraza base", ComoPlantilla: true));

        Assert.True(plantilla!.EsPlantilla);
        Assert.False(plantilla.Activo);
        Assert.All(plantilla.Elementos, e => Assert.Null(e.MesaId));
        Assert.All(plantilla.Elementos, e => Assert.NotNull(e.ZonaId));
    }

    [Fact]
    public async Task Duplicar_DesdePlantilla_CreaPlanoNormalInactivoSinMesas()
    {
        var plantilla = Plano("Terraza base", plantilla: true, elementos: 2, mesaId: null);

        var nuevo = await Crear().DuplicarPlanoAsync(plantilla.Id, new PlanoDuplicarDto("Viernes"));

        Assert.False(nuevo!.EsPlantilla);
        Assert.False(nuevo.Activo);
        Assert.All(nuevo.Elementos, e => Assert.Null(e.MesaId));
        Assert.True(plantilla.EsPlantilla);
    }

    // ================= Plantillas =================

    [Fact]
    public async Task ObtenerPlanos_SinPermiso_NoIncluyePlantillas()
    {
        Plano("Normal");
        Plano("Base", plantilla: true);

        var publico = await Crear().ObtenerPlanosAsync(false);
        var admin = await Crear().ObtenerPlanosAsync(true);

        Assert.Equal(["Normal"], publico.Select(p => p.Nombre));
        Assert.Equal(2, admin.Count);
    }

    [Fact]
    public async Task ObtenerPlano_PlantillaSinPermiso_DevuelveNull()
    {
        var plantilla = Plano("Base", plantilla: true);

        Assert.Null(await Crear().ObtenerPlanoAsync(plantilla.Id, false));
        Assert.NotNull(await Crear().ObtenerPlanoAsync(plantilla.Id, true));
    }

    [Fact]
    public async Task EditarPlano_ActivarUnaPlantilla_LanzaReglaDeNegocio()
    {
        var plantilla = Plano("Base", plantilla: true);

        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            Crear().EditarPlanoAsync(new PlanoEditarDto(plantilla.Id, "Base", true, [])));
    }
}
