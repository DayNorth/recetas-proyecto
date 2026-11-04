using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Reportes;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;

namespace Recetas.Api.Aplicacion.Servicios;

public record ArchivoDescargable(byte[] Bytes, string TipoDeContenido, string Nombre);

public class ServicioDeReportes(IRepositorioDeReportes losReportes, IExportadorDeReportes elExportador,
    CalculadoraDeRangos laCalculadoraDeRangos, IUsuarioActual elUsuarioActual, TimeProvider elReloj)
{
    private static readonly string[] FORMATOS_VALIDOS = ["csv", "xlsx", "pdf"];

    public IReadOnlyList<CatalogoReporteDto> ListeElCatalogo() =>
        CatalogoReportes.TODOS.Select(laDefinicion => new CatalogoReporteDto(laDefinicion.Tipo, laDefinicion.Titulo,
            laDefinicion.Pregunta, laDefinicion.ReglaLectura, laDefinicion.Accion,
            laDefinicion.Parametros.Select(ExpliqueElParametro).ToArray())).ToList();

    public Task<ResultadoReporte> EjecuteElReporte(string elTipo, ConsultaDeReporte laConsulta, CancellationToken cancelacion)
    {
        var (laDefinicion, losParametros) = PrepareElReporte(elTipo, laConsulta);
        return losReportes.EjecuteElReporte(laDefinicion, ResuelvaElRango(laConsulta), losParametros, cancelacion);
    }

    public async Task<ArchivoDescargable> DescargueElReporte(string elTipo, ConsultaDeReporte laConsulta, CancellationToken cancelacion)
    {
        var elFormato = laConsulta.Formato?.Trim().ToLowerInvariant();
        if (elFormato is null || !FORMATOS_VALIDOS.Contains(elFormato))
            throw ExcepcionDeNegocio.PorSolicitudInvalida("formato debe ser csv, xlsx o pdf.");

        var (laDefinicion, losParametros) = PrepareElReporte(elTipo, laConsulta);
        var elRango = ResuelvaElRango(laConsulta);
        var elResultado = await losReportes.EjecuteElReporte(laDefinicion, elRango, losParametros, cancelacion);

        var elContenido = elExportador.Exporte(elResultado, elFormato, elReloj.GetUtcNow().ToOffset(ZonaHoraria.COSTA_RICA));
        await losReportes.RegistreLaDescarga(elUsuarioActual.IdRequerido, laDefinicion.Tipo, elRango, elFormato.ToUpperInvariant(), cancelacion);

        var elNombre = $"reporte-{laDefinicion.Tipo.ToLowerInvariant()}-{elRango.Inicio:yyyyMMdd}-{elRango.Fin:yyyyMMdd}.{elFormato}";
        return new ArchivoDescargable(elContenido.Bytes, elContenido.TipoDeContenido, elNombre);
    }

    public Task<Paginado<DescargaDto>> ListeLasDescargas(int? laPagina, int? elTamano, CancellationToken cancelacion) =>
        losReportes.ListeLasDescargas(SolicitudDePagina.Normalice(laPagina, elTamano), cancelacion);

    public RangoReporte ResuelvaElRango(ConsultaDeReporte laConsulta) =>
        laCalculadoraDeRangos.ResuelvaElRango(laConsulta.Inicio, laConsulta.Fin, laConsulta.MesInicio, laConsulta.MesFin);

    private static (DefinicionDeReporte Definicion, ParametrosReporte Parametros) PrepareElReporte(string elTipo, ConsultaDeReporte laConsulta)
    {
        var laDefinicion = CatalogoReportes.Busque(elTipo) ?? throw ExcepcionDeNegocio.PorRecursoInexistente("Ese reporte no existe.");
        if (laConsulta.Limite is < 1 or > 100)
            throw ExcepcionDeNegocio.PorSolicitudInvalida("limite debe estar entre 1 y 100.");
        if (laConsulta.MinVotos is < 1 or > 1000)
            throw ExcepcionDeNegocio.PorSolicitudInvalida("minVotos debe estar entre 1 y 1000.");
        if (laConsulta.MinVisitas is < 1 or > 100000)
            throw ExcepcionDeNegocio.PorSolicitudInvalida("minVisitas debe ser mayor que 0.");
        return (laDefinicion, new ParametrosReporte(laConsulta.Limite ?? 10, laConsulta.MinVotos ?? 3, laConsulta.MinVisitas ?? 20));
    }

    private static string ExpliqueElParametro(string elParametro) => elParametro switch
    {
        "inicio" => "inicio|mesInicio",
        "fin" => "fin|mesFin",
        _ => elParametro
    };
}
