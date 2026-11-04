using NSubstitute;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Servicios;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;
using Recetas.Api.Tests.Comun;
using static Recetas.Api.Tests.Comun.Dobles;

namespace Recetas.Api.Tests.Aplicacion;

public class ServicioDeModeracionTests
{
    private readonly IRepositorioDeModeracion _moderacion = Substitute.For<IRepositorioDeModeracion>();
    private readonly IRepositorioDeRecetas _recetas = Substitute.For<IRepositorioDeRecetas>();
    private readonly EjecutorSinBaseDeDatos _ejecutor = new();
    private IPuntoDeFalla _falla = Substitute.For<IPuntoDeFalla>();
    private readonly DateTimeOffset _ahora = new(2026, 11, 3, 15, 0, 0, TimeSpan.Zero);

    private ServicioDeModeracion CreeElServicio(int elUsuarioId = 1) =>
        new(_moderacion, _recetas, _ejecutor, _falla, Usuario(elUsuarioId, true), new RelojFijo(_ahora));

    private static ResolverReporteRequest UnaResolucion(string laDecision) => new(laDecision, "  Contenido inapropiado  ");

    [Fact]
    public async Task No_se_puede_denunciar_una_receta_inexistente_no_publicada_o_propia()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns((RecetaBasica?)null);
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio(3).DenuncieElContenido(new CrearReporteContenidoRequest(10, null, "SPAM", null), CT));

        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elEstado: EstadosDeLaReceta.MODERADA));
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio(3).DenuncieElContenido(new CrearReporteContenidoRequest(10, null, "SPAM", null), CT));

        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elAutorId: 3));
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio(3).DenuncieElContenido(new CrearReporteContenidoRequest(10, null, "SPAM", null), CT));
    }

    [Fact]
    public async Task No_se_puede_denunciar_un_comentario_oculto_inexistente_o_propio()
    {
        _moderacion.BusqueElAutorDelComentarioVisible(7, CT).Returns((int?)null);
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio(3).DenuncieElContenido(new CrearReporteContenidoRequest(null, 7, "SPAM", null), CT));

        _moderacion.BusqueElAutorDelComentarioVisible(7, CT).Returns(3);
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio(3).DenuncieElContenido(new CrearReporteContenidoRequest(null, 7, "SPAM", null), CT));
    }

    [Fact]
    public async Task Dos_denuncias_pendientes_del_mismo_usuario_sobre_lo_mismo_dan_409()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elAutorId: 2));
        _moderacion.ExisteUnaDenunciaPendiente(3, 10, null, CT).Returns(true);

        await FalleCon(TipoDeFalla.Conflicto, () => CreeElServicio(3).DenuncieElContenido(new CrearReporteContenidoRequest(10, null, "SPAM", null), CT));
    }

    [Fact]
    public async Task Denunciar_normaliza_el_motivo_y_la_descripcion()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elAutorId: 2));
        _moderacion.AgregueLaDenuncia(3, 10, null, "SPAM", "es publicidad", CT).Returns(new ReporteCreadoDto(1, "PENDIENTE"));

        var laDenuncia = await CreeElServicio(3).DenuncieElContenido(new CrearReporteContenidoRequest(10, null, " spam ", " es publicidad "), CT);

        Assert.Equal("PENDIENTE", laDenuncia.Estado);
    }

    [Fact]
    public async Task La_cola_usa_pendientes_por_defecto_y_atendidas_para_los_otros_estados()
    {
        await CreeElServicio().ListeLasDenuncias(null, null, null, CT);
        await CreeElServicio().ListeLasDenuncias(" moderado ", null, null, CT);

        await _moderacion.Received(1).ListeLasDenunciasPendientes(Arg.Any<Recetas.Api.Aplicacion.Comun.SolicitudDePagina>(), CT);
        await _moderacion.Received(1).ListeLasDenunciasAtendidas("MODERADO", Arg.Any<Recetas.Api.Aplicacion.Comun.SolicitudDePagina>(), CT);
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().ListeLasDenuncias("archivado", null, null, CT));
    }

    [Fact]
    public async Task El_detalle_de_una_denuncia_inexistente_da_404()
    {
        _moderacion.BusqueElDetalleDeLaDenuncia(9, CT).Returns((ReporteDetalleDto?)null);
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().ObtengaLaDenuncia(9, CT));
    }

    [Fact]
    public async Task Resolver_una_denuncia_inexistente_da_404_y_una_ya_resuelta_da_409()
    {
        _moderacion.BloqueeLaDenunciaYObtengaSuEstado(9, CT).Returns((string?)null);
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().ResuelvaLaDenuncia(9, UnaResolucion("MODERAR"), CT));

        _moderacion.BloqueeLaDenunciaYObtengaSuEstado(8, CT).Returns(EstadosDeLaDenuncia.MODERADO);
        await FalleCon(TipoDeFalla.Conflicto, () => CreeElServicio().ResuelvaLaDenuncia(8, UnaResolucion("MODERAR"), CT));

        await _recetas.DidNotReceiveWithAnyArgs().CambieElEstadoSiEs(default, default!, default!, default);
    }

    [Fact]
    public async Task Mantener_cierra_la_denuncia_sin_retirar_contenido_ni_registrar_bitacora()
    {
        _moderacion.BloqueeLaDenunciaYObtengaSuEstado(5, CT).Returns(EstadosDeLaDenuncia.PENDIENTE);
        _moderacion.BusqueElContenidoDenunciado(5, CT).Returns(new ContenidoDenunciado(10, null));

        var elResultado = await CreeElServicio().ResuelvaLaDenuncia(5, UnaResolucion("mantener"), CT);

        Assert.Equal(new ResolucionDto(5, "MANTENIDO", 0), elResultado);
        await _moderacion.Received(1).MarqueLaDenuncia(5, "MANTENIDO", "Contenido inapropiado", 1, _ahora.UtcDateTime, CT);
        await _recetas.DidNotReceiveWithAnyArgs().CambieElEstadoSiEs(default, default!, default!, default);
        await _moderacion.DidNotReceiveWithAnyArgs().AgregueLaAccionDeModeracion(default, default, default, default, default!, default!, default);
    }

    [Fact]
    public async Task Moderar_una_receta_la_retira_registra_la_bitacora_y_cierra_las_demas_denuncias()
    {
        _moderacion.BloqueeLaDenunciaYObtengaSuEstado(5, CT).Returns(EstadosDeLaDenuncia.PENDIENTE);
        _moderacion.BusqueElContenidoDenunciado(5, CT).Returns(new ContenidoDenunciado(10, null));
        _recetas.CambieElEstadoSiEs(10, EstadosDeLaReceta.PUBLICADA, EstadosDeLaReceta.MODERADA, CT).Returns(true);
        _moderacion.CierreLasDemasDenunciasPendientes(5, Arg.Any<ContenidoDenunciado>(), Arg.Any<string>(), 1, _ahora.UtcDateTime, CT).Returns(2);

        var elResultado = await CreeElServicio().ResuelvaLaDenuncia(5, UnaResolucion("MODERAR"), CT);

        Assert.Equal(new ResolucionDto(5, "MODERADO", 2), elResultado);
        Received.InOrder(() =>
        {
            _moderacion.BloqueeLaDenunciaYObtengaSuEstado(5, CT);
            _recetas.CambieElEstadoSiEs(10, EstadosDeLaReceta.PUBLICADA, EstadosDeLaReceta.MODERADA, CT);
            _moderacion.AgregueLaAccionDeModeracion(1, 10, null, 5, "RETIRAR", "Contenido inapropiado", CT);
            _moderacion.MarqueLaDenuncia(5, "MODERADO", "Contenido inapropiado", 1, _ahora.UtcDateTime, CT);
            _moderacion.CierreLasDemasDenunciasPendientes(5, Arg.Any<ContenidoDenunciado>(), Arg.Any<string>(), 1, _ahora.UtcDateTime, CT);
        });
    }

    [Fact]
    public async Task El_texto_de_las_denuncias_cerradas_en_conjunto_hace_referencia_a_la_original_y_cabe_en_500()
    {
        _moderacion.BloqueeLaDenunciaYObtengaSuEstado(5, CT).Returns(EstadosDeLaDenuncia.PENDIENTE);
        _moderacion.BusqueElContenidoDenunciado(5, CT).Returns(new ContenidoDenunciado(10, null));
        _recetas.CambieElEstadoSiEs(10, Arg.Any<string>(), Arg.Any<string>(), CT).Returns(true);

        await CreeElServicio().ResuelvaLaDenuncia(5, new ResolverReporteRequest("MODERAR", new string('x', 500)), CT);

        await _moderacion.Received(1).CierreLasDemasDenunciasPendientes(5, Arg.Any<ContenidoDenunciado>(),
            Arg.Is<string>(t => t.Length == 500 && t.StartsWith("Resuelta junto con la denuncia #5: ")), 1, _ahora.UtcDateTime, CT);
    }

    [Fact]
    public async Task Moderar_contenido_ya_retirado_solo_cierra_la_denuncia_sin_nueva_accion_en_la_bitacora()
    {
        _moderacion.BloqueeLaDenunciaYObtengaSuEstado(5, CT).Returns(EstadosDeLaDenuncia.PENDIENTE);
        _moderacion.BusqueElContenidoDenunciado(5, CT).Returns(new ContenidoDenunciado(10, null));
        _recetas.CambieElEstadoSiEs(10, Arg.Any<string>(), Arg.Any<string>(), CT).Returns(false);
        _moderacion.LaRecetaNoEstaModerada(10, CT).Returns(false);

        await CreeElServicio().ResuelvaLaDenuncia(5, UnaResolucion("MODERAR"), CT);

        await _moderacion.DidNotReceiveWithAnyArgs().AgregueLaAccionDeModeracion(default, default, default, default, default!, default!, default);
        await _moderacion.Received(1).MarqueLaDenuncia(5, "MODERADO", Arg.Any<string>(), 1, _ahora.UtcDateTime, CT);
    }

    [Fact]
    public async Task Moderar_una_receta_que_ya_no_esta_publicada_ni_moderada_da_409()
    {
        _moderacion.BloqueeLaDenunciaYObtengaSuEstado(5, CT).Returns(EstadosDeLaDenuncia.PENDIENTE);
        _moderacion.BusqueElContenidoDenunciado(5, CT).Returns(new ContenidoDenunciado(10, null));
        _recetas.CambieElEstadoSiEs(10, Arg.Any<string>(), Arg.Any<string>(), CT).Returns(false);
        _moderacion.LaRecetaNoEstaModerada(10, CT).Returns(true);

        await FalleCon(TipoDeFalla.Conflicto, () => CreeElServicio().ResuelvaLaDenuncia(5, UnaResolucion("MODERAR"), CT));
    }

    [Fact]
    public async Task Moderar_un_comentario_lo_oculta_en_lugar_de_tocar_la_receta()
    {
        _moderacion.BloqueeLaDenunciaYObtengaSuEstado(5, CT).Returns(EstadosDeLaDenuncia.PENDIENTE);
        _moderacion.BusqueElContenidoDenunciado(5, CT).Returns(new ContenidoDenunciado(null, 7));
        _moderacion.OculteElComentarioVisible(7, CT).Returns(true);

        await CreeElServicio().ResuelvaLaDenuncia(5, UnaResolucion("MODERAR"), CT);

        await _recetas.DidNotReceiveWithAnyArgs().CambieElEstadoSiEs(default, default!, default!, default);
        await _moderacion.Received(1).AgregueLaAccionDeModeracion(1, null, 7, 5, "RETIRAR", "Contenido inapropiado", CT);
    }

    [Fact]
    public async Task Si_falla_antes_de_la_bitacora_no_se_marca_la_denuncia_como_resuelta()
    {
        _falla = new FallaEnElPunto("antes-de-moderacion");
        _moderacion.BloqueeLaDenunciaYObtengaSuEstado(5, CT).Returns(EstadosDeLaDenuncia.PENDIENTE);
        _moderacion.BusqueElContenidoDenunciado(5, CT).Returns(new ContenidoDenunciado(10, null));
        _recetas.CambieElEstadoSiEs(10, Arg.Any<string>(), Arg.Any<string>(), CT).Returns(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreeElServicio().ResuelvaLaDenuncia(5, UnaResolucion("MODERAR"), CT));

        await _moderacion.DidNotReceiveWithAnyArgs().AgregueLaAccionDeModeracion(default, default, default, default, default!, default!, default);
        await _moderacion.DidNotReceiveWithAnyArgs().MarqueLaDenuncia(default, default!, default!, default, default, default);
    }

    [Fact]
    public async Task Retirar_una_receta_publicada_la_modera_y_registra_la_accion()
    {
        _recetas.CambieElEstadoSiEs(10, EstadosDeLaReceta.PUBLICADA, EstadosDeLaReceta.MODERADA, CT).Returns(true);

        var elResultado = await CreeElServicio().RetireLaReceta(10, new MotivoRequest("  incumple las reglas "), CT);

        Assert.Equal(new EstadoContenidoDto(10, "MODERADA"), elResultado);
        await _moderacion.Received(1).AgregueLaAccionDeModeracion(1, 10, null, null, "RETIRAR", "incumple las reglas", CT);
    }

    [Fact]
    public async Task Restaurar_una_receta_que_no_esta_moderada_da_409_y_si_no_existe_da_404()
    {
        _recetas.CambieElEstadoSiEs(10, EstadosDeLaReceta.MODERADA, EstadosDeLaReceta.PUBLICADA, CT).Returns(false);
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta());
        await FalleCon(TipoDeFalla.Conflicto, () => CreeElServicio().RestaureLaReceta(10, new MotivoRequest("x"), CT));

        _recetas.BusqueLaRecetaBasica(10, CT).Returns((RecetaBasica?)null);
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().RestaureLaReceta(10, new MotivoRequest("x"), CT));

        await _moderacion.DidNotReceiveWithAnyArgs().AgregueLaAccionDeModeracion(default, default, default, default, default!, default!, default);
    }

    [Fact]
    public async Task Retirar_y_restaurar_comentarios_informan_su_visibilidad()
    {
        _moderacion.CambieLaVisibilidadDelComentario(7, true, CT).Returns(true);
        _moderacion.CambieLaVisibilidadDelComentario(7, false, CT).Returns(true);

        Assert.Equal("OCULTO", (await CreeElServicio().RetireElComentario(7, new MotivoRequest("x"), CT)).Estado);
        Assert.Equal("VISIBLE", (await CreeElServicio().RestaureElComentario(7, new MotivoRequest("x"), CT)).Estado);
    }

    [Fact]
    public async Task Retirar_un_comentario_ya_oculto_da_409_y_uno_inexistente_da_404()
    {
        _moderacion.CambieLaVisibilidadDelComentario(7, true, CT).Returns(false);
        _moderacion.ExisteElComentario(7, CT).Returns(true);
        var elConflicto = await FalleCon(TipoDeFalla.Conflicto, () => CreeElServicio().RetireElComentario(7, new MotivoRequest("x"), CT));
        Assert.Equal("El comentario ya está oculto.", elConflicto.Titulo);

        _moderacion.ExisteElComentario(7, CT).Returns(false);
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().RetireElComentario(7, new MotivoRequest("x"), CT));
    }

    [Fact]
    public async Task El_historial_rechaza_un_rango_invertido()
    {
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().ListeElHistorial(
            new FiltroDelHistorial(null, null, new DateOnly(2026, 5, 2), new DateOnly(2026, 5, 1)), null, null, CT));
    }

    [Fact]
    public async Task El_historial_convierte_las_fechas_de_costa_rica_a_utc_con_limite_superior_exclusivo()
    {
        await CreeElServicio().ListeElHistorial(new FiltroDelHistorial(10, null, new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 1)), null, null, CT);

        await _moderacion.Received(1).ListeElHistorial(10, null, new DateTime(2026, 5, 1, 6, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 2, 6, 0, 0, DateTimeKind.Utc), Arg.Any<Recetas.Api.Aplicacion.Comun.SolicitudDePagina>(), CT);
    }
}
