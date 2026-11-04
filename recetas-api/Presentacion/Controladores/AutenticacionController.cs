using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Servicios;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Presentacion.Configuracion;
using Recetas.Api.Presentacion.Seguridad;

namespace Recetas.Api.Presentacion.Controladores;

[ApiController]
[Route("api/auth")]
public class AutenticacionController(ServicioDeAutenticacion laAutenticacion) : ControllerBase
{
    [HttpPost("registro")]
    [AllowAnonymous]
    [ProducesResponseType<UsuarioDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegistreAlUsuario(RegistroRequest laSolicitud, CancellationToken cancelacion) =>
        Created("/api/auth/me", await laAutenticacion.RegistreAlUsuario(laSolicitud, cancelacion));

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(LimitesDePeticiones.LOGIN)]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> InicieLaSesion(LoginRequest laSolicitud, CancellationToken cancelacion) =>
        Ok(await laAutenticacion.InicieLaSesion(laSolicitud, cancelacion));

    [HttpGet("me")]
    [Authorize(Policy = Politicas.AUTENTICADO)]
    [ProducesResponseType<UsuarioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObtengaAlUsuarioActual(CancellationToken cancelacion) =>
        Ok(await laAutenticacion.ObtengaAlUsuarioActual(cancelacion));

    [HttpPost("logout")]
    [Authorize(Policy = Politicas.AUTENTICADO)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult CierreLaSesion()
    {
        laAutenticacion.CierreLaSesion();
        return NoContent();
    }
}
