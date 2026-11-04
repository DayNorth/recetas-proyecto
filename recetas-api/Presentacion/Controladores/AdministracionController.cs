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
[Route("api/admin")]
public class AdministracionController(ServicioDeAdministracion laAdministracion) : ControllerBase
{

    [HttpGet("categorias")]
    public Task<IReadOnlyList<CategoriaAdminDto>> ListeLasCategorias(CancellationToken cancelacion) =>
        laAdministracion.ListeLasCategorias(cancelacion);

    [HttpPost("categorias")]
    [ProducesResponseType<CategoriaAdminDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreeLaCategoria(CategoriaRequest laSolicitud, CancellationToken cancelacion)
    {
        var laCategoria = await laAdministracion.CreeLaCategoria(laSolicitud, cancelacion);
        return Created($"/api/admin/categorias/{laCategoria.CategoriaId}", laCategoria);
    }

    [HttpPut("categorias/{id:int}")]
    public Task<CategoriaAdminDto> EditeLaCategoria([FromRoute(Name = "id")] int laCategoriaId, CategoriaRequest laSolicitud,
        CancellationToken cancelacion) =>
        laAdministracion.EditeLaCategoria(laCategoriaId, laSolicitud, cancelacion);

    [HttpPut("categorias/{id:int}/estado")]
    public Task<CategoriaAdminDto> CambieElEstadoDeLaCategoria([FromRoute(Name = "id")] int laCategoriaId, EstadoRequest laSolicitud,
        CancellationToken cancelacion) =>
        laAdministracion.CambieElEstadoDeLaCategoria(laCategoriaId, laSolicitud.Activo, cancelacion);

    [HttpGet("usuarios")]
    public Task<Paginado<UsuarioAdminDto>> ListeLosUsuarios([FromQuery(Name = "q")] string? elTexto,
        [FromQuery(Name = "estado")] string? elEstado, [FromQuery(Name = "rol")] string? elRol,
        [FromQuery(Name = "pagina")] int? laPagina, [FromQuery(Name = "tamano")] int? elTamano, CancellationToken cancelacion) =>
        laAdministracion.ListeLosUsuarios(elTexto, elEstado, elRol, laPagina, elTamano, cancelacion);

    [HttpPut("usuarios/{id:int}/estado")]
    public Task<UsuarioAdminDto> CambieElEstadoDelUsuario([FromRoute(Name = "id")] int elUsuarioId, EstadoRequest laSolicitud,
        CancellationToken cancelacion) =>
        laAdministracion.CambieElEstadoDelUsuario(elUsuarioId, laSolicitud.Activo, cancelacion);
}
