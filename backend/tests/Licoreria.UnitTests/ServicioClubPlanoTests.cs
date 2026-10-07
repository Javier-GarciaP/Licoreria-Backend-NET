using System.Linq.Expressions;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Entities;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>
/// Pruebas deterministas de los planos en <see cref="ServicioClub"/>.
/// Cubre el <b>upsert</b> de elementos (actualiza, crea, elimina sin borrar y
/// recrear todo), la <b>carga explícita</b> de elementos en el listado y el
/// <b>plano activo único</b> (activar uno desactiva los demás).
/// </summary>
public sealed class ServicioClubPlanoTests
{
    private readonly Mock<IRepository<Plano>> _planos = new();
    private readonly Mock<IRepository<PlanoElemento>> _elementos = new();

    private ServicioClub CrearServicio()
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
            Mock.Of<IRelojSistema>(),
            Mock.Of<INotificadorComandas>());

    private static Plano PlanoCon(params PlanoElemento[] elementos)
    {
        var plano = new Plano { Nombre = "Planta baja", Activo = true };
        foreach (var elemento in elementos)
        {
            plano.Elementos.Add(elemento);
        }

        return plano;
    }

    private static PlanoElemento Elemento(Guid id, decimal posX = 1m, int z = 0)
        => new PlanoElemento { ZonaId = null, MesaId = null, Tipo = "mesa", PosX = posX, Z = z };

    private static PlanoElementoCrearDto CrearDtoElemento(Guid? id, decimal posX)
        => new(null, "mesa", null, posX, 2m, 2m, 2m, 0, null, null, 0, null, id);

    private static PlanoEditarDto Editar(Plano plano, params PlanoElementoCrearDto[] elementos)
        => new(plano.Id, plano.Nombre, plano.Activo, elementos);

    private static CancellationToken Token() => CancellationToken.None;

    private void SinOtrosPlanos()
        => _planos
            .Setup(p => p.FindAsync(It.IsAny<Expression<Func<Plano, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

    private void HayOtroPlano(Plano otro)
    {
        _planos
            .Setup(p => p.FindAsync(It.IsAny<Expression<Func<Plano, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([otro]);
        _planos
            .Setup(p => p.GetByIdAsync(otro.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otro);
    }

    private void ElementosExistentes(IReadOnlyList<PlanoElemento> elementos)
        => _elementos
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<PlanoElemento, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(elementos);

    // ================= Upsert =================

    [Fact]
    public async Task EditarPlanoAsync_ElementoExistente_ActualizaEnVezDeRecrear()
    {
        var existente = Elemento(Guid.NewGuid(), 1m);
        var plano = PlanoCon(existente);
        _planos.Setup(p => p.GetByIdAsync(plano.Id, It.IsAny<CancellationToken>())).ReturnsAsync(plano);
        _elementos.Setup(r => r.GetByIdAsync(existente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existente);
        SinOtrosPlanos();
        ElementosExistentes([existente]);

        var servicio = CrearServicio();

        var resultado = await servicio.EditarPlanoAsync(Editar(plano, CrearDtoElemento(existente.Id, 5m)), Token());

        Assert.NotNull(resultado);
        Assert.Equal(5m, resultado.Elementos[0].PosX);
        Assert.Equal(existente.Id, resultado.Elementos[0].Id);
        _elementos.Verify(r => r.Update(existente), Times.Once);
        _elementos.Verify(r => r.AddAsync(It.IsAny<PlanoElemento>(), It.IsAny<CancellationToken>()), Times.Never);
        _elementos.Verify(r => r.Remove(It.IsAny<PlanoElemento>()), Times.Never);
        _planos.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EditarPlanoAsync_ElementoSinId_Crea()
    {
        var plano = PlanoCon();
        _planos.Setup(p => p.GetByIdAsync(plano.Id, It.IsAny<CancellationToken>())).ReturnsAsync(plano);
        SinOtrosPlanos();
        ElementosExistentes([]);

        var servicio = CrearServicio();

        var resultado = await servicio.EditarPlanoAsync(Editar(plano, CrearDtoElemento(null, 3m)), Token());

        Assert.NotNull(resultado);
        _elementos.Verify(
            r => r.AddAsync(It.Is<PlanoElemento>(e => e.PlanoId == plano.Id && e.PosX == 3m), It.IsAny<CancellationToken>()),
            Times.Once);
        _elementos.Verify(r => r.Remove(It.IsAny<PlanoElemento>()), Times.Never);
    }

    [Fact]
    public async Task EditarPlanoAsync_ElementoQueYaNoViene_Elimina()
    {
        var conservado = Elemento(Guid.NewGuid(), 1m);
        var eliminado = Elemento(Guid.NewGuid(), 2m);
        var plano = PlanoCon(conservado, eliminado);
        _planos.Setup(p => p.GetByIdAsync(plano.Id, It.IsAny<CancellationToken>())).ReturnsAsync(plano);
        _elementos.Setup(r => r.GetByIdAsync(conservado.Id, It.IsAny<CancellationToken>())).ReturnsAsync(conservado);
        _elementos.Setup(r => r.GetByIdAsync(eliminado.Id, It.IsAny<CancellationToken>())).ReturnsAsync(eliminado);
        SinOtrosPlanos();
        ElementosExistentes([conservado, eliminado]);

        var servicio = CrearServicio();

        var resultado = await servicio.EditarPlanoAsync(Editar(plano, CrearDtoElemento(conservado.Id, 1m)), Token());

        Assert.NotNull(resultado);
        _elementos.Verify(r => r.Update(conservado), Times.Once);
        _elementos.Verify(r => r.Remove(eliminado), Times.Once);
        _elementos.Verify(r => r.AddAsync(It.IsAny<PlanoElemento>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ================= Carga explícita =================

    [Fact]
    public async Task ObtenerPlanosAsync_ConElementos_LosMapeaOrdenadosPorZ()
    {
        var plano = PlanoCon();
        _planos
            .Setup(p => p.FindAsync(It.IsAny<Expression<Func<Plano, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([plano]);
        // Se devuelven desordenados; el mapeo debe ordenarlos por Z.
        var e1 = Elemento(Guid.NewGuid(), 1m, 0);
        e1.PlanoId = plano.Id;
        var e2 = Elemento(Guid.NewGuid(), 2m, 1);
        e2.PlanoId = plano.Id;
        ElementosExistentes([e2, e1]);

        var servicio = CrearServicio();

        var resultado = await servicio.ObtenerPlanosAsync(Token());

        Assert.Single(resultado);
        Assert.Equal(2, resultado[0].Elementos.Count);
        Assert.Equal(1m, resultado[0].Elementos[0].PosX);
        Assert.Equal(2m, resultado[0].Elementos[1].PosX);
    }

    // ================= Plano activo único =================

    [Fact]
    public async Task EditarPlanoAsync_AlActivar_DesactivaLosDemas()
    {
        var plano = PlanoCon();
        var otro = new Plano { Nombre = "Otro", Activo = true };
        _planos.Setup(p => p.GetByIdAsync(plano.Id, It.IsAny<CancellationToken>())).ReturnsAsync(plano);
        HayOtroPlano(otro);
        ElementosExistentes([]);

        var servicio = CrearServicio();

        var resultado = await servicio.EditarPlanoAsync(Editar(plano), Token());

        Assert.NotNull(resultado);
        Assert.True(resultado.Activo);
        Assert.False(otro.Activo);
        _planos.Verify(p => p.Update(otro), Times.Once);
    }

    [Fact]
    public async Task CrearPlanoAsync_Activo_DesactivaLosDemas()
    {
        var otro = new Plano { Nombre = "Otro", Activo = true };
        HayOtroPlano(otro);
        ElementosExistentes([]);

        var servicio = CrearServicio();

        var resultado = await servicio.CrearPlanoAsync(new PlanoCrearDto("Nuevo", []), Token());

        Assert.True(resultado.Activo);
        Assert.False(otro.Activo);
        _planos.Verify(p => p.Update(otro), Times.Once);
        _planos.Verify(
            p => p.AddAsync(It.Is<Plano>(pl => pl.Nombre == "Nuevo" && pl.Activo), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}