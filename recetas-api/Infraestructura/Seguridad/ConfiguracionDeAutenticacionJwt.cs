using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Recetas.Api.Aplicacion.Puertos;

namespace Recetas.Api.Infraestructura.Seguridad;

public static class ConfiguracionDeAutenticacionJwt
{
    public static IServiceCollection AgregueLaAutenticacionJwt(this IServiceCollection losServicios, OpcionesJwt lasOpciones)
    {
        losServicios
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opciones =>
            {
                opciones.MapInboundClaims = false;
                opciones.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = lasOpciones.Issuer,
                    ValidateAudience = true,
                    ValidAudience = lasOpciones.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = lasOpciones.ClaveDeFirma,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    RoleClaimType = "rol",
                    NameClaimType = "sub"
                };
                opciones.Events = new JwtBearerEvents { OnTokenValidated = ValideLaSesion };
            });
        return losServicios;
    }

    private static async Task ValideLaSesion(TokenValidatedContext elContexto)
    {
        var losTokensRevocados = elContexto.HttpContext.RequestServices.GetRequiredService<IRevocadorDeTokens>();
        var elIdentificador = elContexto.Principal?.FindFirst("jti")?.Value;
        if (losTokensRevocados.EstaRevocado(elIdentificador)) { elContexto.Fail("Sesión cerrada."); return; }

        if (!int.TryParse(elContexto.Principal?.FindFirst("sub")?.Value, out var elUsuarioId))
        {
            elContexto.Fail("Token inválido.");
            return;
        }

        var losUsuarios = elContexto.HttpContext.RequestServices.GetRequiredService<IRepositorioDeUsuarios>();
        var elRolVigente = await losUsuarios.BusqueElRolDeLaCuentaActiva(elUsuarioId, elContexto.HttpContext.RequestAborted);
        if (elRolVigente is null) { elContexto.Fail("Cuenta inexistente o suspendida."); return; }

        if (elContexto.Principal?.Identity is ClaimsIdentity laIdentidad)
        {
            foreach (var elClaim in laIdentidad.FindAll("rol").ToList()) laIdentidad.RemoveClaim(elClaim);
            laIdentidad.AddClaim(new Claim("rol", elRolVigente));
        }
    }
}
