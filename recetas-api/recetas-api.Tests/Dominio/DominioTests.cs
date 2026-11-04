using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Dominio;
using Recetas.Api.Tests.Comun;

namespace Recetas.Api.Tests.Dominio;

public class TextoTests
{
    [Theory]
    [InlineData("  Arroz   blanco ", "Arroz blanco")]
    [InlineData("a\tb\nc", "a b c")]
    [InlineData("   ", "")]
    public void Normalizar_recorta_y_colapsa_los_espacios(string laEntrada, string laSalida) =>
        Assert.Equal(laSalida, Texto.NormaliceElTexto(laEntrada));

    [Fact]
    public void La_clave_de_un_nombre_no_distingue_mayusculas_ni_espacios_repetidos() =>
        Assert.Equal(Texto.CalculeLaClaveDelNombre("  SAL   Gruesa "), Texto.CalculeLaClaveDelNombre("sal gruesa"));

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData("  hola ", "hola")]
    public void Recortar_o_marcar_nulo(string? laEntrada, string? laSalida) =>
        Assert.Equal(laSalida, Texto.RecorteOMarqueNulo(laEntrada));

    [Fact]
    public void Limitar_el_largo_solo_corta_cuando_se_excede()
    {
        Assert.Equal("abc", Texto.LimiteElLargo("abcdef", 3));
        Assert.Equal("abc", Texto.LimiteElLargo("abc", 3));
    }
}

public class ZonaHorariaTests
{
    [Fact]
    public void Costa_rica_es_utc_menos_6_sin_horario_de_verano() =>
        Assert.Equal(TimeSpan.FromHours(-6), ZonaHoraria.COSTA_RICA);

    [Theory]
    [InlineData(2026, 11, 3, 12, 3)]
    [InlineData(2026, 11, 1, 3, 31)]
    public void La_fecha_de_hoy_se_toma_en_hora_de_costa_rica(int anio, int mes, int dia, int hora, int diaEsperado)
    {
        var hoy = ZonaHoraria.ObtengaLaFechaDeHoy(new RelojFijo(new DateTimeOffset(anio, mes, dia, hora, 0, 0, TimeSpan.Zero)));

        Assert.Equal(diaEsperado, hoy.Day);
    }

    [Fact]
    public void El_inicio_del_dia_en_costa_rica_son_las_6_de_la_manana_utc() =>
        Assert.Equal(new DateTime(2026, 5, 1, 6, 0, 0, DateTimeKind.Utc), ZonaHoraria.ConviertaElInicioDelDiaAUtc(new DateOnly(2026, 5, 1)));
}

public class ReglasDeAccesoTests
{
    [Theory]
    [InlineData(EstadosDeLaReceta.PUBLICADA, false, null, true)]
    [InlineData(EstadosDeLaReceta.BORRADOR, false, null, false)]
    [InlineData(EstadosDeLaReceta.BORRADOR, false, 7, true)]
    [InlineData(EstadosDeLaReceta.MODERADA, true, 1, true)]
    [InlineData(EstadosDeLaReceta.MODERADA, false, 1, false)]
    public void Quien_puede_ver_una_receta(string elEstado, bool esAdmin, int? elUsuarioId, bool seEspera) =>
        Assert.Equal(seEspera, ReglasDeAcceso.PuedeVerLaReceta(elEstado, elAutorId: 7, esAdmin, elUsuarioId));
}

public class SolicitudDePaginaTests
{
    [Theory]
    [InlineData(null, null, 1, 12)]
    [InlineData(0, 0, 1, 1)]
    [InlineData(-5, -5, 1, 1)]
    [InlineData(3, 500, 3, 50)]
    [InlineData(2, 10, 2, 10)]
    public void Normalizar_acota_la_pagina_y_el_tamano(int? laPagina, int? elTamano, int paginaEsperada, int tamanoEsperado)
    {
        var laSolicitud = SolicitudDePagina.Normalice(laPagina, elTamano);

        Assert.Equal((paginaEsperada, tamanoEsperado), (laSolicitud.Pagina, laSolicitud.Tamano));
    }

    [Fact]
    public void Cuantos_saltar_depende_de_la_pagina_y_el_tamano() =>
        Assert.Equal(20, new SolicitudDePagina(3, 10).CuantosSaltar);
}

public class ExcepcionDeNegocioTests
{
    [Fact]
    public void Cada_fabrica_crea_el_tipo_de_falla_correspondiente()
    {
        Assert.Equal(TipoDeFalla.SolicitudInvalida, ExcepcionDeNegocio.PorSolicitudInvalida("x", "detalle").Tipo);
        Assert.Equal(TipoDeFalla.RecursoInexistente, ExcepcionDeNegocio.PorRecursoInexistente().Tipo);
        Assert.Equal(TipoDeFalla.Conflicto, ExcepcionDeNegocio.PorConflicto("x").Tipo);
        Assert.Equal(TipoDeFalla.AccesoProhibido, ExcepcionDeNegocio.PorAccesoProhibido().Tipo);
        Assert.Equal(TipoDeFalla.FaltaDeAutenticacion, ExcepcionDeNegocio.PorFaltaDeAutenticacion().Tipo);
        Assert.Equal(TipoDeFalla.ServicioNoDisponible, ExcepcionDeNegocio.PorServicioNoDisponible("x").Tipo);
    }

    [Fact]
    public void El_titulo_y_el_detalle_se_conservan()
    {
        var laExcepcion = ExcepcionDeNegocio.PorSolicitudInvalida("Orden inválido.", "Use: recientes");

        Assert.Equal(("Orden inválido.", "Use: recientes", "Orden inválido."), (laExcepcion.Titulo, laExcepcion.Detalle, laExcepcion.Message));
    }
}
