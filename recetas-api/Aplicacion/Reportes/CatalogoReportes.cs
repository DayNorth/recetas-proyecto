namespace Recetas.Api.Aplicacion.Reportes;

public record DefinicionDeReporte(string Tipo, string Titulo, string Pregunta, string ReglaLectura, string Accion,
    string[] Parametros);

public static class CatalogoReportes
{
    public const string RANGO = "inicio, fin (o mesInicio, mesFin)";

    public static readonly IReadOnlyList<DefinicionDeReporte> TODOS =
    [
        new("MAS_VISTAS", "Recetas más vistas",
            "¿Qué recetas atraen más tráfico?",
            "Las primeras filas concentran la atención. Una receta muy vista con pocos me gusta atrae pero no convence (ver Conversión).",
            "Destacar estas recetas en la portada y usarlas como modelo de títulos y fotos.",
            ["inicio", "fin", "limite"]),

        new("MAS_GUSTADAS", "Recetas con más me gusta",
            "¿Qué recetas convencen a los usuarios?",
            "Más me gusta indica agrado, independientemente de cuánto se visite.",
            "Replicar el estilo, la categoría y la dificultad de estas recetas.",
            ["inicio", "fin", "limite"]),

        new("MAS_BUSCADAS", "Términos más buscados",
            "¿Qué escribe la gente en el buscador?",
            "Los términos repetidos muestran intereses; compárelos con las recetas que existen.",
            "Ajustar títulos, descripciones y categorías para que coincidan con lo que se busca.",
            ["inicio", "fin", "limite"]),

        new("RECETAS_MAS_BUSCADAS", "Recetas más abiertas desde una búsqueda",
            "¿Qué recetas se encuentran activamente buscándolas?",
            "Cuenta las visitas que vinieron de una búsqueda (no las de acceso directo). Muchos términos distintos indican buena visibilidad.",
            "Reforzar los títulos y etiquetas de las recetas que más se abren desde el buscador.",
            ["inicio", "fin", "limite"]),

        new("MEJOR_CALIFICADAS", "Recetas mejor calificadas",
            "¿Dónde está la calidad?",
            "Se ordena por promedio y solo entran recetas con un mínimo de votos (minVotos) para evitar sesgo.",
            "Promover las mejor calificadas y revisar las de nota baja.",
            ["inicio", "fin", "minVotos"]),

        new("POR_CATEGORIA", "Desempeño por categoría",
            "¿Cómo rinde cada categoría?",
            "Compare recetas publicadas contra visitas y me gusta: una categoría con muchas visitas y pocas recetas tiene demanda sin cubrir.",
            "Decidir en qué categorías conviene publicar más contenido.",
            ["inicio", "fin"]),

        new("TENDENCIA_MENSUAL", "Tendencia mensual de actividad",
            "¿El uso del sitio crece o cae?",
            "Un mes con una caída fuerte frente al anterior merece explicación. Los meses sin actividad aparecen en cero.",
            "Planificar contenido y comunicaciones por temporada.",
            ["inicio", "fin"]),

        new("BUSQUEDAS_SIN_RESULTADO", "Búsquedas sin resultados",
            "¿Qué demanda no estamos cubriendo?",
            "Cada fila es un término que se buscó y no devolvió recetas; más veces significa más demanda insatisfecha.",
            "Crear (o importar) recetas para los términos con más repeticiones.",
            ["inicio", "fin", "limite"]),

        new("CONVERSION", "Conversión de visitas a me gusta",
            "¿Qué recetas se ven mucho pero gustan poco?",
            "Tasa = me gusta por cada 100 visitas, de menor a mayor. Solo entran recetas con al menos minVisitas visitas.",
            "Revisar la receta (fotos, pasos, ingredientes) o moderarla si la calificación también es baja.",
            ["inicio", "fin", "minVisitas"]),

        new("OFERTA_DEMANDA", "Oferta frente a demanda por categoría",
            "¿Dónde falta contenido?",
            "Visitas por receta alto significa mucha demanda para poca oferta. Sin recetas publicadas y con demanda, la razón queda vacía y sale primero.",
            "Crear más recetas en las categorías con más visitas por receta.",
            ["inicio", "fin"]),
    ];

    public static DefinicionDeReporte? Busque(string elTipo) =>
        TODOS.FirstOrDefault(laDefinicion => laDefinicion.Tipo.Equals(elTipo, StringComparison.OrdinalIgnoreCase));

    private static readonly Dictionary<string, string> TITULOS = new()
    {
        ["receta_id"] = "ID", ["titulo"] = "Receta", ["categoria"] = "Categoría", ["visitas"] = "Visitas",
        ["me_gusta"] = "Me gusta", ["termino"] = "Término", ["veces_buscado"] = "Veces buscado",
        ["visitas_desde_busqueda"] = "Visitas desde búsqueda", ["terminos_distintos"] = "Términos distintos",
        ["promedio"] = "Promedio", ["votos"] = "Votos", ["recetas_publicadas"] = "Recetas publicadas",
        ["periodo"] = "Mes", ["favoritos"] = "Favoritos", ["comentarios"] = "Comentarios", ["compartidos"] = "Compartidos",
        ["busquedas"] = "Búsquedas", ["tasa_conversion"] = "Conversión (%)", ["calificacion_promedio"] = "Calificación promedio",
        ["visitas_por_receta"] = "Visitas por receta", ["busquedas_sin_resultado"] = "Búsquedas sin resultado"
    };

    public static string ObtengaElTituloDeLaColumna(string elCampoSql) =>
        TITULOS.TryGetValue(elCampoSql, out var elTitulo) ? elTitulo : elCampoSql.Replace('_', ' ');

    public static string ConviertaACamelCase(string elTextoEnSnakeCase)
    {
        var lasPartes = elTextoEnSnakeCase.Split('_', StringSplitOptions.RemoveEmptyEntries);
        return lasPartes[0] + string.Concat(lasPartes.Skip(1).Select(laParte => char.ToUpperInvariant(laParte[0]) + laParte[1..]));
    }
}
