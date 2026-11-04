using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recetas.Api.Aplicacion.Servicios;

namespace Recetas.Api.Presentacion.Controladores;

[ApiController]
[AllowAnonymous]
[Route("api/health")]
public class SaludController(ServicioDeSalud laSalud, ILogger<SaludController> elRegistro) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObtengaElEstado(CancellationToken cancelacion)
    {
        try
        {
            var elInforme = await laSalud.Verifique(cancelacion);
            return Ok(new
            {
                estado = "ok",
                hora = DateTimeOffset.Now,
                baseDeDatos = new
                {
                    conectada = true,
                    version = elInforme.Version,
                    tablas = elInforme.Tablas,
                    recetas = elInforme.Recetas,
                    usuarios = elInforme.Usuarios,
                    recetaMasVistaEnElRango = elInforme.RecetaMasVistaEnElRango
                }
            });
        }
        catch (Exception laFalla)
        {
            elRegistro.LogError(laFalla, "Falla al comprobar la base de datos");
            return StatusCode(503, new { estado = "error", baseDeDatos = new { conectada = false } });
        }
    }
}
