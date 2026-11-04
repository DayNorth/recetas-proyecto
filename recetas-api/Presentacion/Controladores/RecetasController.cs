using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Servicios;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;
using Recetas.Api.Presentacion.Seguridad;

namespace Recetas.Api.Presentacion.Controladores;

[ApiController]
[Route("api/recetas")]
public class RecetasController(ServicioDeConsultaDeRecetas laConsulta, ServicioDeEscrituraDeRecetas laEscritura,
    IValidator<CrearRecetaRequest> elValidadorDeRecetas) : ControllerBase
{
    private static readonly JsonSerializerOptions OPCIONES_JSON = new(JsonSerializerDefaults.Web);

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<ResultadoBusqueda>(StatusCodes.Status200OK)]
    public Task<ResultadoBusqueda> BusqueLasRecetas(
        [FromQuery(Name = "q")] string? elTexto, [FromQuery(Name = "categoriaId")] short? laCategoriaId,
        [FromQuery(Name = "dificultadId")] short? laDificultadId, [FromQuery(Name = "tiempoMax")] int? elTiempoMaximo,
        [FromQuery(Name = "ingredientes")] int[]? losIngredientes, [FromQuery(Name = "orden")] string? elOrden,
        [FromQuery(Name = "registrar")] bool seRegistra, [FromQuery(Name = "pagina")] int? laPagina,
        [FromQuery(Name = "tamano")] int? elTamano, CancellationToken cancelacion) =>
        laConsulta.BusqueLasRecetas(new ConsultaDelCatalogo(elTexto, laCategoriaId, laDificultadId, elTiempoMaximo, losIngredientes,
            elOrden, seRegistra, laPagina, elTamano), cancelacion);

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType<RecetaDetalleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<RecetaDetalleDto> ObtengaElDetalleDeLaReceta([FromRoute(Name = "id")] int laRecetaId,
        [FromQuery(Name = "busquedaId")] long? laBusquedaId, CancellationToken cancelacion) =>
        laConsulta.ObtengaElDetalle(laRecetaId, laBusquedaId, cancelacion);

    [HttpGet("{id:int}/ingredientes")]
    [AllowAnonymous]
    [ProducesResponseType<IngredientesPorPorcionesDto>(StatusCodes.Status200OK)]
    public Task<IngredientesPorPorcionesDto> CalculeLosIngredientesPorPorciones([FromRoute(Name = "id")] int laRecetaId,
        [FromQuery(Name = "porciones")] int? lasPorciones, CancellationToken cancelacion) =>
        laConsulta.CalculeLosIngredientesPorPorciones(laRecetaId, lasPorciones, cancelacion);

    [HttpGet("mias")]
    [Authorize(Policy = Politicas.AUTENTICADO)]
    [ProducesResponseType<Paginado<RecetaResumenDto>>(StatusCodes.Status200OK)]
    public Task<Paginado<RecetaResumenDto>> ListeMisRecetas([FromQuery(Name = "estado")] string? elEstado,
        [FromQuery(Name = "pagina")] int? laPagina, [FromQuery(Name = "tamano")] int? elTamano, CancellationToken cancelacion) =>
        laConsulta.ListeMisRecetas(elEstado, laPagina, elTamano, cancelacion);

    [HttpPost]
    [Authorize(Policy = Politicas.AUTENTICADO)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(12 * 1024 * 1024)]
    [ProducesResponseType<RecetaCreadaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegistreLaReceta([FromForm(Name = "datos")] string? losDatos,
        [FromForm(Name = "imagen")] IFormFile? laImagen, CancellationToken cancelacion)
    {
        var laSolicitud = LeaLaSolicitudDeLaReceta(losDatos);
        await elValidadorDeRecetas.ValidateAndThrowAsync(laSolicitud, cancelacion);

        var laRecetaId = await laEscritura.RegistreLaReceta(laSolicitud, laImagen.ConviertaAlArchivoSubido(), cancelacion);
        return CreatedAtAction(nameof(ObtengaElDetalleDeLaReceta), new { id = laRecetaId }, new RecetaCreadaDto(laRecetaId));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.AUTENTICADO)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ActualiceLaReceta([FromRoute(Name = "id")] int laRecetaId, CrearRecetaRequest laSolicitud,
        CancellationToken cancelacion)
    {
        await laEscritura.ActualiceLaReceta(laRecetaId, laSolicitud, cancelacion);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Politicas.AUTENTICADO)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DesactiveLaReceta([FromRoute(Name = "id")] int laRecetaId, CancellationToken cancelacion)
    {
        await laEscritura.DesactiveLaReceta(laRecetaId, cancelacion);
        return NoContent();
    }

    private static CrearRecetaRequest LeaLaSolicitudDeLaReceta(string? losDatos)
    {
        try
        {
            return JsonSerializer.Deserialize<CrearRecetaRequest>(losDatos ?? "", OPCIONES_JSON) ?? throw new JsonException();
        }
        catch (JsonException)
        {
            throw ExcepcionDeNegocio.PorSolicitudInvalida("El campo \"datos\" debe ser un JSON válido de la receta.");
        }
    }
}
