using NSubstitute;
using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Servicios;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;
using static Recetas.Api.Tests.Comun.Dobles;

namespace Recetas.Api.Tests.Aplicacion;

public class ServicioDeConsultaDeRecetasTests
{
    private readonly IConsultasDeRecetas _consultas = Substitute.For<IConsultasDeRecetas>();
    private readonly IRepositorioDeRecetas _recetas = Substitute.For<IRepositorioDeRecetas>();
    private readonly IRepositorioDeCatalogos _catalogos = Substitute.For<IRepositorioDeCatalogos>();

    private ServicioDeConsultaDeRecetas CreeElServicio(int? elUsuarioId = null, bool esAdministrador = false)
    {
        var elUsuario = Usuario(elUsuarioId, esAdministrador);
        return new(_consultas, _recetas, _catalogos, new ControlDeAccesoARecetas(_recetas, elUsuario), elUsuario);
    }

    private static ConsultaDelCatalogo UnaConsulta(string? elTexto = null, short? laCategoria = null, int? elTiempo = null,
        int[]? losIngredientes = null, string? elOrden = null, bool registrar = false) =>
        new(elTexto, laCategoria, null, elTiempo, losIngredientes, elOrden, registrar, null, null);

    private static Paginado<RecetaResumenDto> UnaPagina(decimal? elPromedio = null, int elTotal = 1) =>
        new([new RecetaResumenDto(1, "Arroz", null, null, "Platos", "Fácil", 30, 0, elPromedio, 0, "PUBLICADA")], 1, 12, elTotal);

    private static RecetaDetalleDto UnDetalle(int elAutorId = 2, string elEstado = EstadosDeLaReceta.PUBLICADA) =>
        new(10, "Arroz", null, 1, "Platos", 1, "Fácil", 30, 4, "LOCAL", elEstado, DateTime.UtcNow, new AutorDto(elAutorId, "Ana"),
            [], [], [], 0, 0, null, 0, null, null, null);

