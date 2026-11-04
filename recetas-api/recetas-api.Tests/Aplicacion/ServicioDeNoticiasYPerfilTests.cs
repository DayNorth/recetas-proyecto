using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Servicios;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;
using static Recetas.Api.Tests.Comun.Dobles;

namespace Recetas.Api.Tests.Aplicacion;

public class ServicioDeNoticiasTests
{
    private readonly IRepositorioDeNoticias _noticias = Substitute.For<IRepositorioDeNoticias>();
    private readonly IAlmacenDeArchivos _archivos = Substitute.For<IAlmacenDeArchivos>();

    private ServicioDeNoticias CreeElServicio(bool esAdmin = true) => new(_noticias, _archivos, Usuario(esAdmin ? 1 : 3, esAdmin));

    private static NoticiaDto UnaNoticia(bool estaPublicada = true, string? laImagen = null) =>
        new(1, "Titulo", "Contenido", laImagen, estaPublicada, DateTime.UtcNow, "Admin");

    public ServicioDeNoticiasTests()
    {
        _archivos.GuardeElArchivo(Arg.Any<Stream>(), "noticias", true, false, Arg.Any<long>(), CT).Returns("noticias/nueva.jpg");
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    public async Task Solo_el_administrador_puede_listar_las_no_publicadas(bool loPide, bool esAdmin, bool seIncluyen)
    {
        await CreeElServicio(esAdmin).ListeLasNoticias(loPide, null, null, CT);

        await _noticias.Received(1).ListeLasNoticias(seIncluyen, Arg.Any<Recetas.Api.Aplicacion.Comun.SolicitudDePagina>(), CT);
    }

    [Fact]
    public async Task Una_noticia_no_publicada_da_404_salvo_para_el_administrador()
    {
        _noticias.BusqueLaNoticia(1, CT).Returns(UnaNoticia(estaPublicada: false));

        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio(esAdmin: false).ObtengaLaNoticia(1, CT));
        Assert.Equal(1, (await CreeElServicio(esAdmin: true).ObtengaLaNoticia(1, CT)).NoticiaId);
    }

    [Fact]
    public async Task Crear_normaliza_los_textos_y_publica_por_defecto()
    {
        _noticias.AgregueLaNoticia(1, "Tip de cocina", "Texto", null, true, CT).Returns(UnaNoticia());

        await CreeElServicio().CreeLaNoticia(new DatosDeLaNoticia("  Tip   de cocina ", "  Texto ", null, null, false), CT);

        await _noticias.Received(1).AgregueLaNoticia(1, "Tip de cocina", "Texto", null, true, CT);
    }

    [Fact]
    public async Task Crear_con_imagen_la_guarda_y_si_falla_la_base_la_borra()
    {
        _noticias.AgregueLaNoticia(1, Arg.Any<string>(), Arg.Any<string>(), "noticias/nueva.jpg", Arg.Any<bool>(), CT)
            .ThrowsAsync(new InvalidOperationException());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreeElServicio().CreeLaNoticia(new DatosDeLaNoticia("T", "C", true, UnArchivo(), false), CT));

