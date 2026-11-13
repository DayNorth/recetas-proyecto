using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Reportes;
using Recetas.Api.Aplicacion.Servicios;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;
using Recetas.Api.Tests.Comun;
using static Recetas.Api.Tests.Comun.Dobles;

namespace Recetas.Api.Tests.Aplicacion;

public class CalculadoraDeRangosTests
{
    private static CalculadoraDeRangos CreeLaCalculadora(DateTimeOffset? laHora = null) =>
        new(new RelojFijo(laHora ?? new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.Zero)));

    [Fact]
    public void Sin_parametros_usa_los_6_meses_completos_anteriores_al_actual()
    {
        var elRango = CreeLaCalculadora().ResuelvaElRango(null, null, null, null);

        Assert.Equal(new RangoReporte(new DateOnly(2026, 5, 1), new DateOnly(2026, 10, 31)), elRango);
    }

    [Fact]
    public void El_mes_en_curso_se_calcula_con_la_hora_de_costa_rica_no_la_de_utc()
    {
        var elRango = CreeLaCalculadora(new DateTimeOffset(2026, 11, 1, 3, 0, 0, TimeSpan.Zero)).ResuelvaElRango(null, null, null, null);

        Assert.Equal(new RangoReporte(new DateOnly(2026, 4, 1), new DateOnly(2026, 9, 30)), elRango);
    }

    [Fact]
    public void Con_meses_el_fin_es_el_ultimo_dia_del_mes_final()
    {
        var elRango = CreeLaCalculadora().ResuelvaElRango(null, null, "2026-01", "2026-02");

        Assert.Equal(new RangoReporte(new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 28)), elRango);
    }

    [Fact]
    public void Con_fechas_se_respetan_tal_cual()
    {
        var elRango = CreeLaCalculadora().ResuelvaElRango(new DateOnly(2026, 3, 5), new DateOnly(2026, 3, 9), null, null);

        Assert.Equal(new RangoReporte(new DateOnly(2026, 3, 5), new DateOnly(2026, 3, 9)), elRango);
    }

    [Theory]
    [InlineData("2026-01-01", "2026-02-01", "2026-01", "2026-02")]
    [InlineData("2026-01-01", null, null, null)]
    [InlineData(null, null, "2026-01", null)]
    [InlineData(null, null, "enero", "2026-02")]
    [InlineData("2026-03-02", "2026-03-01", null, null)]
    [InlineData("2024-01-01", "2026-01-01", null, null)]
    public void Los_rangos_invalidos_dan_400(string? elInicio, string? elFin, string? elMesInicio, string? elMesFin)
    {
        DateOnly? Fecha(string? elTexto) => elTexto is null ? null : DateOnly.Parse(elTexto);

        var laExcepcion = Assert.Throws<ExcepcionDeNegocio>(() => CreeLaCalculadora().ResuelvaElRango(Fecha(elInicio), Fecha(elFin), elMesInicio, elMesFin));
        Assert.Equal(TipoDeFalla.SolicitudInvalida, laExcepcion.Tipo);
    }

    [Fact]
    public void Exactamente_24_meses_es_valido()
    {
        var elRango = CreeLaCalculadora().ResuelvaElRango(null, null, "2024-01", "2025-12");

        Assert.Equal(new DateOnly(2025, 12, 31), elRango.Fin);
    }
}

