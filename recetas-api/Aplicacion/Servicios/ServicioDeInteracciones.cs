using System.Net;
using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;

namespace Recetas.Api.Aplicacion.Servicios;

public class ServicioDeInteracciones(IRepositorioDeInteracciones lasInteracciones, IRepositorioDeRecetas lasRecetas,
    IRepositorioDeCatalogos losCatalogos, ControlDeAccesoARecetas elControlDeAcceso, IUsuarioActual elUsuarioActual,
    OpcionesDeEnlaces lasOpcionesDeEnlaces)
{

    public async Task<ToggleDto> DeMeGustaALaReceta(int laRecetaId, CancellationToken cancelacion)
    {
        await elControlDeAcceso.ExijaLaRecetaPublicada(laRecetaId, cancelacion);
        await lasInteracciones.AgregueElMeGusta(elUsuarioActual.IdRequerido, laRecetaId, cancelacion);
        return new ToggleDto(true, await lasInteracciones.CuenteLosMeGusta(laRecetaId, cancelacion));
    }

    public async Task<ToggleDto> QuiteElMeGusta(int laRecetaId, CancellationToken cancelacion)
    {
        await elControlDeAcceso.ExijaLaRecetaExistente(laRecetaId, cancelacion);
        await lasInteracciones.QuiteElMeGusta(elUsuarioActual.IdRequerido, laRecetaId, cancelacion);
        return new ToggleDto(false, await lasInteracciones.CuenteLosMeGusta(laRecetaId, cancelacion));
    }

    public async Task<ToggleDto> AgregueAFavoritos(int laRecetaId, CancellationToken cancelacion)
    {
        await elControlDeAcceso.ExijaLaRecetaPublicada(laRecetaId, cancelacion);
        await lasInteracciones.AgregueElFavorito(elUsuarioActual.IdRequerido, laRecetaId, cancelacion);
        return new ToggleDto(true, await lasInteracciones.CuenteLosFavoritos(laRecetaId, cancelacion));
    }

    public async Task<ToggleDto> QuiteDeFavoritos(int laRecetaId, CancellationToken cancelacion)
    {
        await elControlDeAcceso.ExijaLaRecetaExistente(laRecetaId, cancelacion);
        await lasInteracciones.QuiteElFavorito(elUsuarioActual.IdRequerido, laRecetaId, cancelacion);
        return new ToggleDto(false, await lasInteracciones.CuenteLosFavoritos(laRecetaId, cancelacion));
    }

    public Task<Paginado<FavoritoDto>> ListeLosFavoritos(int? laPagina, int? elTamano, CancellationToken cancelacion) =>
        lasInteracciones.ListeLosFavoritos(elUsuarioActual.IdRequerido, SolicitudDePagina.Normalice(laPagina, elTamano), cancelacion);

    public async Task<ComentariosPage> ListeLosComentarios(int laRecetaId, int? laPagina, int? elTamano, CancellationToken cancelacion)
    {
        var laReceta = await lasRecetas.BusqueLaRecetaBasica(laRecetaId, cancelacion);
        if (laReceta is null || !elControlDeAcceso.PuedeVerLaReceta(laReceta.Estado, laReceta.AutorId))
            throw ExcepcionDeNegocio.PorRecursoInexistente("La receta no existe.");

        var laPaginaDeComentarios = await lasInteracciones.ListeLosComentariosVisibles(laRecetaId,
            SolicitudDePagina.Normalice(laPagina, elTamano), cancelacion);
        var lasEstadisticas = await lasInteracciones.CalculeLasCalificaciones(laRecetaId, cancelacion);
        return new ComentariosPage(laPaginaDeComentarios.Items, laPaginaDeComentarios.Pagina, laPaginaDeComentarios.Tamano,
            laPaginaDeComentarios.Total, lasEstadisticas.Promedio, lasEstadisticas.Total);
    }

    public async Task<ComentarioDto> CalifiqueLaReceta(int laRecetaId, CalificarRequest laSolicitud, CancellationToken cancelacion)
    {
        var laReceta = await elControlDeAcceso.ExijaLaRecetaPublicada(laRecetaId, cancelacion);
        var elUsuarioId = elUsuarioActual.IdRequerido;
        if (laReceta.AutorId == elUsuarioId)
            throw ExcepcionDeNegocio.PorSolicitudInvalida("No puede calificar su propia receta.");

        short? laCalificacion = laSolicitud.Calificacion is null ? null : (short)laSolicitud.Calificacion;
        return await lasInteracciones.GuardeLaCalificacion(elUsuarioId, laRecetaId, laCalificacion,
            Texto.RecorteOMarqueNulo(laSolicitud.Comentario), cancelacion);
    }

    public async Task BorreLaCalificacion(int laRecetaId, CancellationToken cancelacion)
    {
        await elControlDeAcceso.ExijaLaRecetaExistente(laRecetaId, cancelacion);
        await lasInteracciones.BorreLaCalificacion(elUsuarioActual.IdRequerido, laRecetaId, cancelacion);
    }

    public async Task RegistreLaInteraccion(int laRecetaId, InteraccionRequest laSolicitud, CancellationToken cancelacion)
    {
        var elTipo = laSolicitud.Tipo?.Trim().ToUpperInvariant() ?? "";
        if (!InteraccionRequest.TIPOS_VALIDOS.Contains(elTipo))
            throw ExcepcionDeNegocio.PorSolicitudInvalida("Tipo inválido. Use: " + string.Join(", ", InteraccionRequest.TIPOS_VALIDOS) + ".");
        await elControlDeAcceso.ExijaLaRecetaPublicada(laRecetaId, cancelacion);
        await lasInteracciones.AgregueLaInteraccion(laRecetaId, elUsuarioActual.Id, elTipo, cancelacion);
    }

    public async Task<CompartirResponse> CompartaLaReceta(int laRecetaId, CompartirRequest laSolicitud, CancellationToken cancelacion)
    {
        await elControlDeAcceso.ExijaLaRecetaPublicada(laRecetaId, cancelacion);
        var laRed = await losCatalogos.BusqueLaRedSocial(laSolicitud.RedId, cancelacion)
                    ?? throw ExcepcionDeNegocio.PorSolicitudInvalida("La red social no existe.");

        await lasInteracciones.AgregueElCompartido(laRecetaId, elUsuarioActual.Id, laRed.RedId, cancelacion);

        var laUrl = $"{lasOpcionesDeEnlaces.UrlPublicaDeLaSpa.TrimEnd('/')}/recetas/{laRecetaId}";
        var laUrlCodificada = WebUtility.UrlEncode(laUrl);
        var elEnlace = laRed.Nombre.ToLowerInvariant() switch
        {
            "facebook" => $"https://www.facebook.com/sharer/sharer.php?u={laUrlCodificada}",
            "whatsapp" => $"https://wa.me/?text={laUrlCodificada}",
            "x" => $"https://twitter.com/intent/tweet?url={laUrlCodificada}",
            _ => null
        };
        return new CompartirResponse(laUrl, elEnlace);
    }
}