        _archivos.Received(1).BorreElArchivo("noticias/nueva.jpg");
    }

    [Fact]
    public async Task Editar_una_noticia_inexistente_da_404_sin_guardar_la_imagen()
    {
        _noticias.ExisteLaNoticia(9, CT).Returns(false);

        await FalleCon(TipoDeFalla.RecursoInexistente,
            () => CreeElServicio().EditeLaNoticia(9, new DatosDeLaNoticia("T", "C", null, UnArchivo(), false), CT));

        await _archivos.DidNotReceiveWithAnyArgs().GuardeElArchivo(default!, default!, default, default, default, default);
    }

    [Fact]
    public async Task Editar_reemplazando_la_imagen_borra_la_anterior_despues_de_guardar()
    {
        _noticias.ExisteLaNoticia(1, CT).Returns(true);
        _noticias.ActualiceLaNoticia(1, "T", "C", "noticias/nueva.jpg", false, null, CT)
            .Returns((UnaNoticia(laImagen: "noticias/nueva.jpg"), (string?)"noticias/vieja.jpg"));

        await CreeElServicio().EditeLaNoticia(1, new DatosDeLaNoticia("T", "C", null, UnArchivo(), false), CT);

        _archivos.Received(1).BorreElArchivo("noticias/vieja.jpg");
    }

    [Fact]
    public async Task Editar_sin_cambiar_la_imagen_no_borra_nada_y_quitarla_borra_la_anterior()
    {
        _noticias.ExisteLaNoticia(1, CT).Returns(true);
        _noticias.ActualiceLaNoticia(1, "T", "C", null, false, null, CT)
            .Returns((UnaNoticia(laImagen: "noticias/igual.jpg"), (string?)"noticias/igual.jpg"));
        _noticias.ActualiceLaNoticia(1, "T", "C", null, true, null, CT)
            .Returns((UnaNoticia(laImagen: null), (string?)"noticias/igual.jpg"));

        await CreeElServicio().EditeLaNoticia(1, new DatosDeLaNoticia("T", "C", null, null, false), CT);
        _archivos.DidNotReceiveWithAnyArgs().BorreElArchivo(default);

        await CreeElServicio().EditeLaNoticia(1, new DatosDeLaNoticia("T", "C", null, null, true), CT);
        _archivos.Received(1).BorreElArchivo("noticias/igual.jpg");
    }

    [Fact]
    public async Task Editar_si_falla_la_base_borra_la_imagen_nueva_y_conserva_la_anterior()
    {
        _noticias.ExisteLaNoticia(1, CT).Returns(true);
        _noticias.ActualiceLaNoticia(1, Arg.Any<string>(), Arg.Any<string>(), "noticias/nueva.jpg", Arg.Any<bool>(), Arg.Any<bool?>(), CT)
            .ThrowsAsync(new InvalidOperationException());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreeElServicio().EditeLaNoticia(1, new DatosDeLaNoticia("T", "C", null, UnArchivo(), false), CT));

        _archivos.Received(1).BorreElArchivo("noticias/nueva.jpg");
        _archivos.DidNotReceive().BorreElArchivo("noticias/vieja.jpg");
    }

    [Fact]
    public async Task Despublicar_una_noticia_inexistente_da_404()
    {
        _noticias.DespubliqueLaNoticia(9, CT).Returns(false);
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().DespubliqueLaNoticia(9, CT));
    }
}

public class ServicioDePerfilTests
{
    private readonly IRepositorioDeUsuarios _usuarios = Substitute.For<IRepositorioDeUsuarios>();
    private readonly IAlmacenDeArchivos _archivos = Substitute.For<IAlmacenDeArchivos>();

    private ServicioDePerfil CreeElServicio() => new(_usuarios, Usuario(3), _archivos);

    [Fact]
    public async Task Actualizar_el_perfil_recorta_los_textos_y_descarta_la_descripcion_vacia()
    {
        await CreeElServicio().ActualiceElPerfil(new ActualizarPerfilRequest(" Ana ", " Pérez ", "   "), CT);

        await _usuarios.Received(1).ActualiceElPerfil(3, "Ana", "Pérez", null, CT);
    }

    [Fact]
    public async Task Cambiar_la_foto_sin_archivo_o_vacio_da_400()
    {
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().CambieLaFoto(null, CT));
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().CambieLaFoto(UnArchivo(0), CT));
    }

    [Fact]
    public async Task Cambiar_la_foto_borra_la_anterior_solo_cuando_la_base_confirma()
    {
        _archivos.GuardeElArchivo(Arg.Any<Stream>(), "perfiles", true, false, 2 * 1024 * 1024, CT).Returns("perfiles/nueva.png");
        _usuarios.ActualiceLaFoto(3, "perfiles/nueva.png", CT).Returns((UnUsuarioDto(), (string?)"perfiles/vieja.png"));

        await CreeElServicio().CambieLaFoto(UnArchivo(), CT);

        _archivos.Received(1).BorreElArchivo("perfiles/vieja.png");
    }

    [Fact]
    public async Task Si_falla_la_base_se_borra_la_foto_nueva_y_no_la_anterior()
    {
        _archivos.GuardeElArchivo(Arg.Any<Stream>(), "perfiles", true, false, Arg.Any<long>(), CT).Returns("perfiles/nueva.png");
        _usuarios.ActualiceLaFoto(3, "perfiles/nueva.png", CT).ThrowsAsync(new InvalidOperationException());

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreeElServicio().CambieLaFoto(UnArchivo(), CT));

        _archivos.Received(1).BorreElArchivo("perfiles/nueva.png");
        _archivos.Received(1).BorreElArchivo(Arg.Any<string?>());
    }
}
