using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace Recetas.Api.Presentacion.Configuracion;

public static class LimitesDePeticiones
{
    public const string LOGIN = "login";
    public const string COMPARTIR = "compartir";

    private const int LIMITE_GENERAL_POR_DEFECTO = 120;
    private const int LIMITE_DE_LOGIN_POR_DEFECTO = 5;
    private const int LIMITE_DE_COMPARTIR_POR_DEFECTO = 10;

    public static IServiceCollection AgregueLosLimitesDePeticiones(this IServiceCollection losServicios, IConfiguration laConfiguracion)
    {
        losServicios.AddRateLimiter(opciones =>
        {
            opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            opciones.OnRejected = async (elContexto, cancelacion) =>
            {
                elContexto.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await elContexto.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>().TryWriteAsync(new ProblemDetailsContext
                {
                    HttpContext = elContexto.HttpContext,
                    ProblemDetails = new ProblemDetails { Status = 429, Title = "Demasiadas peticiones; intente más tarde." }
                });
            };

            opciones.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(elContexto =>
                CreeLaVentanaPorIp(elContexto, laConfiguracion.GetValue("RateLimit:General", LIMITE_GENERAL_POR_DEFECTO)));
            opciones.AddPolicy(LOGIN, elContexto =>
                CreeLaVentanaPorIp(elContexto, laConfiguracion.GetValue("RateLimit:Login", LIMITE_DE_LOGIN_POR_DEFECTO)));
            opciones.AddPolicy(COMPARTIR, elContexto =>
                CreeLaVentanaPorIp(elContexto, laConfiguracion.GetValue("RateLimit:Compartir", LIMITE_DE_COMPARTIR_POR_DEFECTO)));
        });
        return losServicios;
    }

    private static RateLimitPartition<string> CreeLaVentanaPorIp(HttpContext elContexto, int elLimite) =>
        RateLimitPartition.GetFixedWindowLimiter(ObtengaLaIp(elContexto),
            _ => new FixedWindowRateLimiterOptions { PermitLimit = elLimite, Window = TimeSpan.FromMinutes(1) });

    private static string ObtengaLaIp(HttpContext elContexto) => elContexto.Connection.RemoteIpAddress?.ToString() ?? "desconocida";
}
