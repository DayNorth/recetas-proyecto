using Microsoft.IdentityModel.JsonWebTokens;
using NSubstitute;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Dominio;
using Recetas.Api.Infraestructura.Archivos;
using Recetas.Api.Infraestructura.Reportes;
using Recetas.Api.Infraestructura.Seguridad;
using Recetas.Api.Tests.Comun;

namespace Recetas.Api.Tests.Infraestructura;

public class AlmacenLocalDeArchivosTests : IDisposable
{
    private static readonly byte[] PNG = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];
    private static readonly byte[] JPG = [0xFF, 0xD8, 0xFF, 0xE0, 0, 0];
    private static readonly byte[] WEBP = [(byte)'R', (byte)'I', (byte)'F', (byte)'F', 0, 0, 0, 0, (byte)'W', (byte)'E', (byte)'B', (byte)'P'];
    private static readonly byte[] PDF = "%PDF-1.7 contenido"u8.ToArray();

    private readonly string _carpeta = Path.Combine(Path.GetTempPath(), "recetas-unit-" + Guid.NewGuid().ToString("N"));
    private readonly AlmacenLocalDeArchivos _almacen;

    public AlmacenLocalDeArchivosTests()
    {
        _almacen = new AlmacenLocalDeArchivos(new OpcionesDeAlmacenamiento { Path = _carpeta }, Substitute.For<Microsoft.Extensions.Hosting.IHostEnvironment>());
    }

    public void Dispose()
    {
        if (Directory.Exists(_carpeta)) Directory.Delete(_carpeta, true);
    }

    [Theory]
    [InlineData("png", ".png")]
    [InlineData("jpg", ".jpg")]
    [InlineData("webp", ".webp")]
    public async Task El_tipo_se_detecta_por_el_contenido_y_el_archivo_queda_con_nombre_aleatorio(string elTipo, string laExtension)
    {
        var losBytes = elTipo switch { "png" => PNG, "jpg" => JPG, _ => WEBP };

        var laRuta = await _almacen.GuardeElArchivo(new MemoryStream(losBytes), "pruebas", true, false, 1024);

        Assert.StartsWith("pruebas/", laRuta);
        Assert.EndsWith(laExtension, laRuta);
        Assert.True(File.Exists(Path.Combine(_almacen.RutaRaiz, laRuta)));
    }

    [Fact]
    public async Task Un_texto_con_extension_de_imagen_se_rechaza_porque_el_contenido_no_coincide()
    {
        var laExcepcion = await Assert.ThrowsAsync<ExcepcionDeNegocio>(
            () => _almacen.GuardeElArchivo(new MemoryStream("no soy una imagen"u8.ToArray()), "pruebas", true, true, 1024));

        Assert.Equal(TipoDeFalla.SolicitudInvalida, laExcepcion.Tipo);
    }

    [Fact]
    public async Task Un_pdf_solo_se_acepta_cuando_se_permite()
    {
        await Assert.ThrowsAsync<ExcepcionDeNegocio>(() => _almacen.GuardeElArchivo(new MemoryStream(PDF), "pruebas", true, false, 1024));

        Assert.EndsWith(".pdf", await _almacen.GuardeElArchivo(new MemoryStream(PDF), "pruebas", true, true, 1024));
    }

    [Fact]
    public async Task Se_rechazan_los_archivos_vacios_y_los_que_superan_el_maximo()
    {
        await Assert.ThrowsAsync<ExcepcionDeNegocio>(() => _almacen.GuardeElArchivo(new MemoryStream([]), "pruebas", true, false, 1024));
        await Assert.ThrowsAsync<ExcepcionDeNegocio>(() => _almacen.GuardeElArchivo(new MemoryStream(new byte[2048]), "pruebas", true, false, 1024));
    }

    [Theory]
    [InlineData("..")]
    [InlineData("a/b")]
    [InlineData("a\\b")]
    public async Task Una_carpeta_con_separadores_o_puntos_dobles_se_rechaza(string laCarpeta) =>
        await Assert.ThrowsAsync<ArgumentException>(() => _almacen.GuardeElArchivo(new MemoryStream(PNG), laCarpeta, true, false, 1024));

    [Fact]
    public async Task Borrar_elimina_el_archivo_pero_nunca_toca_nada_fuera_de_la_carpeta_de_archivos()
    {
        var laRuta = await _almacen.GuardeElArchivo(new MemoryStream(PNG), "pruebas", true, false, 1024);
        var unoAfuera = Path.Combine(Path.GetTempPath(), "recetas-fuera-" + Guid.NewGuid().ToString("N") + ".txt");
        await File.WriteAllTextAsync(unoAfuera, "no me borres");

        _almacen.BorreElArchivo(laRuta);
        _almacen.BorreElArchivo("../" + Path.GetFileName(unoAfuera));
        _almacen.BorreElArchivo(null);
        _almacen.BorreElArchivo("  ");

        Assert.False(File.Exists(Path.Combine(_almacen.RutaRaiz, laRuta)));
        Assert.True(File.Exists(unoAfuera));
        File.Delete(unoAfuera);
    }
}

public class SeguridadTests
{
    [Fact]
    public void El_token_lleva_sub_rol_y_jti_y_vence_segun_las_horas_configuradas()
    {
        var lasOpciones = new OpcionesJwt { Key = "clave-de-pruebas-0123456789-0123456789-abcdef", Horas = 3 };
        var ahora = new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);
        var elServicio = new ServicioDeTokensJwt(lasOpciones, new RelojFijo(ahora));