public class ServicioDeReportesTests
{
    private readonly IRepositorioDeReportes _repositorio = Substitute.For<IRepositorioDeReportes>();
    private readonly IExportadorDeReportes _exportador = Substitute.For<IExportadorDeReportes>();
    private readonly DateTimeOffset _ahora = new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);

    private ServicioDeReportes CreeElServicio() =>
        new(_repositorio, _exportador, new CalculadoraDeRangos(new RelojFijo(_ahora)), Usuario(1, true), new RelojFijo(_ahora));

    private static ResultadoReporte UnResultado(string elTipo = "MAS_VISTAS") =>
        new(elTipo, "Título", new RangoReporte(new DateOnly(2026, 5, 1), new DateOnly(2026, 10, 31)), [], [], new DecisionReporte("p", "r", "a"));

    [Fact]
    public void El_catalogo_lista_los_10_reportes_y_expone_los_alias_de_los_parametros_de_rango()
    {
        var elCatalogo = CreeElServicio().ListeElCatalogo();

        Assert.Equal(10, elCatalogo.Count);
        Assert.Equal(["inicio|mesInicio", "fin|mesFin", "limite"], elCatalogo.Single(r => r.Tipo == "MAS_VISTAS").Parametros);
    }

    [Fact]
    public async Task Un_tipo_de_reporte_desconocido_da_404()
    {
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().EjecuteElReporte("DROP_TABLE", new ConsultaDeReporte(), CT));

        await _repositorio.DidNotReceiveWithAnyArgs().EjecuteElReporte(default!, default!, default!, default);
    }

    [Theory]
    [InlineData(0, null, null)]
    [InlineData(101, null, null)]
    [InlineData(null, 0, null)]
    [InlineData(null, 1001, null)]
    [InlineData(null, null, 0)]
    [InlineData(null, null, 100001)]
    public async Task Los_limites_fuera_de_rango_dan_400(int? elLimite, int? losVotos, int? lasVisitas)
    {
        var laConsulta = new ConsultaDeReporte { Limite = elLimite, MinVotos = losVotos, MinVisitas = lasVisitas };
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().EjecuteElReporte("MAS_VISTAS", laConsulta, CT));
    }

    [Fact]
    public async Task El_tipo_no_distingue_mayusculas_y_los_parametros_tienen_valores_por_defecto()
    {
        await CreeElServicio().EjecuteElReporte("mas_vistas", new ConsultaDeReporte(), CT);

        await _repositorio.Received(1).EjecuteElReporte(Arg.Is<DefinicionDeReporte>(d => d.Tipo == "MAS_VISTAS"), Arg.Any<RangoReporte>(),
            new ParametrosReporte(10, 3, 20), CT);
    }

    [Fact]
    public async Task Descargar_con_un_formato_invalido_da_400_antes_de_consultar()
    {
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().DescargueElReporte("MAS_VISTAS", new ConsultaDeReporte { Formato = "docx" }, CT));
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().DescargueElReporte("MAS_VISTAS", new ConsultaDeReporte(), CT));

        await _repositorio.DidNotReceiveWithAnyArgs().EjecuteElReporte(default!, default!, default!, default);
    }

    [Fact]
    public async Task Descargar_exporta_registra_la_bitacora_y_arma_el_nombre_del_archivo()
    {
        var elResultado = UnResultado();
        _repositorio.EjecuteElReporte(Arg.Any<DefinicionDeReporte>(), Arg.Any<RangoReporte>(), Arg.Any<ParametrosReporte>(), CT).Returns(elResultado);
        _exportador.Exporte(elResultado, "csv", new DateTimeOffset(2026, 11, 3, 6, 0, 0, TimeSpan.FromHours(-6)))
            .Returns(new ContenidoExportado([1, 2], "text/csv"));

        var elArchivo = await CreeElServicio().DescargueElReporte("mas_vistas", new ConsultaDeReporte { Formato = " CSV ", Inicio = new DateOnly(2026, 1, 1), Fin = new DateOnly(2026, 1, 31) }, CT);

        Assert.Equal("reporte-mas_vistas-20260101-20260131.csv", elArchivo.Nombre);
        Assert.Equal("text/csv", elArchivo.TipoDeContenido);
        await _repositorio.Received(1).RegistreLaDescarga(1, "MAS_VISTAS", new RangoReporte(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31)), "CSV", CT);
    }

    [Fact]
    public async Task Si_la_exportacion_falla_la_descarga_no_queda_registrada()
    {
        _repositorio.EjecuteElReporte(Arg.Any<DefinicionDeReporte>(), Arg.Any<RangoReporte>(), Arg.Any<ParametrosReporte>(), CT).Returns(UnResultado());
        _exportador.Exporte(Arg.Any<ResultadoReporte>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>()).Throws(new InvalidOperationException());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreeElServicio().DescargueElReporte("MAS_VISTAS", new ConsultaDeReporte { Formato = "pdf" }, CT));

        await _repositorio.DidNotReceiveWithAnyArgs().RegistreLaDescarga(default, default!, default!, default!, default);
    }
}

