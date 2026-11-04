using NSubstitute;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Servicios;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;
using Recetas.Api.Tests.Comun;
using static Recetas.Api.Tests.Comun.Dobles;

namespace Recetas.Api.Tests.Aplicacion;

public class ServicioDeListasTests
{
    private readonly IRepositorioDeListas _listas = Substitute.For<IRepositorioDeListas>();
    private readonly IRepositorioDeRecetas _recetas = Substitute.For<IRepositorioDeRecetas>();
    private readonly EjecutorSinBaseDeDatos _ejecutor = new();
    private IPuntoDeFalla _falla = Substitute.For<IPuntoDeFalla>();

    private ServicioDeListas CreeElServicio() =>
        new(_listas, new ControlDeAccesoARecetas(_recetas, Usuario(3)), _ejecutor, _falla, Usuario(3));

    private static ListaDetalleDto UnaLista(int elId = 50) => new(elId, "Lista", "ACTIVA", DateTime.UtcNow, []);

    [Fact]
    public async Task Crear_una_lista_de_una_receta_no_publicada_da_404_sin_abrir_transaccion()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elEstado: EstadosDeLaReceta.INACTIVA));

        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().CreeLaLista(new CrearListaRequest(10, 2, null), CT));

        Assert.Equal(0, _ejecutor.Ejecuciones);
    }

    [Fact]
    public async Task Crear_una_lista_de_una_receta_sin_ingredientes_da_409()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta());
        _listas.AgregueLaLista(3, Arg.Any<string>(), CT).Returns(50);
        _listas.AgregueLosItemsDesdeLaReceta(50, 10, 2, CT).Returns(0);

        await FalleCon(TipoDeFalla.Conflicto, () => CreeElServicio().CreeLaLista(new CrearListaRequest(10, 2, null), CT));

        await _listas.DidNotReceiveWithAnyArgs().BusqueElDetalleDeLaLista(default, default, default);
    }

    [Fact]
    public async Task Crear_una_lista_sin_nombre_usa_el_titulo_de_la_receta_y_devuelve_su_detalle()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta());
        _listas.AgregueLaLista(3, "Lista de Arroz con pollo", CT).Returns(50);
        _listas.AgregueLosItemsDesdeLaReceta(50, 10, 6, CT).Returns(4);
        _listas.BusqueElDetalleDeLaLista(3, 50, CT).Returns(UnaLista());

        var laLista = await CreeElServicio().CreeLaLista(new CrearListaRequest(10, 6, "  "), CT);

        Assert.Equal(50, laLista.ListaId);
        Assert.Equal(1, _ejecutor.Ejecuciones);
    }

    [Fact]
    public async Task El_nombre_de_la_lista_se_normaliza_y_se_limita_a_80_caracteres()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta());
        _listas.AgregueLaLista(3, Arg.Any<string>(), CT).Returns(50);
        _listas.AgregueLosItemsDesdeLaReceta(50, 10, 2, CT).Returns(1);
        _listas.BusqueElDetalleDeLaLista(3, 50, CT).Returns(UnaLista());

        await CreeElServicio().CreeLaLista(new CrearListaRequest(10, 2, "  Compras   de " + new string('x', 100)), CT);

        await _listas.Received(1).AgregueLaLista(3, Arg.Is<string>(n => n.Length == 80 && n.StartsWith("Compras de x")), CT);
    }

    [Fact]
    public async Task Listar_con_un_estado_invalido_da_400_y_con_uno_valido_lo_normaliza()
    {
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().ListeLasListas("cerrada", null, null, CT));

        await CreeElServicio().ListeLasListas(" completada ", null, null, CT);

        await _listas.Received(1).ListeLasListas(3, "COMPLETADA", Arg.Any<Recetas.Api.Aplicacion.Comun.SolicitudDePagina>(), CT);
    }

    [Fact]
    public async Task La_lista_de_otro_usuario_responde_404()
    {
        _listas.BusqueElDetalleDeLaLista(3, 77, CT).Returns((ListaDetalleDto?)null);
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().ObtengaLaLista(77, CT));
    }

    [Fact]
    public async Task Marcar_un_item_de_una_lista_ajena_da_404_sin_abrir_transaccion()
    {
        _listas.ExisteLaListaDelUsuario(3, 77, CT).Returns(false);

        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().MarqueElItem(77, 1, new MarcarItemRequest(true), CT));

        Assert.Equal(0, _ejecutor.Ejecuciones);
    }

    [Fact]
    public async Task Marcar_un_item_que_no_pertenece_a_la_lista_da_404_y_no_recalcula_el_estado()
    {
        _listas.ExisteLaListaDelUsuario(3, 50, CT).Returns(true);
        _listas.MarqueElItem(50, 99, true, CT).Returns(false);

        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().MarqueElItem(50, 99, new MarcarItemRequest(true), CT));

        await _listas.DidNotReceiveWithAnyArgs().EstablezcaElEstadoDeLaLista(default, default!, default);
    }

    [Theory]
    [InlineData(false, EstadosDeLaLista.COMPLETADA)]
    [InlineData(true, EstadosDeLaLista.ACTIVA)]
    public async Task El_estado_de_la_lista_se_recalcula_segun_los_items_pendientes(bool hayPendientes, string elEstadoEsperado)
    {
        _listas.ExisteLaListaDelUsuario(3, 50, CT).Returns(true);
        _listas.MarqueElItem(50, 1, true, CT).Returns(true);
        _listas.HayItemsPendientes(50, CT).Returns(hayPendientes);
        _listas.BusqueElItem(1, CT).Returns(new ListaItemDto(1, 1, "Sal", 1, "g", 1m, null, null, true));

        var elResultado = await CreeElServicio().MarqueElItem(50, 1, new MarcarItemRequest(true), CT);

        Assert.Equal(elEstadoEsperado, elResultado.EstadoLista);
        Received.InOrder(() =>
        {
            _listas.BloqueeLaLista(50, CT);
            _listas.MarqueElItem(50, 1, true, CT);
            _listas.EstablezcaElEstadoDeLaLista(50, elEstadoEsperado, CT);
        });
    }

    [Fact]
    public async Task Si_falla_despues_de_marcar_el_item_no_se_cambia_el_estado_de_la_lista()
    {
        _falla = new FallaEnElPunto("item-actualizado");
        _listas.ExisteLaListaDelUsuario(3, 50, CT).Returns(true);
        _listas.MarqueElItem(50, 1, true, CT).Returns(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreeElServicio().MarqueElItem(50, 1, new MarcarItemRequest(true), CT));

        await _listas.DidNotReceiveWithAnyArgs().EstablezcaElEstadoDeLaLista(default, default!, default);
    }

    [Fact]
    public async Task Borrar_una_lista_ajena_o_inexistente_da_404()
    {
        _listas.BorreLaLista(3, 77, CT).Returns(false);
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().BorreLaLista(77, CT));
    }

    [Fact]
    public async Task Borrar_una_lista_propia_no_lanza()
    {
        _listas.BorreLaLista(3, 50, CT).Returns(true);
        await CreeElServicio().BorreLaLista(50, CT);
    }
}
