using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Servicios;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Presentacion.Seguridad;

namespace Recetas.Api.Presentacion.Controladores;

[ApiController]
[Authorize(Policy = Politicas.SOLO_ADMIN)]
[Route("api/reportes")]
public class ReportesController(ServicioDeReportes losReportes, ServicioDelTablero elTablero) : ControllerBase
{
    [HttpGet]
    public IReadOnlyList<CatalogoReporteDto> ListeElCatalogo() => losReportes.ListeElCatalogo();

    [HttpGet("tablero")]
    public Task<TableroDto> ObtengaElTablero([FromQuery] ConsultaDeReporte laConsulta, CancellationToken cancelacion) =>
        elTablero.ObtengaElTablero(laConsulta, cancelacion);

    [HttpGet("descargas")]
    public Task<Paginado<DescargaDto>> ListeLasDescargas([FromQuery(Name = "pagina")] int? laPagina,
        [FromQuery(Name = "tamano")] int? elTamano, CancellationToken cancelacion) =>
        losReportes.ListeLasDescargas(laPagina, elTamano, cancelacion);

    [HttpGet("{tipo}")]
    public Task<ResultadoReporte> EjecuteElReporte([FromRoute(Name = "tipo")] string elTipo, [FromQuery] ConsultaDeReporte laConsulta,
        CancellationToken cancelacion) =>
        losReportes.EjecuteElReporte(elTipo, laConsulta, cancelacion);

    [HttpGet("{tipo}/descargar")]
    public async Task<IActionResult> DescargueElReporte([FromRoute(Name = "tipo")] string elTipo, [FromQuery] ConsultaDeReporte laConsulta,
        CancellationToken cancelacion)
    {
        var elArchivo = await losReportes.DescargueElReporte(elTipo, laConsulta, cancelacion);
        return File(elArchivo.Bytes, elArchivo.TipoDeContenido, elArchivo.Nombre);
    }
}