public class ServicioDelTableroTests
{
    private readonly IRepositorioDeReportes _repositorio = Substitute.For<IRepositorioDeReportes>();
    private static readonly UmbralesTablero UMBRALES = new(3, 1.5m, 30, 0.5m, 0.20m, 5);

    private ServicioDelTablero CreeElServicio() =>
        new(_repositorio, new CalculadoraDeRangos(new RelojFijo(new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.Zero))), UMBRALES);

    private static Dictionary<string, object?> Fila(params (string Campo, object? Valor)[] losCampos) =>
        losCampos.ToDictionary(c => c.Campo, c => c.Valor);

    private void Devuelva(string elTipo, params Dictionary<string, object?>[] lasFilas) =>
        _repositorio.EjecuteElReporte(Arg.Is<DefinicionDeReporte>(d => d.Tipo == elTipo), Arg.Any<RangoReporte>(), Arg.Any<ParametrosReporte>(), CT)
            .Returns(new ResultadoReporte(elTipo, elTipo, new RangoReporte(default, default), [], lasFilas, new DecisionReporte("", "", "")));

    public ServicioDelTableroTests()
    {
        foreach (var elTipo in new[] { "BUSQUEDAS_SIN_RESULTADO", "OFERTA_DEMANDA", "CONVERSION", "TENDENCIA_MENSUAL", "MEJOR_CALIFICADAS" })
            Devuelva(elTipo);
    }

    [Fact]
    public async Task Sin_datos_no_hay_hallazgos_y_el_rango_es_el_de_los_6_meses_anteriores()
    {
        var elTablero = await CreeElServicio().ObtengaElTablero(new ConsultaDeReporte(), CT);

        Assert.Empty(elTablero.Hallazgos);
        Assert.Equal(new DateOnly(2026, 5, 1), elTablero.Rango.Inicio);
    }

    [Fact]
    public async Task Las_busquedas_sin_resultado_se_reportan_solo_si_alcanzan_el_umbral()
    {
        Devuelva("BUSQUEDAS_SIN_RESULTADO", Fila(("termino", "paella"), ("vecesBuscado", 5L)), Fila(("termino", "sushi"), ("vecesBuscado", 2L)));

        var elHallazgo = Assert.Single((await CreeElServicio().ObtengaElTablero(new ConsultaDeReporte(), CT)).Hallazgos);

        Assert.Equal("BUSQUEDAS_SIN_RESULTADO", elHallazgo.Codigo);
        Assert.Equal(["paella"], elHallazgo.Items.Select(i => i.Etiqueta));
    }

    [Fact]
    public async Task Las_categorias_con_mucha_mas_demanda_que_el_promedio_o_sin_recetas_se_reportan()
    {
        Devuelva("OFERTA_DEMANDA",
            Fila(("categoria", "Postres"), ("visitasPorReceta", 30m), ("visitas", 300m)),
            Fila(("categoria", "Sopas"), ("visitasPorReceta", 5m), ("visitas", 50m)),
            Fila(("categoria", "Bebidas"), ("visitasPorReceta", 5m), ("visitas", 50m)),
            Fila(("categoria", "Vegano"), ("visitasPorReceta", null), ("visitas", 40m)));

        var elHallazgo = Assert.Single((await CreeElServicio().ObtengaElTablero(new ConsultaDeReporte(), CT)).Hallazgos);

        Assert.Equal("CATEGORIA_CON_DEMANDA", elHallazgo.Codigo);
        Assert.Equal(["Postres", "Vegano"], elHallazgo.Items.Select(i => i.Etiqueta));
    }

    [Fact]
    public async Task Una_receta_con_conversion_menor_a_la_mitad_del_promedio_se_marca_como_alta()
    {
        Devuelva("CONVERSION",
            Fila(("titulo", "Mala"), ("visitas", 100m), ("interacciones", 10m), ("tasaConversion", 10m)),
            Fila(("titulo", "Buena"), ("visitas", 100m), ("interacciones", 50m), ("tasaConversion", 50m)));

        var elHallazgo = Assert.Single((await CreeElServicio().ObtengaElTablero(new ConsultaDeReporte(), CT)).Hallazgos);

        Assert.Equal(("CONVERSION_BAJA", "alta"), (elHallazgo.Codigo, elHallazgo.Severidad));
        Assert.Equal(["Mala"], elHallazgo.Items.Select(i => i.Etiqueta));
    }

    [Fact]
    public async Task La_caida_mensual_ignora_el_mes_en_curso_porque_esta_incompleto()
    {
        Devuelva("TENDENCIA_MENSUAL",
            Fila(("periodo", "2026-08"), ("visitas", 100m)),
            Fila(("periodo", "2026-09"), ("visitas", 70m)),
            Fila(("periodo", "2026-10"), ("visitas", 75m)),
            Fila(("periodo", "2026-11"), ("visitas", 1m)));

        var elHallazgo = Assert.Single((await CreeElServicio().ObtengaElTablero(new ConsultaDeReporte(), CT)).Hallazgos);

        Assert.Equal("CAIDA_MENSUAL", elHallazgo.Codigo);
        var laCaida = Assert.Single(elHallazgo.Items);
        Assert.Equal(("2026-09", -30.0m), (laCaida.Etiqueta, laCaida.Valor));
    }

    [Fact]
    public async Task La_primera_mejor_calificada_se_propone_para_promover()
    {
        Devuelva("MEJOR_CALIFICADAS", Fila(("titulo", "Lasaña"), ("promedio", 4.8m), ("votos", 9L)));

        var elHallazgo = Assert.Single((await CreeElServicio().ObtengaElTablero(new ConsultaDeReporte(), CT)).Hallazgos);

        Assert.Equal(("RECETA_A_PROMOVER", "info"), (elHallazgo.Codigo, elHallazgo.Severidad));
        Assert.Equal("Lasaña", elHallazgo.Items[0].Etiqueta);
    }
}

