using System.Security.Claims;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Dominio;

namespace Recetas.Api.Presentacion.Seguridad;

public static class Politicas
{
    public const string AUTENTICADO = "Autenticado";
    public const string SOLO_ADMIN = "SoloAdmin";
}

public class UsuarioActual(IHttpContextAccessor elAccesorDelContexto) : IUsuarioActual
{
    private ClaimsPrincipal? Principal => elAccesorDelContexto.HttpContext?.User;

    public bool EstaAutenticado => Principal?.Identity?.IsAuthenticated == true && Id is not null;

    public int? Id => int.TryParse(Principal?.FindFirstValue("sub"), out var elId) ? elId : null;

    public string? Rol => Principal?.FindFirstValue("rol");

    public bool EsAdministrador => Rol == Roles.ADMIN;

    public int IdRequerido => Id ?? throw new InvalidOperationException("No hay un usuario autenticado.");

    public string? IdentificadorDelToken => Principal?.FindFirstValue("jti");

    public DateTimeOffset? ExpiracionDelToken =>
        long.TryParse(Principal?.FindFirstValue("exp"), out var laExpiracion) ? DateTimeOffset.FromUnixTimeSeconds(laExpiracion) : null;
}
