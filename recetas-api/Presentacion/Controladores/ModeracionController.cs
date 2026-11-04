using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Servicios;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Presentacion.Seguridad;

namespace Recetas.Api.Presentacion.Controladores;

[ApiController]
[Authorize(Policy = Politicas.AUTENTICADO)]
[Route("api/reportes-contenido")]
public class DenunciasController(ServicioDeModeracion laModeracion) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ReporteCreadoDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> DenuncieElContenido(CrearReporteContenidoRequest laSolicitud, CancellationToken cancelacion)
    {
        var laDenuncia = await laModeracion.DenuncieElContenido(laSolicitud, cancelacion);
        return Created($"/api/moderacion/reportes/{laDenuncia.ReporteContenidoId}", laDenuncia);
    }
}

[ApiController]
[Authorize(Policy = Politicas.SOLO_ADMIN)]
[Route("api/moderacion")]
public class ModeracionController(ServicioDeModeracion laModeracion) : ControllerBase
{

    [HttpGet("reportes")]
    public Task<Paginado<ReporteListadoDto>> ListeLasDenuncias([FromQuery(Name = "estado")] string? elEstado,
        [FromQuery(Name = "pagina")] int? laPagina, [FromQuery(Name = "tamano")] int? elTamano, CancellationToken cancelacion) =>
        laModeracion.ListeLasDenuncias(elEstado, laPagina, elTamano, cancelacion);

    [HttpGet("reportes/{id:int}")]
    public Task<ReporteDetalleDto> ObtengaLaDenuncia([FromRoute(Name = "id")] int laDenunciaId, CancellationToken cancelacion) =>
        laModeracion.ObtengaLaDenuncia(laDenunciaId, cancelacion);

    [HttpPut("reportes/{id:int}/resolver")]
    public Task<ResolucionDto> ResuelvaLaDenuncia([FromRoute(Name = "id")] int laDenunciaId, ResolverReporteRequest laSolicitud,
        CancellationToken cancelacion) =>
        laModeracion.ResuelvaLaDenuncia(laDenunciaId, laSolicitud, cancelacion);

    [HttpPost("recetas/{id:int}/retirar")]
    public Task<EstadoContenidoDto> RetireLaReceta([FromRoute(Name = "id")] int laRecetaId, MotivoRequest laSolicitud,
        CancellationToken cancelacion) =>
        laModeracion.RetireLaReceta(laRecetaId, laSolicitud, cancelacion);

    [HttpPost("recetas/{id:int}/restaurar")]
    public Task<EstadoContenidoDto> RestaureLaReceta([FromRoute(Name = "id")] int laRecetaId, MotivoRequest laSolicitud,
        CancellationToken cancelacion) =>
        laModeracion.RestaureLaReceta(laRecetaId, laSolicitud, cancelacion);

    [HttpPost("comentarios/{id:int}/retirar")]
    public Task<EstadoContenidoDto> RetireElComentario([FromRoute(Name = "id")] int elComentarioId, MotivoRequest laSolicitud,
        CancellationToken cancelacion) =>
        laModeracion.RetireElComentario(elComentarioId, laSolicitud, cancelacion);

    [HttpPost("comentarios/{id:int}/restaurar")]
    public Task<EstadoContenidoDto> RestaureElComentario([FromRoute(Name = "id")] int elComentarioId, MotivoRequest laSolicitud,
        CancellationToken cancelacion) =>
        laModeracion.RestaureElComentario(elComentarioId, laSolicitud, cancelacion);

    [HttpGet("historial")]
    public Task<Paginado<HistorialDto>> ListeElHistorial([FromQuery(Name = "recetaId")] int? laRecetaId,
        [FromQuery(Name = "comentarioId")] int? elComentarioId, [FromQuery(Name = "desde")] DateOnly? desde,
        [FromQuery(Name = "hasta")] DateOnly? hasta, [FromQuery(Name = "pagina")] int? laPagina,
        [FromQuery(Name = "tamano")] int? elTamano, CancellationToken cancelacion) =>
        laModeracion.ListeElHistorial(new FiltroDelHistorial(laRecetaId, elComentarioId, desde, hasta), laPagina, elTamano, cancelacion);
}
