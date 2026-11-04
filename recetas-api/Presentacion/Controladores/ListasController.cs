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
[Route("api/listas")]
public class ListasController(ServicioDeListas lasListas) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ListaDetalleDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreeLaLista(CrearListaRequest laSolicitud, CancellationToken cancelacion)
    {
        var laLista = await lasListas.CreeLaLista(laSolicitud, cancelacion);
        return CreatedAtAction(nameof(ObtengaLaLista), new { id = laLista.ListaId }, laLista);
    }

    [HttpGet]
    public Task<Paginado<ListaResumenDto>> ListeLasListas([FromQuery(Name = "estado")] string? elEstado,
        [FromQuery(Name = "pagina")] int? laPagina, [FromQuery(Name = "tamano")] int? elTamano, CancellationToken cancelacion) =>
        lasListas.ListeLasListas(elEstado, laPagina, elTamano, cancelacion);

    [HttpGet("{id:int}")]
    public Task<ListaDetalleDto> ObtengaLaLista([FromRoute(Name = "id")] int laListaId, CancellationToken cancelacion) =>
        lasListas.ObtengaLaLista(laListaId, cancelacion);

    [HttpPatch("{id:int}/items/{itemId:int}")]
    public Task<ItemActualizadoDto> MarqueElItem([FromRoute(Name = "id")] int laListaId, [FromRoute(Name = "itemId")] int elItemId,
        MarcarItemRequest laSolicitud, CancellationToken cancelacion) =>
        lasListas.MarqueElItem(laListaId, elItemId, laSolicitud, cancelacion);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> BorreLaLista([FromRoute(Name = "id")] int laListaId, CancellationToken cancelacion)
    {
        await lasListas.BorreLaLista(laListaId, cancelacion);
        return NoContent();
    }
}