    [Theory]
    [InlineData("alfabetico")]
    [InlineData("  ")]
    public async Task Un_orden_desconocido_da_400_y_uno_en_blanco_usa_recientes(string elOrden)
    {
        _consultas.BusqueLasRecetasPublicadas(Arg.Any<FiltroDeRecetas>(), Arg.Any<SolicitudDePagina>(), CT).Returns(UnaPagina());

        if (string.IsNullOrWhiteSpace(elOrden))
        {
            await CreeElServicio().BusqueLasRecetas(UnaConsulta(elOrden: elOrden), CT);
            await _consultas.Received(1).BusqueLasRecetasPublicadas(Arg.Is<FiltroDeRecetas>(f => f.Orden == "recientes"), Arg.Any<SolicitudDePagina>(), CT);
        }
        else
            await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().BusqueLasRecetas(UnaConsulta(elOrden: elOrden), CT));
    }

    [Fact]
    public async Task Tiempo_maximo_no_positivo_o_mas_de_20_ingredientes_dan_400()
    {
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().BusqueLasRecetas(UnaConsulta(elTiempo: 0), CT));
        await FalleCon(TipoDeFalla.SolicitudInvalida,
            () => CreeElServicio().BusqueLasRecetas(UnaConsulta(losIngredientes: [.. Enumerable.Range(1, 21)]), CT));
    }

    [Fact]
    public async Task Los_ingredientes_repetidos_se_cuentan_una_sola_vez()
    {
        _consultas.BusqueLasRecetasPublicadas(Arg.Any<FiltroDeRecetas>(), Arg.Any<SolicitudDePagina>(), CT).Returns(UnaPagina());

        await CreeElServicio().BusqueLasRecetas(UnaConsulta(losIngredientes: [1, 1, 2, 2, 3]), CT);

        await _consultas.Received(1).BusqueLasRecetasPublicadas(Arg.Is<FiltroDeRecetas>(f => f.Ingredientes.SequenceEqual(new[] { 1, 2, 3 })),
            Arg.Any<SolicitudDePagina>(), CT);
    }

    [Fact]
    public async Task El_texto_se_recorta_y_se_limita_a_100_caracteres()
    {
        _consultas.BusqueLasRecetasPublicadas(Arg.Any<FiltroDeRecetas>(), Arg.Any<SolicitudDePagina>(), CT).Returns(UnaPagina());

        await CreeElServicio().BusqueLasRecetas(UnaConsulta(elTexto: "  " + new string('a', 150) + "  "), CT);

        await _consultas.Received(1).BusqueLasRecetasPublicadas(Arg.Is<FiltroDeRecetas>(f => f.Texto!.Length == 100),
            Arg.Any<SolicitudDePagina>(), CT);
    }

    [Fact]
    public async Task La_calificacion_promedio_se_redondea_a_dos_decimales()
    {
        _consultas.BusqueLasRecetasPublicadas(Arg.Any<FiltroDeRecetas>(), Arg.Any<SolicitudDePagina>(), CT).Returns(UnaPagina(4.33333m));

        var elResultado = await CreeElServicio().BusqueLasRecetas(UnaConsulta(), CT);

        Assert.Equal(4.33m, elResultado.Items[0].CalificacionPromedio);
    }

    [Fact]
    public async Task La_busqueda_se_registra_solo_si_se_pide_y_hay_algun_criterio()
    {
        _consultas.BusqueLasRecetasPublicadas(Arg.Any<FiltroDeRecetas>(), Arg.Any<SolicitudDePagina>(), CT).Returns(UnaPagina(elTotal: 3));
        _consultas.AgregueLaBusqueda(Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<short?>(), Arg.Any<int>(), CT).Returns(55L);

        var sinCriterio = await CreeElServicio().BusqueLasRecetas(UnaConsulta(registrar: true), CT);
        var sinPedirlo = await CreeElServicio().BusqueLasRecetas(UnaConsulta(elTexto: "arroz"), CT);
        var registrada = await CreeElServicio(4).BusqueLasRecetas(UnaConsulta(elTexto: "arroz", registrar: true), CT);

        Assert.Null(sinCriterio.BusquedaId);
        Assert.Null(sinPedirlo.BusquedaId);
        Assert.Equal(55L, registrada.BusquedaId);
        await _consultas.Received(1).AgregueLaBusqueda(4, "arroz", null, 3, CT);
    }

    [Fact]
    public async Task El_termino_de_una_busqueda_sin_texto_sale_de_los_ingredientes_o_de_la_categoria()
    {
        _consultas.BusqueLasRecetasPublicadas(Arg.Any<FiltroDeRecetas>(), Arg.Any<SolicitudDePagina>(), CT).Returns(UnaPagina());
        _catalogos.ListeLosNombresDeIngredientes(Arg.Any<IReadOnlyCollection<int>>(), CT).Returns(new[] { "Ajo", "Sal" });
        _catalogos.BusqueElNombreDeLaCategoria(3, CT).Returns("Postres");
        _catalogos.ConsulteSiLaCategoriaEstaActiva(3, CT).Returns(true);

        await CreeElServicio().BusqueLasRecetas(UnaConsulta(losIngredientes: [1, 2], registrar: true), CT);
        await CreeElServicio().BusqueLasRecetas(UnaConsulta(laCategoria: 3, registrar: true), CT);
        await CreeElServicio().BusqueLasRecetas(UnaConsulta(elTiempo: 20, registrar: true), CT);

        await _consultas.Received(1).AgregueLaBusqueda(null, "Ajo, Sal", null, 1, CT);
        await _consultas.Received(1).AgregueLaBusqueda(null, "Postres", (short)3, 1, CT);
        await _consultas.Received(1).AgregueLaBusqueda(null, "hasta 20 min", null, 1, CT);
    }

    [Fact]
    public async Task Una_categoria_inexistente_se_guarda_sin_categoria_para_no_violar_la_llave_foranea()
    {
        _consultas.BusqueLasRecetasPublicadas(Arg.Any<FiltroDeRecetas>(), Arg.Any<SolicitudDePagina>(), CT).Returns(UnaPagina());
        _catalogos.ConsulteSiLaCategoriaEstaActiva(99, CT).Returns((bool?)null);

        await CreeElServicio().BusqueLasRecetas(UnaConsulta(elTexto: "x", laCategoria: 99, registrar: true), CT);

        await _consultas.Received(1).AgregueLaBusqueda(null, "x", null, 1, CT);
    }

    [Theory]
    [InlineData(null, false, EstadosDeLaReceta.BORRADOR)]
    [InlineData(3, false, EstadosDeLaReceta.INACTIVA)]
    public async Task El_detalle_de_una_receta_no_publicada_ajena_da_404_y_no_registra_visita(int? elUsuarioId, bool esAdmin, string elEstado)
    {
        _consultas.BusqueElDetalleDeLaReceta(10, elUsuarioId, CT).Returns(UnDetalle(elEstado: elEstado));

        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio(elUsuarioId, esAdmin).ObtengaElDetalle(10, null, CT));

        await _consultas.DidNotReceiveWithAnyArgs().AgregueLaVisita(default, default, default, default);
    }

    [Fact]
    public async Task El_detalle_de_una_receta_inexistente_da_404()
    {
        _consultas.BusqueElDetalleDeLaReceta(10, null, CT).Returns((RecetaDetalleDto?)null);
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().ObtengaElDetalle(10, null, CT));
    }

    [Fact]
    public async Task Un_administrador_ve_el_detalle_de_una_receta_moderada_sin_contar_visita()
    {
        _consultas.BusqueElDetalleDeLaReceta(10, 1, CT).Returns(UnDetalle(elEstado: EstadosDeLaReceta.MODERADA));

        await CreeElServicio(1, esAdministrador: true).ObtengaElDetalle(10, null, CT);

        await _consultas.DidNotReceiveWithAnyArgs().AgregueLaVisita(default, default, default, default);
    }

    [Fact]
    public async Task La_visita_de_un_anonimo_a_una_receta_publicada_se_registra()
    {
        _consultas.BusqueElDetalleDeLaReceta(10, null, CT).Returns(UnDetalle());

        await CreeElServicio().ObtengaElDetalle(10, null, CT);

        await _consultas.Received(1).AgregueLaVisita(10, null, null, CT);
    }

    [Fact]
    public async Task La_visita_del_propio_autor_no_se_cuenta()
    {
        _consultas.BusqueElDetalleDeLaReceta(10, 2, CT).Returns(UnDetalle(elAutorId: 2));

        await CreeElServicio(2).ObtengaElDetalle(10, null, CT);

        await _consultas.DidNotReceiveWithAnyArgs().AgregueLaVisita(default, default, default, default);
    }

    [Fact]
    public async Task La_visita_conserva_la_busqueda_solo_si_es_valida_para_ese_usuario()
    {
        _consultas.BusqueElDetalleDeLaReceta(10, 3, CT).Returns(UnDetalle());
        _consultas.LaBusquedaEsValidaParaElUsuario(55L, 3, CT).Returns(true);
        _consultas.LaBusquedaEsValidaParaElUsuario(66L, 3, CT).Returns(false);

        await CreeElServicio(3).ObtengaElDetalle(10, 55L, CT);
        await CreeElServicio(3).ObtengaElDetalle(10, 66L, CT);

        await _consultas.Received(1).AgregueLaVisita(10, 3, 55L, CT);
        await _consultas.Received(1).AgregueLaVisita(10, 3, null, CT);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task Las_porciones_fuera_de_1_a_100_dan_400(int lasPorciones)
    {
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().CalculeLosIngredientesPorPorciones(10, lasPorciones, CT));
    }

    [Fact]
    public async Task Sin_indicar_porciones_se_usan_las_de_la_receta()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta());
        _consultas.CalculeLosIngredientesParaLasPorciones(10, 4, CT).Returns(new List<IngredienteRecetaDto>());

        var elResultado = await CreeElServicio().CalculeLosIngredientesPorPorciones(10, null, CT);

        Assert.Equal((4, 4), (elResultado.Porciones, elResultado.PorcionesBase));
    }

    [Fact]
    public async Task Calcular_porciones_de_una_receta_no_visible_da_404()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elEstado: EstadosDeLaReceta.INACTIVA));
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().CalculeLosIngredientesPorPorciones(10, 2, CT));
    }

    [Fact]
    public async Task Mis_recetas_con_un_estado_invalido_da_400_y_con_uno_valido_lo_normaliza()
    {
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio(2).ListeMisRecetas("eliminada", null, null, CT));

        _consultas.ListeLasRecetasDelAutor(2, "INACTIVA", Arg.Any<SolicitudDePagina>(), CT).Returns(UnaPagina());
        await CreeElServicio(2).ListeMisRecetas(" inactiva ", null, null, CT);

        await _consultas.Received(1).ListeLasRecetasDelAutor(2, "INACTIVA", Arg.Any<SolicitudDePagina>(), CT);
    }
}