        var elToken = elServicio.Cree(42, Roles.ADMIN);
        var elJwt = new JsonWebTokenHandler().ReadJsonWebToken(elToken.Token);

        Assert.Equal("42", elJwt.GetClaim("sub").Value);
        Assert.Equal("ADMIN", elJwt.GetClaim("rol").Value);
        Assert.False(string.IsNullOrEmpty(elJwt.Id));
        Assert.Equal(ahora.AddHours(3), elToken.Expira);
        Assert.Equal(ahora.AddHours(3).UtcDateTime, elJwt.ValidTo);
    }

    [Fact]
    public void Dos_tokens_del_mismo_usuario_tienen_identificadores_distintos()
    {
        var elServicio = new ServicioDeTokensJwt(new OpcionesJwt { Key = new string('k', 40) }, TimeProvider.System);
        var lector = new JsonWebTokenHandler();

        Assert.NotEqual(lector.ReadJsonWebToken(elServicio.Cree(1, "USUARIO").Token).Id, lector.ReadJsonWebToken(elServicio.Cree(1, "USUARIO").Token).Id);
    }

    [Fact]
    public void Un_token_revocado_se_reconoce_y_los_vencidos_se_limpian()
    {
        var reloj = new RelojFijo(new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.Zero));
        var elRevocador = new RevocadorDeTokensEnMemoria(reloj);

        elRevocador.Revoque("vencido", reloj.GetUtcNow().AddMinutes(-1));
        elRevocador.Revoque("vigente", reloj.GetUtcNow().AddHours(1));

        Assert.True(elRevocador.EstaRevocado("vigente"));
        Assert.False(elRevocador.EstaRevocado("vencido"));
        Assert.False(elRevocador.EstaRevocado("otro"));
        Assert.False(elRevocador.EstaRevocado(null));
    }

    [Fact]
    public void El_hash_de_bcrypt_verifica_la_clave_correcta_y_rechaza_las_demas()
    {
        var elHasheador = new HasheadorBCrypt();
        var elHash = elHasheador.Hashee("clave1234");

        Assert.NotEqual("clave1234", elHash);
        Assert.True(elHasheador.Verifique("clave1234", elHash));
        Assert.False(elHasheador.Verifique("otra1234", elHash));
        Assert.False(elHasheador.Verifique("clave1234", elHasheador.HashDeRelleno));
    }
}

public class ExportadorDeReportesTests
{
    private static ResultadoReporte UnResultado(params Dictionary<string, object?>[] lasFilas) =>
        new("MAS_VISTAS", "Recetas más vistas", new RangoReporte(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31)),
            [new ColumnaReporte("titulo", "Receta", "texto"), new ColumnaReporte("promedio", "Promedio", "decimal")],
            lasFilas, new DecisionReporte("¿Qué?", "Lea así", "Haga esto"));

    private static readonly DateTimeOffset AHORA = new(2026, 11, 3, 6, 0, 0, TimeSpan.FromHours(-6));

    [Fact]
    public void Formatear_celdas_usa_coma_decimal_y_vacio_para_nulo()
    {
        Assert.Equal("4,5", ExportadorDeReportes.FormateeLaCelda(4.50m));
        Assert.Equal("4,25", ExportadorDeReportes.FormateeLaCelda(4.25));
        Assert.Equal("", ExportadorDeReportes.FormateeLaCelda(null));
        Assert.Equal("12", ExportadorDeReportes.FormateeLaCelda(12));
        Assert.Equal("texto", ExportadorDeReportes.FormateeLaCelda("texto"));
    }

    [Fact]
    public void El_csv_lleva_bom_utf8_separador_punto_y_coma_y_escapa_los_campos_especiales()
    {
        var lasFilas = new[]
        {
            new Dictionary<string, object?> { ["titulo"] = "Arroz; con \"pollo\"", ["promedio"] = 4.5m },
            new Dictionary<string, object?> { ["titulo"] = "Sopa", ["promedio"] = null }
        };

        var elContenido = new ExportadorDeReportes().Exporte(UnResultado(lasFilas), "csv", AHORA);

        Assert.Equal("text/csv; charset=utf-8", elContenido.TipoDeContenido);
        Assert.Equal([0xEF, 0xBB, 0xBF], elContenido.Bytes[..3]);
        var elTexto = System.Text.Encoding.UTF8.GetString(elContenido.Bytes[3..]);
        Assert.Equal("Receta;Promedio\r\n\"Arroz; con \"\"pollo\"\"\";4,5\r\nSopa;\r\n", elTexto);
    }

    [Fact]
    public void El_xlsx_es_un_zip_y_el_pdf_empieza_con_la_firma_de_pdf()
    {
        var elExportador = new ExportadorDeReportes();
        var lasFilas = new[] { new Dictionary<string, object?> { ["titulo"] = "Arroz", ["promedio"] = 4.5m } };

        var elXlsx = elExportador.Exporte(UnResultado(lasFilas), "xlsx", AHORA);
        var elPdf = elExportador.Exporte(UnResultado(lasFilas), "pdf", AHORA);

        Assert.Equal("PK"u8.ToArray(), elXlsx.Bytes[..2]);
        Assert.Equal("%PDF"u8.ToArray(), elPdf.Bytes[..4]);
        Assert.Equal("application/pdf", elPdf.TipoDeContenido);
    }

    [Fact]
    public void Un_formato_desconocido_lanza() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExportadorDeReportes().Exporte(UnResultado(), "docx", AHORA));
}
