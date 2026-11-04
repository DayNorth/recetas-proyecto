namespace Recetas.Api.Aplicacion.Dtos;

public record CatalogoReporteDto(string Tipo, string Titulo, string Pregunta, string ReglaLectura, string Accion, string[] Parametros);
public record DescargaDto(int ReporteId, string Usuario, string TipoReporte, DateOnly FechaInicio, DateOnly FechaFin, string Formato, DateTime FechaGeneracion);

public record RangoReporte(DateOnly Inicio, DateOnly Fin);
public record ColumnaReporte(string Campo, string Titulo, string Tipo);
public record DecisionReporte(string Pregunta, string ReglaLectura, string Accion);
public record ResultadoReporte(string Tipo, string Titulo, RangoReporte Rango, IReadOnlyList<ColumnaReporte> Columnas,
    IReadOnlyList<Dictionary<string, object?>> Filas, DecisionReporte Decision);

public record ParametrosReporte(int Limite = 10, int MinVotos = 3, int MinVisitas = 20);

public record ItemHallazgo(string Etiqueta, decimal? Valor, string? Nota);
public record Hallazgo(string Codigo, string Titulo, string Severidad, string Detalle, string Accion, IReadOnlyList<ItemHallazgo> Items);
public record UmbralesTablero(int MinBusquedasSinResultado, decimal FactorDemanda, int MinVisitasConversion,
    decimal FactorConversion, decimal CaidaMensual, int MinVotosPromocion);
public record TableroDto(RangoReporte Rango, UmbralesTablero Umbrales, IReadOnlyList<Hallazgo> Hallazgos);
