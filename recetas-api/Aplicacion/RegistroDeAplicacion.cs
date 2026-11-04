using Microsoft.Extensions.DependencyInjection;
using Recetas.Api.Aplicacion.Servicios;

namespace Recetas.Api.Aplicacion;

public static class RegistroDeAplicacion
{
    public static IServiceCollection AgregueLaAplicacion(this IServiceCollection losServicios)
    {
        losServicios.AddScoped<ControlDeAccesoARecetas>();
        losServicios.AddScoped<CalculadoraDeRangos>();

        losServicios.AddScoped<ServicioDeAutenticacion>();
        losServicios.AddScoped<ServicioDePerfil>();
        losServicios.AddScoped<ServicioDeCatalogos>();
        losServicios.AddScoped<ServicioDeAdministracion>();
        losServicios.AddScoped<ServicioDeConsultaDeRecetas>();
        losServicios.AddScoped<ServicioDeEscrituraDeRecetas>();
        losServicios.AddScoped<ServicioDeInteracciones>();
        losServicios.AddScoped<ServicioDeListas>();
        losServicios.AddScoped<ServicioDeMultimedia>();
        losServicios.AddScoped<ServicioDeNoticias>();
        losServicios.AddScoped<ServicioDeModeracion>();
        losServicios.AddScoped<ServicioDeReportes>();
        losServicios.AddScoped<ServicioDelTablero>();
        losServicios.AddScoped<ServicioDeSalud>();
        return losServicios;
    }
}