public class CatalogoReportesTests
{
    [Theory]
    [InlineData("me_gusta", "meGusta")]
    [InlineData("visitas_desde_busqueda", "visitasDesdeBusqueda")]
    [InlineData("titulo", "titulo")]
    public void Convierte_snake_case_de_sql_a_camel_case_de_json(string elSql, string elJson) =>
        Assert.Equal(elJson, CatalogoReportes.ConviertaACamelCase(elSql));

    [Fact]
    public void Las_columnas_conocidas_tienen_titulo_en_espanol_y_las_demas_se_derivan_del_nombre()
    {
        Assert.Equal("Me gusta", CatalogoReportes.ObtengaElTituloDeLaColumna("me_gusta"));
        Assert.Equal("columna rara", CatalogoReportes.ObtengaElTituloDeLaColumna("columna_rara"));
    }

    [Fact]
    public void El_catalogo_tiene_10_reportes_con_tipos_unicos_y_se_busca_sin_distinguir_mayusculas()
    {
        Assert.Equal(10, CatalogoReportes.TODOS.Count);
        Assert.Equal(10, CatalogoReportes.TODOS.Select(r => r.Tipo).Distinct().Count());
        Assert.Equal("CONVERSION", CatalogoReportes.Busque("conversion")!.Tipo);
        Assert.Null(CatalogoReportes.Busque("inexistente"));
    }
}
