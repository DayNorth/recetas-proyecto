using FluentValidation;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Presentacion.Filtros;
using Recetas.Api.Presentacion.Middleware;
using Recetas.Api.Presentacion.Seguridad;
using Scalar.AspNetCore;

namespace Recetas.Api.Presentacion.Configuracion;

public static class RegistroDePresentacion
{
    private const long MAXIMO_DE_BYTES_DEL_FORMULARIO = 12 * 1024 * 1024;

    public static IServiceCollection AgregueLaPresentacion(this IServiceCollection losServicios, IConfiguration laConfiguracion)
    {
        losServicios.AddHttpContextAccessor();
        losServicios.AddScoped<IUsuarioActual, UsuarioActual>();

        losServicios.AddAuthorizationBuilder()
            .AddPolicy(Politicas.AUTENTICADO, politica => politica.RequireAuthenticatedUser().RequireRole(Dominio.Roles.USUARIO, Dominio.Roles.ADMIN))
            .AddPolicy(Politicas.SOLO_ADMIN, politica => politica.RequireAuthenticatedUser().RequireRole(Dominio.Roles.ADMIN));

        losServicios.AddValidatorsFromAssemblyContaining<FiltroDeValidacion>();
        losServicios.AddProblemDetails();
        losServicios.AddExceptionHandler<ManejadorGlobalDeExcepciones>();
        losServicios.AgregueLosLimitesDePeticiones(laConfiguracion);
        losServicios.Configure<FormOptions>(opciones => opciones.MultipartBodyLengthLimit = MAXIMO_DE_BYTES_DEL_FORMULARIO);

        losServicios.AddControllers(opciones => opciones.Filters.Add<FiltroDeValidacion>());
        losServicios.AgregueLaDocumentacionOpenApi();

        var losOrigenesPermitidos = (laConfiguracion["Cors:Origins"] ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        losServicios.AddCors(opciones => opciones.AddDefaultPolicy(politica =>
            politica.WithOrigins(losOrigenesPermitidos).AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("Location")));
        return losServicios;
    }

    public static WebApplication ConfigureElPipelineHttp(this WebApplication laAplicacion)
    {
        laAplicacion.UseExceptionHandler();
        laAplicacion.UseStatusCodePages();
        laAplicacion.UseCors();

        var elAlmacen = laAplicacion.Services.GetRequiredService<IAlmacenDeArchivos>();
        Directory.CreateDirectory(elAlmacen.RutaRaiz);
        laAplicacion.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(elAlmacen.RutaRaiz),
            RequestPath = "/uploads"
        });

        laAplicacion.UseAuthentication();
        laAplicacion.UseRateLimiter();
        laAplicacion.UseAuthorization();

        if (laAplicacion.Environment.IsDevelopment())
        {
            laAplicacion.MapOpenApi();
            laAplicacion.MapScalarApiReference("/scalar");
        }

        laAplicacion.MapControllers();
        return laAplicacion;
    }
}
