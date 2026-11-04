using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Servicios;
using Recetas.Api.Presentacion.Seguridad;

namespace Recetas.Api.Presentacion.Controladores;

[ApiController]
[Authorize(Policy = Politicas.AUTENTICADO)]
[Route("api/recetas/{id:int}/multimedia")]
public class MultimediaController(ServicioDeMultimedia laMultimedia) : ControllerBase
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(12 * 1024 * 1024)]
    [ProducesResponseType<MultimediaDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> AgregueElArchivo([FromRoute(Name = "id")] int laRecetaId,
        [FromForm(Name = "archivo")] IFormFile? elArchivo, [FromForm(Name = "url")] string? laUrl,
        [FromForm(Name = "descripcion")] string? laDescripcion, CancellationToken cancelacion)
    {
        var elArchivoAgregado = await laMultimedia.AgregueElArchivo(laRecetaId, elArchivo.ConviertaAlArchivoSubido(), laUrl,
            laDescripcion, cancelacion);
        return Created($"/api/recetas/{laRecetaId}", elArchivoAgregado);
    }

    [HttpDelete("{mid:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> BorreElArchivo([FromRoute(Name = "id")] int laRecetaId,
        [FromRoute(Name = "mid")] int elArchivoId, CancellationToken cancelacion)
    {
        await laMultimedia.BorreElArchivo(laRecetaId, elArchivoId, cancelacion);
        return NoContent();
    }

    [HttpPut("{mid:int}/portada")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DefinaLaPortada([FromRoute(Name = "id")] int laRecetaId,
        [FromRoute(Name = "mid")] int elArchivoId, CancellationToken cancelacion)
    {
        await laMultimedia.DefinaLaPortada(laRecetaId, elArchivoId, cancelacion);
        return NoContent();
    }
}
