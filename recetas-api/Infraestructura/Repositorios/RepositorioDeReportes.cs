using Microsoft.EntityFrameworkCore;
using Npgsql;
using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Reportes;
using Recetas.Api.Infraestructura.Persistencia;
using Recetas.Api.Infraestructura.Persistencia.Entidades;

namespace Recetas.Api.Infraestructura.Repositorios;

public class RepositorioDeReportes(RecetasDbContext laBaseDeDatos) : IRepositorioDeReportes
{
    private static readonly IReadOnlyDictionary<string, string> CONSULTAS_POR_TIPO = new Dictionary<string, string>
    {
        ["MAS_VISTAS"] = "SELECT * FROM sp_reporte_mas_vistas(@inicio, @fin, @limite)",
        ["MAS_GUSTADAS"] = "SELECT * FROM sp_reporte_mas_gustadas(@inicio, @fin, @limite)",
        ["MAS_BUSCADAS"] = "SELECT * FROM sp_reporte_mas_buscadas(@inicio, @fin, @limite)",
        ["RECETAS_MAS_BUSCADAS"] = "SELECT * FROM sp_reporte_recetas_mas_buscadas(@inicio, @fin, @limite)",
        ["MEJOR_CALIFICADAS"] = "SELECT * FROM sp_reporte_mejor_calificadas(@inicio, @fin, @minVotos)",
        ["POR_CATEGORIA"] = "SELECT * FROM sp_reporte_por_categoria(@inicio, @fin)",
        ["TENDENCIA_MENSUAL"] = "SELECT * FROM sp_reporte_tendencia_mensual(@inicio, @fin)",
        ["BUSQUEDAS_SIN_RESULTADO"] = "SELECT * FROM sp_reporte_busquedas_sin_resultado(@inicio, @fin, @limite)",
        ["CONVERSION"] = "SELECT * FROM sp_reporte_conversion(@inicio, @fin, @minVisitas)",
        ["OFERTA_DEMANDA"] = "SELECT * FROM sp_reporte_oferta_demanda_categoria(@inicio, @fin)"
    };

    public async Task<ResultadoReporte> EjecuteElReporte(DefinicionDeReporte laDefinicion, RangoReporte elRango,
        ParametrosReporte losParametros, CancellationToken cancelacion)
    {
        await laBaseDeDatos.Database.OpenConnectionAsync(cancelacion);
        try
        {
            await using var elComando = (NpgsqlCommand)laBaseDeDatos.Database.GetDbConnection().CreateCommand();
            elComando.CommandText = CONSULTAS_POR_TIPO[laDefinicion.Tipo];
            elComando.Parameters.AddWithValue("inicio", NpgsqlTypes.NpgsqlDbType.Date, elRango.Inicio);
            elComando.Parameters.AddWithValue("fin", NpgsqlTypes.NpgsqlDbType.Date, elRango.Fin);
            if (laDefinicion.Parametros.Contains("limite")) elComando.Parameters.AddWithValue("limite", losParametros.Limite);
            if (laDefinicion.Parametros.Contains("minVotos")) elComando.Parameters.AddWithValue("minVotos", losParametros.MinVotos);
            if (laDefinicion.Parametros.Contains("minVisitas")) elComando.Parameters.AddWithValue("minVisitas", losParametros.MinVisitas);

            await using var elLector = await elComando.ExecuteReaderAsync(cancelacion);
            var lasColumnas = new List<ColumnaReporte>();
            for (var posicion = 0; posicion < elLector.FieldCount; posicion++)
            {
                var elTipoDelCampo = elLector.GetFieldType(posicion);
                var elTipo = elTipoDelCampo == typeof(string) ? "texto"
                    : (elTipoDelCampo == typeof(decimal) || elTipoDelCampo == typeof(double)) ? "decimal"
                    : "entero";
                lasColumnas.Add(new ColumnaReporte(CatalogoReportes.ConviertaACamelCase(elLector.GetName(posicion)),
                    CatalogoReportes.ObtengaElTituloDeLaColumna(elLector.GetName(posicion)), elTipo));
            }

            var lasFilas = new List<Dictionary<string, object?>>();
            while (await elLector.ReadAsync(cancelacion))
            {
                var laFila = new Dictionary<string, object?>();
                for (var posicion = 0; posicion < elLector.FieldCount; posicion++)
                    laFila[lasColumnas[posicion].Campo] = elLector.IsDBNull(posicion) ? null : elLector.GetValue(posicion);
                lasFilas.Add(laFila);
            }
            return new ResultadoReporte(laDefinicion.Tipo, laDefinicion.Titulo, elRango, lasColumnas, lasFilas,
                new DecisionReporte(laDefinicion.Pregunta, laDefinicion.ReglaLectura, laDefinicion.Accion));
        }
        finally { await laBaseDeDatos.Database.CloseConnectionAsync(); }
    }

    public Task<Paginado<DescargaDto>> ListeLasDescargas(SolicitudDePagina laPagina, CancellationToken cancelacion) =>
        laBaseDeDatos.ReportesGenerados.AsNoTracking().OrderByDescending(descarga => descarga.FechaGeneracion)
            .ThenByDescending(descarga => descarga.ReporteId)
            .Select(descarga => new DescargaDto(descarga.ReporteId, descarga.Usuario.Nombre + " " + descarga.Usuario.Apellidos,
                descarga.TipoReporte, descarga.FechaInicio, descarga.FechaFin, descarga.Formato, descarga.FechaGeneracion))
            .PagineLaConsulta(laPagina, cancelacion);

    public async Task RegistreLaDescarga(int elUsuarioId, string elTipo, RangoReporte elRango, string elFormato, CancellationToken cancelacion)
    {
        laBaseDeDatos.ReportesGenerados.Add(new ReporteGenerado
        {
            UsuarioId = elUsuarioId,
            TipoReporte = elTipo,
            FechaInicio = elRango.Inicio,
            FechaFin = elRango.Fin,
            Formato = elFormato
        });
        await laBaseDeDatos.SaveChangesAsync(cancelacion);
    }
}
