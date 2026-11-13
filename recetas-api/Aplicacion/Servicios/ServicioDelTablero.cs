using System.Globalization;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Reportes;
using Recetas.Api.Aplicacion.Solicitudes;

namespace Recetas.Api.Aplicacion.Servicios;

public class ServicioDelTablero(IRepositorioDeReportes losReportes, CalculadoraDeRangos laCalculadoraDeRangos,
    UmbralesTablero losUmbrales)
{
    public async Task<TableroDto> ObtengaElTablero(ConsultaDeReporte laConsulta, CancellationToken cancelacion)
    {
        var elRango = laCalculadoraDeRangos.ResuelvaElRango(laConsulta.Inicio, laConsulta.Fin, laConsulta.MesInicio, laConsulta.MesFin);
        var losHallazgos = new List<Hallazgo>();

        Task<ResultadoReporte> EjecuteElReporte(string elTipo, ParametrosReporte losParametros) =>
            losReportes.EjecuteElReporte(CatalogoReportes.Busque(elTipo)!, elRango, losParametros, cancelacion);

        var lasBusquedasSinResultado = await EjecuteElReporte("BUSQUEDAS_SIN_RESULTADO", new(Limite: 20));
        var lasFrecuentes = lasBusquedasSinResultado.Filas
            .Where(laFila => ConviertaADecimal(laFila["vecesBuscado"]) >= losUmbrales.MinBusquedasSinResultado).ToList();
        if (lasFrecuentes.Count > 0)
            losHallazgos.Add(new("BUSQUEDAS_SIN_RESULTADO", "Hay búsquedas frecuentes sin resultados", "media",
                $"{lasFrecuentes.Count} término(s) se buscaron al menos {losUmbrales.MinBusquedasSinResultado} veces y no devolvieron recetas.",
                "Crear (o importar) recetas para estos términos.",
                lasFrecuentes.Select(laFila => new ItemHallazgo((string)laFila["termino"]!, ConviertaADecimal(laFila["vecesBuscado"]), "veces buscado")).ToList()));

        var laOfertaYLaDemanda = await EjecuteElReporte("OFERTA_DEMANDA", new());
        var lasCategoriasConRecetas = laOfertaYLaDemanda.Filas.Where(laFila => laFila["visitasPorReceta"] is not null).ToList();
        var elPromedio = lasCategoriasConRecetas.Count == 0 ? 0m : lasCategoriasConRecetas.Average(laFila => ConviertaADecimal(laFila["visitasPorReceta"]));
        var lasDemandadas = lasCategoriasConRecetas
            .Where(laFila => elPromedio > 0 && ConviertaADecimal(laFila["visitasPorReceta"]) >= losUmbrales.FactorDemanda * elPromedio)
            .Select(laFila => new ItemHallazgo((string)laFila["categoria"]!, ConviertaADecimal(laFila["visitasPorReceta"]),
                $"visitas por receta (promedio {elPromedio:0.##})")).ToList();
        lasDemandadas.AddRange(laOfertaYLaDemanda.Filas
            .Where(laFila => laFila["visitasPorReceta"] is null && ConviertaADecimal(laFila["visitas"]) > 0)
            .Select(laFila => new ItemHallazgo((string)laFila["categoria"]!, ConviertaADecimal(laFila["visitas"]), "visitas y ninguna receta publicada")));
        if (lasDemandadas.Count > 0)
            losHallazgos.Add(new("CATEGORIA_CON_DEMANDA", "Categorías con más demanda que oferta", "media",
                $"Estas categorías reciben al menos {losUmbrales.FactorDemanda:0.##} veces las visitas por receta del promedio.",
                "Crear más recetas en estas categorías.", lasDemandadas));

        var laConversion = await EjecuteElReporte("CONVERSION", new(MinVisitas: losUmbrales.MinVisitasConversion));
        var lasVisitas = laConversion.Filas.Sum(laFila => ConviertaADecimal(laFila["visitas"]));
        var laConversionGlobal = lasVisitas == 0 ? 0m : laConversion.Filas.Sum(laFila => ConviertaADecimal(laFila["interacciones"])) * 100m / lasVisitas;
        var lasDeBajaConversion = laConversion.Filas
            .Where(laFila => laConversionGlobal > 0 && ConviertaADecimal(laFila["tasaConversion"]) < losUmbrales.FactorConversion * laConversionGlobal)
            .Select(laFila => new ItemHallazgo((string)laFila["titulo"]!, ConviertaADecimal(laFila["tasaConversion"]),
                $"% de conversión con {ConviertaADecimal(laFila["visitas"]):0} visitas (promedio {laConversionGlobal:0.##} %)")).ToList();
        if (lasDeBajaConversion.Count > 0)
            losHallazgos.Add(new("CONVERSION_BAJA", "Recetas con muchas visitas y poca conversión", "alta",
                $"Su conversión es menor al {losUmbrales.FactorConversion * 100:0} % del promedio general.",
                "Revisar la receta (fotos, pasos, ingredientes) o evaluar su moderación.", lasDeBajaConversion));

        var laTendencia = await EjecuteElReporte("TENDENCIA_MENSUAL", new());
        var elMesActual = laCalculadoraDeRangos.ObtengaLaFechaDeHoy().ToString("yyyy-MM");
        var lasCaidas = new List<ItemHallazgo>();
        for (var posicion = 1; posicion < laTendencia.Filas.Count; posicion++)
        {
            var elPeriodo = (string)laTendencia.Filas[posicion]["periodo"]!;
            if (elPeriodo == elMesActual) continue;
            var lasVisitasPrevias = ConviertaADecimal(laTendencia.Filas[posicion - 1]["visitas"]);
            var lasVisitasActuales = ConviertaADecimal(laTendencia.Filas[posicion]["visitas"]);
            if (lasVisitasPrevias > 0 && (lasVisitasPrevias - lasVisitasActuales) / lasVisitasPrevias > losUmbrales.CaidaMensual)
                lasCaidas.Add(new ItemHallazgo(elPeriodo, Math.Round((lasVisitasActuales - lasVisitasPrevias) * 100m / lasVisitasPrevias, 1),
                    $"% de variación en visitas ({lasVisitasPrevias:0} → {lasVisitasActuales:0})"));
        }
        if (lasCaidas.Count > 0)
            losHallazgos.Add(new("CAIDA_MENSUAL", "Caída de visitas frente al mes anterior", "media",
                $"Las visitas bajaron más de {losUmbrales.CaidaMensual * 100:0} % respecto al mes anterior.",
                "Planificar contenido y comunicación para esos meses.", lasCaidas));

        var lasMejoresCalificadas = await EjecuteElReporte("MEJOR_CALIFICADAS", new(MinVotos: losUmbrales.MinVotosPromocion));
        if (lasMejoresCalificadas.Filas.Count > 0)
        {
            var laMejor = lasMejoresCalificadas.Filas[0];
            losHallazgos.Add(new("RECETA_A_PROMOVER", "Receta mejor calificada", "info",
                $"Tiene el mejor promedio con al menos {losUmbrales.MinVotosPromocion} votos.", "Promoverla en la portada y en las noticias.",
                [new((string)laMejor["titulo"]!, ConviertaADecimal(laMejor["promedio"]), $"promedio con {ConviertaADecimal(laMejor["votos"]):0} votos")]));
        }

        return new TableroDto(elRango, losUmbrales, losHallazgos);
    }

    private static decimal ConviertaADecimal(object? elValor) => elValor is null ? 0m : Convert.ToDecimal(elValor, CultureInfo.InvariantCulture);
}
