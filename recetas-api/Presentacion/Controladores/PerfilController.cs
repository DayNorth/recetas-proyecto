using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Servicios;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Presentacion.Seguridad;

namespace Recetas.Api.Presentacion.Controladores;

[ApiController]
[Authorize(Policy = Politicas.AUTENTICADO)]
[Route("api/perfil")]
public class PerfilController(ServicioDePerfil elPerfil) : ControllerBase
{
    [HttpGet]
    public Task<UsuarioDto> ObtengaElPerfil(CancellationToken cancelacion) => elPerfil.ObtengaElPerfil(cancelacion);

    [HttpPut]
    public Task<UsuarioDto> ActualiceElPerfil(ActualizarPerfilRequest laSolicitud, CancellationToken cancelacion) =>
        elPerfil.ActualiceElPerfil(laSolicitud, cancelacion);

    [HttpPost("foto")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public Task<UsuarioDto> CambieLaFoto([FromForm(Name = "foto")] IFormFile? laFoto, CancellationToken cancelacion) =>
        elPerfil.CambieLaFoto(laFoto.ConviertaAlArchivoSubido(), cancelacion);
}
