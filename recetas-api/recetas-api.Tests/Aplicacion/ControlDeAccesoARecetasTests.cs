using NSubstitute;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Servicios;
using Recetas.Api.Dominio;
using static Recetas.Api.Tests.Comun.Dobles;

namespace Recetas.Api.Tests.Aplicacion;

public class ControlDeAccesoARecetasTests
{
    private readonly IRepositorioDeRecetas _recetas = Substitute.For<IRepositorioDeRecetas>();

    private ControlDeAccesoARecetas CreeElControl(int? elUsuarioId, bool esAdministrador = false) =>
        new(_recetas, Usuario(elUsuarioId, esAdministrador));

    [Theory]
    [InlineData(null, false, EstadosDeLaReceta.PUBLICADA, true)]
    [InlineData(null, false, EstadosDeLaReceta.BORRADOR, false)]
    [InlineData(3, false, EstadosDeLaReceta.INACTIVA, false)]
    [InlineData(2, false, EstadosDeLaReceta.MODERADA, true)]
    [InlineData(1, true, EstadosDeLaReceta.MODERADA, true)]
    public void Visibilidad_de_una_receta_segun_estado_y_usuario(int? elUsuarioId, bool esAdmin, string elEstado, bool seEsperaVerla)
    {
        Assert.Equal(seEsperaVerla, CreeElControl(elUsuarioId, esAdmin).PuedeVerLaReceta(elEstado, elAutorId: 2));
    }

    [Fact]
    public async Task Exigir_autor_de_una_receta_inexistente_da_404()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns((RecetaBasica?)null);
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElControl(2).ExijaAlAutor(10, CT));
    }

    [Fact]
    public async Task Exigir_autor_de_la_receta_propia_la_devuelve_en_cualquier_estado()
    {
        var laReceta = UnaReceta(elAutorId: 2, elEstado: EstadosDeLaReceta.INACTIVA);
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(laReceta);

        Assert.Equal(laReceta, await CreeElControl(2).ExijaAlAutor(10, CT));
    }

    [Fact]
    public async Task Exigir_autor_de_una_receta_ajena_visible_da_403()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elAutorId: 2));
        await FalleCon(TipoDeFalla.AccesoProhibido, () => CreeElControl(3).ExijaAlAutor(10, CT));
    }

    [Fact]
    public async Task Exigir_autor_de_una_receta_ajena_no_visible_da_404_para_no_revelar_que_existe()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elAutorId: 2, elEstado: EstadosDeLaReceta.BORRADOR));
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElControl(3).ExijaAlAutor(10, CT));
    }

    [Fact]
    public async Task Un_administrador_tampoco_edita_recetas_ajenas_aunque_pueda_verlas()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elAutorId: 2, elEstado: EstadosDeLaReceta.MODERADA));
        await FalleCon(TipoDeFalla.AccesoProhibido, () => CreeElControl(1, esAdministrador: true).ExijaAlAutor(10, CT));
    }

    [Theory]
    [InlineData(EstadosDeLaReceta.BORRADOR)]
    [InlineData(EstadosDeLaReceta.INACTIVA)]
    [InlineData(EstadosDeLaReceta.MODERADA)]
    public async Task Exigir_publicada_rechaza_cualquier_otro_estado_con_404(string elEstado)
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elEstado: elEstado));
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElControl(3).ExijaLaRecetaPublicada(10, CT));
    }

    [Fact]
    public async Task Exigir_publicada_acepta_una_receta_publicada()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta());
        Assert.Equal(10, (await CreeElControl(3).ExijaLaRecetaPublicada(10, CT)).RecetaId);
    }
}
