using System.Linq.Expressions;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Entities;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>
/// Pruebas deterministas del guardado de planos en <see cref="ServicioClub"/>.
/// Verifica que <c>EditarPlanoAsync</c> haga <b>upsert</b> de los elementos:
/// actualiza los que llegan con Id conocido, crea los nuevos y elimina los que
/// ya no vienen, sin borrar y recrear todo en cada guardado.
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

    private static PlanoElemento Elemento(Guid id, decimal posX = 1m)
        => new PlanoElemento { ZonaId = null, MesaId = null, Tipo = "mesa", PosX = posX };

    private static PlanoElementoCrearDto CrearDtoElemento(Guid? id, decimal posX)
        => new(null, "mesa", null, posX, 2m, 2m, 2m, 0, null, null, 0, null, id);

    private static PlanoEditarDto Editar(Plano plano, params PlanoElementoCrearDto[] elementos)
        => new(plano.Id, plano.Nombre, plano.Activo, elementos);

    private static CancellationToken Token() => CancellationToken.None;

    [Fact]
    public async Task EditarPlanoAsync_ElementoExistente_ActualizaEnVezDeRecrear()
    {
        var existente = Elemento(Guid.NewGuid(), 1m);
        var plano = PlanoCon(existente);
        _planos.Setup(p => p.GetByIdAsync(plano.Id, It.IsAny<CancellationToken>())).ReturnsAsync(plano);
        _elementos
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<PlanoElemento, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([existente]);

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
        _elementos
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<PlanoElemento, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

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
        _elementos
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<PlanoElemento, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([conservado, eliminado]);
        _elementos
            .Setup(r => r.GetByIdAsync(eliminado.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eliminado);

        var servicio = CrearServicio();

        var resultado = await servicio.EditarPlanoAsync(Editar(plano, CrearDtoElemento(conservado.Id, 1m)), Token());

        Assert.NotNull(resultado);
        _elementos.Verify(r => r.Update(conservado), Times.Once);
        _elementos.Verify(r => r.Remove(eliminado), Times.Once);
        _elementos.Verify(r => r.AddAsync(It.IsAny<PlanoElemento>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}