using Recetas.Api.Aplicacion;
using Recetas.Api.Infraestructura;
using Recetas.Api.Presentacion.Configuracion;

var elConstructor = WebApplication.CreateBuilder(args);

elConstructor.Services.AgregueLaAplicacion();
elConstructor.Services.AgregueLaInfraestructura(elConstructor.Configuration);
elConstructor.Services.AgregueLaPresentacion(elConstructor.Configuration);

var laAplicacion = elConstructor.Build();
laAplicacion.ConfigureElPipelineHttp();
laAplicacion.Run();

public partial class Program;
