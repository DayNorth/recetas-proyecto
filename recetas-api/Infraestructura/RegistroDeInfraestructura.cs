using Microsoft.EntityFrameworkCore;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Infraestructura.Archivos;
using Recetas.Api.Infraestructura.Persistencia;
using Recetas.Api.Infraestructura.Reportes;
using Recetas.Api.Infraestructura.Repositorios;
using Recetas.Api.Infraestructura.Seguridad;

namespace Recetas.Api.Infraestructura;

public static class RegistroDeInfraestructura
{
    private const int MINIMO_DE_CARACTERES_DE_LA_CLAVE_JWT = 32;
    private const string URL_DE_LA_SPA_POR_DEFECTO = "http://localhost:8080";

    public static IServiceCollection AgregueLaInfraestructura(this IServiceCollection losServicios, IConfiguration laConfiguracion)
    {
        AgregueLaPersistencia(losServicios, laConfiguracion);
        AgregueLaSeguridad(losServicios, laConfiguracion);
        AgregueLosArchivos(losServicios, laConfiguracion);
        AgregueLasOpcionesDeNegocio(losServicios);

        losServicios.AddSingleton(TimeProvider.System);
        losServicios.AddSingleton<IPuntoDeFalla, SinFallas>();
        losServicios.AddSingleton<IExportadorDeReportes, ExportadorDeReportes>();
        return losServicios;
    }

    private static void AgregueLaPersistencia(IServiceCollection losServicios, IConfiguration laConfiguracion)
    {
        var laCadenaDeConexion = laConfiguracion.GetConnectionString("Default")
                                 ?? throw new InvalidOperationException("Falta ConnectionStrings:Default.");

        losServicios.AddDbContext<RecetasDbContext>(opciones =>
            opciones.UseNpgsql(laCadenaDeConexion, npgsql =>
            {
                npgsql.EnableRetryOnFailure(maxRetryCount: 3);
            }));

        losServicios.AddScoped<IEjecutorDeTransacciones, EjecutorDeTransacciones>();
        losServicios.AddScoped<IRepositorioDeUsuarios, RepositorioDeUsuarios>();
        losServicios.AddScoped<IRepositorioDeCatalogos, RepositorioDeCatalogos>();
        losServicios.AddScoped<IRepositorioDeRecetas, RepositorioDeRecetas>();
        losServicios.AddScoped<IConsultasDeRecetas, ConsultasDeRecetas>();
        losServicios.AddScoped<IRepositorioDeInteracciones, RepositorioDeInteracciones>();
        losServicios.AddScoped<IRepositorioDeListas, RepositorioDeListas>();
        losServicios.AddScoped<IRepositorioDeMultimedia, RepositorioDeMultimedia>();
        losServicios.AddScoped<IRepositorioDeNoticias, RepositorioDeNoticias>();
        losServicios.AddScoped<IRepositorioDeModeracion, RepositorioDeModeracion>();
        losServicios.AddScoped<IRepositorioDeReportes, RepositorioDeReportes>();
        losServicios.AddScoped<IVerificadorDeSalud, VerificadorDeSaludPostgres>();
    }

    private static void AgregueLaSeguridad(IServiceCollection losServicios, IConfiguration laConfiguracion)
    {
        var lasOpcionesJwt = laConfiguracion.GetSection("Jwt").Get<OpcionesJwt>() ?? new OpcionesJwt();
        if (string.IsNullOrWhiteSpace(lasOpcionesJwt.Key) || lasOpcionesJwt.Key.Length < MINIMO_DE_CARACTERES_DE_LA_CLAVE_JWT)
            throw new InvalidOperationException("Jwt:Key debe tener al menos 32 caracteres.");

        losServicios.AddSingleton(lasOpcionesJwt);
        losServicios.AddSingleton<IRevocadorDeTokens, RevocadorDeTokensEnMemoria>();
        losServicios.AddSingleton<IServicioDeTokens, ServicioDeTokensJwt>();
        losServicios.AddSingleton<IHasheadorDeContrasenas, HasheadorBCrypt>();
        losServicios.AgregueLaAutenticacionJwt(lasOpcionesJwt);
    }

    private static void AgregueLosArchivos(IServiceCollection losServicios, IConfiguration laConfiguracion)
    {
        losServicios.AddSingleton(laConfiguracion.GetSection("Storage").Get<OpcionesDeAlmacenamiento>() ?? new OpcionesDeAlmacenamiento());
        losServicios.AddSingleton<IAlmacenDeArchivos, AlmacenLocalDeArchivos>();
    }

    private static void AgregueLasOpcionesDeNegocio(IServiceCollection losServicios)
    {
        losServicios.AddSingleton(proveedor => new OpcionesDeEnlaces(
            proveedor.GetRequiredService<IConfiguration>()["PublicUrl:Spa"] ?? URL_DE_LA_SPA_POR_DEFECTO));

        losServicios.AddSingleton(proveedor =>
        {
            var laConfiguracion = proveedor.GetRequiredService<IConfiguration>();
            return new UmbralesTablero(
                laConfiguracion.GetValue("Reportes:Tablero:MinBusquedasSinResultado", 3),
                laConfiguracion.GetValue("Reportes:Tablero:FactorDemanda", 1.5m),
                laConfiguracion.GetValue("Reportes:Tablero:MinVisitasConversion", 30),
                laConfiguracion.GetValue("Reportes:Tablero:FactorConversion", 0.5m),
                laConfiguracion.GetValue("Reportes:Tablero:CaidaMensual", 0.20m),
                laConfiguracion.GetValue("Reportes:Tablero:MinVotosPromocion", 5));
        });
    }
}
