using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Servicios;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Presentacion.Configuracion;
using Recetas.Api.Presentacion.Seguridad;

namespace Recetas.Api.Presentacion.Controladores;

[ApiController]
public class InteraccionController(ServicioDeInteracciones lasInteracciones) : ControllerBase
{

    [HttpPut("api/recetas/{id:int}/me-gusta")]
    [Authorize(Policy = Politicas.AUTENTICADO)]
    public Task<ToggleDto> DeMeGustaALaReceta([FromRoute(Name = "id")] int laRecetaId, CancellationToken cancelacion) =>
        lasInteracciones.DeMeGustaALaReceta(laRecetaId, cancelacion);

    [HttpDelete("api/recetas/{id:int}/me-gusta")]
    [Authorize(Policy = Politicas.AUTENTICADO)]
    public Task<ToggleDto> QuiteElMeGusta([FromRoute(Name = "id")] int laRecetaId, CancellationToken cancelacion) =>
        lasInteracciones.QuiteElMeGusta(laRecetaId, cancelacion);

    [HttpPut("api/recetas/{id:int}/favorito")]
    [Authorize(Policy = Politicas.AUTENTICADO)]
    public Task<ToggleDto> AgregueAFavoritos([FromRoute(Name = "id")] int laRecetaId, CancellationToken cancelacion) =>
        lasInteracciones.AgregueAFavoritos(laRecetaId, cancelacion);

    [HttpDelete("api/recetas/{id:int}/favorito")]
    [Authorize(Policy = Politicas.AUTENTICADO)]
    public Task<ToggleDto> QuiteDeFavoritos([FromRoute(Name = "id")] int laRecetaId, CancellationToken cancelacion) =>
        lasInteracciones.QuiteDeFavoritos(laRecetaId, cancelacion);

    [HttpGet("api/favoritos")]
    [Authorize(Policy = Politicas.AUTENTICADO)]
    public Task<Paginado<FavoritoDto>> ListeLosFavoritos([FromQuery(Name = "pagina")] int? laPagina,
        [FromQuery(Name = "tamano")] int? elTamano, CancellationToken cancelacion) =>
        lasInteracciones.ListeLosFavoritos(laPagina, elTamano, cancelacion);

    [HttpGet("api/recetas/{id:int}/comentarios")]
    [AllowAnonymous]
    public Task<ComentariosPage> ListeLosComentarios([FromRoute(Name = "id")] int laRecetaId, [FromQuery(Name = "pagina")] int? laPagina,
        [FromQuery(Name = "tamano")] int? elTamano, CancellationToken cancelacion) =>
        lasInteracciones.ListeLosComentarios(laRecetaId, laPagina, elTamano, cancelacion);

    [HttpPut("api/recetas/{id:int}/calificacion")]
    [Authorize(Policy = Politicas.AUTENTICADO)]
    public Task<ComentarioDto> CalifiqueLaReceta([FromRoute(Name = "id")] int laRecetaId, CalificarRequest laSolicitud,
        CancellationToken cancelacion) =>
        lasInteracciones.CalifiqueLaReceta(laRecetaId, laSolicitud, cancelacion);

    [HttpDelete("api/recetas/{id:int}/calificacion")]
    [Authorize(Policy = Politicas.AUTENTICADO)]
    public async Task<IActionResult> BorreLaCalificacion([FromRoute(Name = "id")] int laRecetaId, CancellationToken cancelacion)
    {
        await lasInteracciones.BorreLaCalificacion(laRecetaId, cancelacion);
        return NoContent();
    }

    [HttpPost("api/recetas/{id:int}/interacciones")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RegistreLaInteraccion([FromRoute(Name = "id")] int laRecetaId, InteraccionRequest laSolicitud,
        CancellationToken cancelacion)
    {
        await lasInteracciones.RegistreLaInteraccion(laRecetaId, laSolicitud, cancelacion);
        return NoContent();
    }

    [HttpPost("api/recetas/{id:int}/compartir")]
    [AllowAnonymous]
    [EnableRateLimiting(LimitesDePeticiones.COMPARTIR)]
    public Task<CompartirResponse> CompartaLaReceta([FromRoute(Name = "id")] int laRecetaId, CompartirRequest laSolicitud,
        CancellationToken cancelacion) =>
        lasInteracciones.CompartaLaReceta(laRecetaId, laSolicitud, cancelacion);
}
