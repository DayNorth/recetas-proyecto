using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;

namespace Recetas.Api.Aplicacion.Servicios;

public class ServicioDeNoticias(IRepositorioDeNoticias lasNoticias, IAlmacenDeArchivos losArchivos, IUsuarioActual elUsuarioActual)
{
    private const long MAXIMO_DE_BYTES_DE_LA_IMAGEN = 5 * 1024 * 1024;

    public Task<Paginado<NoticiaDto>> ListeLasNoticias(bool incluirNoPublicadas, int? laPagina, int? elTamano, CancellationToken cancelacion) =>
        lasNoticias.ListeLasNoticias(incluirNoPublicadas && elUsuarioActual.EsAdministrador,
            SolicitudDePagina.Normalice(laPagina, elTamano), cancelacion);

    public async Task<NoticiaDto> ObtengaLaNoticia(int laNoticiaId, CancellationToken cancelacion)
    {
        var laNoticia = await lasNoticias.BusqueLaNoticia(laNoticiaId, cancelacion);
        if (laNoticia is null || (!laNoticia.Publicada && !elUsuarioActual.EsAdministrador))
            throw ExcepcionDeNegocio.PorRecursoInexistente("La noticia no existe.");
        return laNoticia;
    }

    public async Task<NoticiaDto> CreeLaNoticia(DatosDeLaNoticia losDatos, CancellationToken cancelacion)
    {
        var laRutaDeLaImagen = await GuardeLaImagen(losDatos.Imagen, cancelacion);
        try
        {
            return await lasNoticias.AgregueLaNoticia(elUsuarioActual.IdRequerido, Texto.NormaliceElTexto(losDatos.Titulo),
                losDatos.Contenido.Trim(), laRutaDeLaImagen, losDatos.Publicada ?? true, cancelacion);
        }
        catch
        {
            losArchivos.BorreElArchivo(laRutaDeLaImagen);
            throw;
        }
    }

    public async Task<NoticiaDto> EditeLaNoticia(int laNoticiaId, DatosDeLaNoticia losDatos, CancellationToken cancelacion)
    {
        if (!await lasNoticias.ExisteLaNoticia(laNoticiaId, cancelacion))
            throw ExcepcionDeNegocio.PorRecursoInexistente("La noticia no existe.");

        var laRutaDeLaImagenNueva = await GuardeLaImagen(losDatos.Imagen, cancelacion);
        NoticiaDto laNoticia;
        string? laImagenAnterior;
        try
        {
            (laNoticia, laImagenAnterior) = await lasNoticias.ActualiceLaNoticia(laNoticiaId, Texto.NormaliceElTexto(losDatos.Titulo),
                losDatos.Contenido.Trim(), laRutaDeLaImagenNueva, losDatos.QuitarImagen, losDatos.Publicada, cancelacion);
        }
        catch
        {
            losArchivos.BorreElArchivo(laRutaDeLaImagenNueva);
            throw;
        }

        if (laImagenAnterior != laNoticia.Imagen) losArchivos.BorreElArchivo(laImagenAnterior);
        return laNoticia;
    }

    public async Task DespubliqueLaNoticia(int laNoticiaId, CancellationToken cancelacion)
    {
        if (!await lasNoticias.DespubliqueLaNoticia(laNoticiaId, cancelacion))
            throw ExcepcionDeNegocio.PorRecursoInexistente("La noticia no existe.");
    }

    private async Task<string?> GuardeLaImagen(ArchivoSubido? laImagen, CancellationToken cancelacion)
    {
        if (laImagen is not { Longitud: > 0 }) return null;
        await using var elFlujo = laImagen.AbraElFlujo();
        return await losArchivos.GuardeElArchivo(elFlujo, "noticias", true, false, MAXIMO_DE_BYTES_DE_LA_IMAGEN, cancelacion);
    }
}
