using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Servicios;
using Recetas.Api.Dominio;
using Recetas.Api.Tests.Comun;
using static Recetas.Api.Tests.Comun.Dobles;

namespace Recetas.Api.Tests.Aplicacion;

public class ServicioDeMultimediaTests
{
    private readonly IRepositorioDeMultimedia _multimedia = Substitute.For<IRepositorioDeMultimedia>();
    private readonly IRepositorioDeRecetas _recetas = Substitute.For<IRepositorioDeRecetas>();
    private readonly IAlmacenDeArchivos _archivos = Substitute.For<IAlmacenDeArchivos>();
    private readonly EjecutorSinBaseDeDatos _ejecutor = new();
    private IPuntoDeFalla _falla = Substitute.For<IPuntoDeFalla>();

    public ServicioDeMultimediaTests()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elAutorId: 2));
        _multimedia.AgregueElArchivo(10, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<bool>(), CT)
            .Returns(c => new MultimediaDto(1, c.ArgAt<string>(1), c.ArgAt<string>(2), c.ArgAt<string?>(3), c.ArgAt<bool>(4)));
    }

    private ServicioDeMultimedia CreeElServicio(int elUsuarioId = 2) =>
        new(_multimedia, new ControlDeAccesoARecetas(_recetas, Usuario(elUsuarioId)), _archivos, _ejecutor, _falla);

    private void GuarderaComo(string laRuta) =>
        _archivos.GuardeElArchivo(Arg.Any<Stream>(), "recetas", true, true, Arg.Any<long>(), CT).Returns(laRuta);

    [Fact]
    public async Task Solo_el_autor_agrega_archivos_y_una_receta_moderada_no_se_modifica()
    {
        await FalleCon(TipoDeFalla.AccesoProhibido, () => CreeElServicio(elUsuarioId: 5).AgregueElArchivo(10, UnArchivo(), null, null, CT));

        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elAutorId: 2, elEstado: EstadosDeLaReceta.MODERADA));
        await FalleCon(TipoDeFalla.Conflicto, () => CreeElServicio().AgregueElArchivo(10, UnArchivo(), null, null, CT));
    }

    [Fact]
    public async Task Hay_que_enviar_un_archivo_o_una_url_pero_no_ambos_ni_ninguno()
    {
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().AgregueElArchivo(10, null, null, null, CT));
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().AgregueElArchivo(10, UnArchivo(0), " ", null, CT));
        await FalleCon(TipoDeFalla.SolicitudInvalida,
            () => CreeElServicio().AgregueElArchivo(10, UnArchivo(), "https://youtu.be/abcdefghijk", null, CT));
    }

    [Fact]
    public async Task La_descripcion_no_puede_superar_150_caracteres()
    {
        await FalleCon(TipoDeFalla.SolicitudInvalida,
            () => CreeElServicio().AgregueElArchivo(10, UnArchivo(), null, new string('x', 151), CT));
    }

    [Fact]
    public async Task Una_receta_admite_hasta_10_archivos()
    {
        _multimedia.CuenteLosArchivos(10, CT).Returns(ServicioDeMultimedia.MAXIMO_DE_ARCHIVOS_POR_RECETA);

        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().AgregueElArchivo(10, UnArchivo(), null, null, CT));

        await _archivos.DidNotReceiveWithAnyArgs().GuardeElArchivo(default!, default!, default, default, default, default);
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=abcdefghijk")]
    [InlineData("https://youtu.be/abcdefghijk")]
    [InlineData("youtube.com/shorts/abcdefghijk")]
    [InlineData("https://m.youtube.com/embed/abcdefghijk")]
    public async Task Las_urls_de_youtube_se_convierten_al_formato_de_incrustacion_y_no_son_portada(string laUrl)
    {
        var elArchivo = await CreeElServicio().AgregueElArchivo(10, null, laUrl, null, CT);

        Assert.Equal(("VIDEO", "https://www.youtube.com/embed/abcdefghijk", false), (elArchivo.Tipo, elArchivo.Ruta, elArchivo.EsPortada));
        await _archivos.DidNotReceiveWithAnyArgs().GuardeElArchivo(default!, default!, default, default, default, default);
    }

    [Theory]
    [InlineData("https://vimeo.com/123456789")]
    [InlineData("https://www.youtube.com/watch?v=corto")]
    [InlineData("https://evil.com/?u=https://youtu.be/abcdefghijk")]
    public async Task Una_url_que_no_es_de_youtube_da_400(string laUrl)
    {
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().AgregueElArchivo(10, null, laUrl, null, CT));
    }

    [Fact]
    public async Task La_primera_imagen_queda_como_portada_y_la_segunda_no()
    {
        GuarderaComo("recetas/a.jpg");

        _multimedia.LaRecetaTienePortada(10, CT).Returns(false);
        Assert.True((await CreeElServicio().AgregueElArchivo(10, UnArchivo(), null, null, CT)).EsPortada);

        _multimedia.LaRecetaTienePortada(10, CT).Returns(true);
        Assert.False((await CreeElServicio().AgregueElArchivo(10, UnArchivo(), null, null, CT)).EsPortada);
    }

    [Fact]
    public async Task Un_pdf_se_guarda_como_documento_y_nunca_es_portada()
    {
        GuarderaComo("recetas/a.pdf");
        _multimedia.LaRecetaTienePortada(10, CT).Returns(false);

        var elArchivo = await CreeElServicio().AgregueElArchivo(10, UnArchivo(8 * 1024 * 1024), null, null, CT);

        Assert.Equal((TiposDeMultimedia.DOCUMENTO, false), (elArchivo.Tipo, elArchivo.EsPortada));
    }

    [Fact]
    public async Task Una_imagen_de_mas_de_5_MB_se_rechaza_y_se_borra_del_disco()
    {
        GuarderaComo("recetas/grande.png");

        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().AgregueElArchivo(10, UnArchivo(6 * 1024 * 1024), null, null, CT));

        _archivos.Received(1).BorreElArchivo("recetas/grande.png");
        await _multimedia.DidNotReceiveWithAnyArgs().AgregueElArchivo(default, default!, default!, default, default, default);
    }

    [Fact]
    public async Task Si_falla_el_registro_en_la_base_se_borra_el_archivo_subido_pero_un_video_no_toca_el_disco()
    {
        GuarderaComo("recetas/a.jpg");
        _multimedia.AgregueElArchivo(10, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<bool>(), CT)
            .ThrowsAsync(new InvalidOperationException());

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreeElServicio().AgregueElArchivo(10, UnArchivo(), null, null, CT));
        _archivos.Received(1).BorreElArchivo("recetas/a.jpg");

        _archivos.ClearReceivedCalls();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreeElServicio().AgregueElArchivo(10, null, "https://youtu.be/abcdefghijk", null, CT));
        _archivos.DidNotReceiveWithAnyArgs().BorreElArchivo(default);
    }

    [Fact]
    public async Task Borrar_un_archivo_inexistente_en_la_receta_da_404_y_no_toca_el_disco()
    {
        _multimedia.BorreElArchivoYReasigneLaPortada(10, 5, CT).Returns((string?)null);

        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().BorreElArchivo(10, 5, CT));

        _archivos.DidNotReceiveWithAnyArgs().BorreElArchivo(default);
    }

    [Theory]
    [InlineData("recetas/a.jpg", "recetas/a.jpg")]
    [InlineData("https://www.youtube.com/embed/abcdefghijk", null)]
    public async Task Borrar_quita_el_archivo_del_disco_despues_del_commit_y_solo_si_es_local(string laRutaGuardada, string? laRutaBorrada)
    {
        _multimedia.BorreElArchivoYReasigneLaPortada(10, 5, CT).Returns(laRutaGuardada);

        await CreeElServicio().BorreElArchivo(10, 5, CT);

        Assert.Equal(1, _ejecutor.Ejecuciones);
        _archivos.Received(1).BorreElArchivo(laRutaBorrada);
    }

    [Fact]
    public async Task Solo_una_imagen_puede_ser_portada()
    {
        _multimedia.BusqueElArchivo(10, 5, CT).Returns(new MultimediaDto(5, "VIDEO", "https://x", null, false));
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().DefinaLaPortada(10, 5, CT));

        _multimedia.BusqueElArchivo(10, 6, CT).Returns((MultimediaDto?)null);
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().DefinaLaPortada(10, 6, CT));
    }

    [Fact]
    public async Task Definir_la_portada_limpia_las_demas_y_luego_marca_la_nueva()
    {
        _multimedia.BusqueElArchivo(10, 5, CT).Returns(new MultimediaDto(5, "IMAGEN", "recetas/a.jpg", null, false));

        await CreeElServicio().DefinaLaPortada(10, 5, CT);

        Received.InOrder(() =>
        {
            _multimedia.QuiteLasDemasPortadas(10, 5, CT);
            _multimedia.EstablezcaLaPortada(5, CT);
        });
    }

    [Fact]
    public async Task Si_falla_tras_limpiar_las_portadas_no_se_marca_la_nueva()
    {
        _falla = new FallaEnElPunto("portada-limpiada");
        _multimedia.BusqueElArchivo(10, 5, CT).Returns(new MultimediaDto(5, "IMAGEN", "recetas/a.jpg", null, false));

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreeElServicio().DefinaLaPortada(10, 5, CT));

        await _multimedia.DidNotReceiveWithAnyArgs().EstablezcaLaPortada(default, default);
    }
}
