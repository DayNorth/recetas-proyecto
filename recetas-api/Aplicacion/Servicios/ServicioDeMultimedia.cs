using System.Text.RegularExpressions;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Dominio;

namespace Recetas.Api.Aplicacion.Servicios;

public partial class ServicioDeMultimedia(IRepositorioDeMultimedia laMultimedia, ControlDeAccesoARecetas elControlDeAcceso,
    IAlmacenDeArchivos losArchivos, IEjecutorDeTransacciones elEjecutor, IPuntoDeFalla elPuntoDeFalla)
{
    public const int MAXIMO_DE_ARCHIVOS_POR_RECETA = 10;
    private const long MAXIMO_DE_BYTES_DE_LA_IMAGEN = 5 * 1024 * 1024;
    private const long MAXIMO_DE_BYTES_DEL_PDF = 10 * 1024 * 1024;
    private const int LARGO_MAXIMO_DE_LA_DESCRIPCION = 150;

    [GeneratedRegex(@"^(?:https?://)?(?:www\.|m\.)?(?:youtube\.com/(?:watch\?(?:[^#]*&)?v=|embed/|shorts/)|youtu\.be/)(?<id>[A-Za-z0-9_-]{11})(?:[?&#/].*)?$",
        RegexOptions.IgnoreCase)]
    private static partial Regex ObtengaLaExpresionDeYouTube();

    private static bool LaRutaEsLocal(string laRuta) => !laRuta.StartsWith("http", StringComparison.OrdinalIgnoreCase);

    public async Task<MultimediaDto> AgregueElArchivo(int laRecetaId, ArchivoSubido? elArchivo, string? laUrl, string? laDescripcion,
        CancellationToken cancelacion)
    {
        await ExijaUnaRecetaModificable(laRecetaId, cancelacion);

        var tieneArchivo = elArchivo is { Longitud: > 0 };
        var tieneUrl = !string.IsNullOrWhiteSpace(laUrl);
        if (tieneArchivo == tieneUrl)
            throw ExcepcionDeNegocio.PorSolicitudInvalida("Envíe un archivo (\"archivo\") o una URL de YouTube (\"url\"), no ambos ni ninguno.");
        if (laDescripcion is { Length: > LARGO_MAXIMO_DE_LA_DESCRIPCION })
            throw ExcepcionDeNegocio.PorSolicitudInvalida("La descripción no puede superar 150 caracteres.");

        if (await laMultimedia.CuenteLosArchivos(laRecetaId, cancelacion) >= MAXIMO_DE_ARCHIVOS_POR_RECETA)
            throw ExcepcionDeNegocio.PorSolicitudInvalida($"Una receta admite hasta {MAXIMO_DE_ARCHIVOS_POR_RECETA} archivos.");

        string elTipo, laRuta;
        if (tieneUrl)
        {
            var laCoincidencia = ObtengaLaExpresionDeYouTube().Match(laUrl!.Trim());
            if (!laCoincidencia.Success) throw ExcepcionDeNegocio.PorSolicitudInvalida("La URL debe ser un video de YouTube.");
            elTipo = TiposDeMultimedia.VIDEO;
            laRuta = $"https://www.youtube.com/embed/{laCoincidencia.Groups["id"].Value}";
        }
        else
        {
            await using var elFlujo = elArchivo!.AbraElFlujo();
            laRuta = await losArchivos.GuardeElArchivo(elFlujo, "recetas", permitirImagenes: true, permitirPdf: true,
                MAXIMO_DE_BYTES_DEL_PDF, cancelacion);
            elTipo = laRuta.EndsWith(".pdf") ? TiposDeMultimedia.DOCUMENTO : TiposDeMultimedia.IMAGEN;
            if (elTipo == TiposDeMultimedia.IMAGEN && elArchivo.Longitud > MAXIMO_DE_BYTES_DE_LA_IMAGEN)
            {
                losArchivos.BorreElArchivo(laRuta);
                throw ExcepcionDeNegocio.PorSolicitudInvalida("Las imágenes no pueden superar 5 MB.");
            }
        }

        try
        {
            var esPortada = elTipo == TiposDeMultimedia.IMAGEN && !await laMultimedia.LaRecetaTienePortada(laRecetaId, cancelacion);
            return await laMultimedia.AgregueElArchivo(laRecetaId, elTipo, laRuta, Texto.RecorteOMarqueNulo(laDescripcion), esPortada, cancelacion);
        }
        catch
        {
            if (elTipo != TiposDeMultimedia.VIDEO) losArchivos.BorreElArchivo(laRuta);
            throw;
        }
    }

    public async Task BorreElArchivo(int laRecetaId, int elArchivoId, CancellationToken cancelacion)
    {
        await ExijaUnaRecetaModificable(laRecetaId, cancelacion);

        var laRutaABorrar = await elEjecutor.EjecuteEnTransaccion(async laCancelacion =>
        {
            var laRuta = await laMultimedia.BorreElArchivoYReasigneLaPortada(laRecetaId, elArchivoId, laCancelacion)
                         ?? throw ExcepcionDeNegocio.PorRecursoInexistente("El archivo no existe en esta receta.");
            return LaRutaEsLocal(laRuta) ? laRuta : null;
        }, cancelacion);

        losArchivos.BorreElArchivo(laRutaABorrar);
    }

    public async Task DefinaLaPortada(int laRecetaId, int elArchivoId, CancellationToken cancelacion)
    {
        await ExijaUnaRecetaModificable(laRecetaId, cancelacion);

        var elArchivo = await laMultimedia.BusqueElArchivo(laRecetaId, elArchivoId, cancelacion)
                        ?? throw ExcepcionDeNegocio.PorRecursoInexistente("El archivo no existe en esta receta.");
        if (elArchivo.Tipo != TiposDeMultimedia.IMAGEN)
            throw ExcepcionDeNegocio.PorSolicitudInvalida("Solo una imagen puede ser la portada.");

        await elEjecutor.EjecuteEnTransaccion(async laCancelacion =>
        {
            await laMultimedia.QuiteLasDemasPortadas(laRecetaId, elArchivoId, laCancelacion);
            elPuntoDeFalla.PaseElPuntoDeFalla("portada-limpiada");
            await laMultimedia.EstablezcaLaPortada(elArchivoId, laCancelacion);
        }, cancelacion);
    }

    private async Task ExijaUnaRecetaModificable(int laRecetaId, CancellationToken cancelacion)
    {
        var laReceta = await elControlDeAcceso.ExijaAlAutor(laRecetaId, cancelacion);
        if (laReceta.Estado == EstadosDeLaReceta.MODERADA)
            throw ExcepcionDeNegocio.PorConflicto("La receta fue moderada y no se puede modificar.");
    }
}
