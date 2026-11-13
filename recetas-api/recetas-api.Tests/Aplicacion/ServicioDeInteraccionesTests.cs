using NSubstitute;
using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Servicios;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;
using static Recetas.Api.Tests.Comun.Dobles;

namespace Recetas.Api.Tests.Aplicacion;

public class ServicioDeInteraccionesTests
{
    private readonly IRepositorioDeInteracciones _interacciones = Substitute.For<IRepositorioDeInteracciones>();
    private readonly IRepositorioDeRecetas _recetas = Substitute.For<IRepositorioDeRecetas>();
    private readonly IRepositorioDeCatalogos _catalogos = Substitute.For<IRepositorioDeCatalogos>();

    private ServicioDeInteracciones CreeElServicio(int? elUsuarioId = 3) =>
        new(_interacciones, _recetas, _catalogos, new ControlDeAccesoARecetas(_recetas, Usuario(elUsuarioId)), Usuario(elUsuarioId),
            new OpcionesDeEnlaces("http://spa.test/"));

    [Fact]
    public async Task Dar_me_gusta_a_una_receta_no_publicada_da_404_y_no_escribe()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elEstado: EstadosDeLaReceta.INACTIVA));

        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().DeMeGustaALaReceta(10, CT));

        await _interacciones.DidNotReceiveWithAnyArgs().AgregueElMeGusta(default, default, default);
    }

    [Fact]
    public async Task Dar_me_gusta_devuelve_el_total_actualizado()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta());
        _interacciones.CuenteLosMeGusta(10, CT).Returns(8);

        var elResultado = await CreeElServicio().DeMeGustaALaReceta(10, CT);

        Assert.Equal(new ToggleDto(true, 8), elResultado);
        await _interacciones.Received(1).AgregueElMeGusta(3, 10, CT);
    }

    [Fact]
    public async Task Quitar_el_me_gusta_funciona_aunque_la_receta_ya_no_este_publicada()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elEstado: EstadosDeLaReceta.MODERADA));
        _interacciones.CuenteLosMeGusta(10, CT).Returns(2);

        Assert.Equal(new ToggleDto(false, 2), await CreeElServicio().QuiteElMeGusta(10, CT));
        await _interacciones.Received(1).QuiteElMeGusta(3, 10, CT);
    }

    [Fact]
    public async Task Quitar_el_me_gusta_de_una_receta_inexistente_da_404()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns((RecetaBasica?)null);
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().QuiteElMeGusta(10, CT));
    }

    [Fact]
    public async Task Los_favoritos_exigen_publicada_para_agregar_pero_no_para_quitar()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elEstado: EstadosDeLaReceta.INACTIVA));

        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().AgregueAFavoritos(10, CT));
        await CreeElServicio().QuiteDeFavoritos(10, CT);

        await _interacciones.Received(1).QuiteElFavorito(3, 10, CT);
    }

    [Fact]
    public async Task Los_comentarios_de_una_receta_no_visible_dan_404()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elEstado: EstadosDeLaReceta.BORRADOR));
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio(null).ListeLosComentarios(10, null, null, CT));
    }

    [Fact]
    public async Task Los_comentarios_incluyen_promedio_y_total_de_calificaciones()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta());
        _interacciones.ListeLosComentariosVisibles(10, Arg.Any<SolicitudDePagina>(), CT).Returns(new Paginado<ComentarioDto>([], 1, 12, 0));
        _interacciones.CalculeLasCalificaciones(10, CT).Returns(new EstadisticaDeCalificaciones(4.5m, 6));

        var laPagina = await CreeElServicio(null).ListeLosComentarios(10, null, null, CT);

        Assert.Equal((4.5m, 6), (laPagina.Promedio, laPagina.TotalCalificaciones));
    }

    [Fact]
    public async Task No_se_puede_calificar_la_propia_receta()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elAutorId: 3));

        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio(3).CalifiqueLaReceta(10, new CalificarRequest(5, null), CT));

        await _interacciones.DidNotReceiveWithAnyArgs().GuardeLaCalificacion(default, default, default, default, default);
    }

    [Fact]
    public async Task Calificar_guarda_la_calificacion_y_el_comentario_recortado()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elAutorId: 2));

        await CreeElServicio(3).CalifiqueLaReceta(10, new CalificarRequest(4, "  muy rica "), CT);
        await CreeElServicio(3).CalifiqueLaReceta(10, new CalificarRequest(null, "   solo texto  "), CT);
        await CreeElServicio(3).CalifiqueLaReceta(10, new CalificarRequest(2, "  "), CT);

        await _interacciones.Received(1).GuardeLaCalificacion(3, 10, (short)4, "muy rica", CT);
        await _interacciones.Received(1).GuardeLaCalificacion(3, 10, null, "solo texto", CT);
        await _interacciones.Received(1).GuardeLaCalificacion(3, 10, (short)2, null, CT);
    }

    [Fact]
    public async Task Calificar_una_receta_no_publicada_da_404()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elEstado: EstadosDeLaReceta.MODERADA));
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().CalifiqueLaReceta(10, new CalificarRequest(5, null), CT));
    }

    [Fact]
    public async Task Una_interaccion_de_un_visitante_anonimo_se_registra_sin_usuario()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta());

        await CreeElServicio(null).RegistreLaInteraccion(10, new InteraccionRequest("imprimir"), CT);

        await _interacciones.Received(1).AgregueLaInteraccion(10, null, "IMPRIMIR", CT);
    }

    [Fact]
    public async Task Una_interaccion_de_tipo_desconocido_da_400_sin_registrar()
    {
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().RegistreLaInteraccion(10, new InteraccionRequest("HACKEAR"), CT));

        await _interacciones.DidNotReceiveWithAnyArgs().AgregueLaInteraccion(default, default, default!, default);
    }

    [Fact]
    public async Task Compartir_en_una_red_inexistente_da_400_sin_registrar()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta());
        _catalogos.BusqueLaRedSocial(9, CT).Returns((RedSocialDto?)null);

        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().CompartaLaReceta(10, new CompartirRequest(9), CT));

        await _interacciones.DidNotReceiveWithAnyArgs().AgregueElCompartido(default, default, default, default);
    }

    [Theory]
    [InlineData("Facebook", "https://www.facebook.com/sharer/sharer.php?u=http%3A%2F%2Fspa.test%2Frecetas%2F10")]
    [InlineData("WhatsApp", "https://wa.me/?text=http%3A%2F%2Fspa.test%2Frecetas%2F10")]
    [InlineData("X", "https://twitter.com/intent/tweet?url=http%3A%2F%2Fspa.test%2Frecetas%2F10")]
    [InlineData("Instagram", null)]
    public async Task Compartir_arma_la_url_publica_y_el_enlace_de_cada_red(string laRed, string? elEnlaceEsperado)
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta());
        _catalogos.BusqueLaRedSocial(1, CT).Returns(new RedSocialDto(1, laRed));

        var laRespuesta = await CreeElServicio(null).CompartaLaReceta(10, new CompartirRequest(1), CT);

        Assert.Equal("http://spa.test/recetas/10", laRespuesta.Url);
        Assert.Equal(elEnlaceEsperado, laRespuesta.Enlace);
        await _interacciones.Received(1).AgregueElCompartido(10, null, 1, CT);
    }
}
