using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;

namespace Recetas.Api.Infraestructura.Reportes;

public class ExportadorDeReportes : IExportadorDeReportes
{
    static ExportadorDeReportes() => QuestPDF.Settings.License = LicenseType.Community;

    public const string TIPO_DE_CONTENIDO_CSV = "text/csv; charset=utf-8";
    public const string TIPO_DE_CONTENIDO_XLSX = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const string TIPO_DE_CONTENIDO_PDF = "application/pdf";

    public ContenidoExportado Exporte(ResultadoReporte elResultado, string elFormato, DateTimeOffset laFechaDeGeneracion) =>
        elFormato switch
        {
            "csv" => new ContenidoExportado(GenereElCsv(elResultado), TIPO_DE_CONTENIDO_CSV),
            "xlsx" => new ContenidoExportado(GenereElXlsx(elResultado), TIPO_DE_CONTENIDO_XLSX),
            "pdf" => new ContenidoExportado(GenereElPdf(elResultado, laFechaDeGeneracion), TIPO_DE_CONTENIDO_PDF),
            _ => throw new ArgumentOutOfRangeException(nameof(elFormato), elFormato, "Formato no soportado.")
        };

    public static string FormateeLaCelda(object? elValor) => elValor switch
    {
        null => "",
        decimal elDecimal => elDecimal.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ','),
        double elDoble => elDoble.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ','),
        IFormattable elFormateable => elFormateable.ToString(null, CultureInfo.InvariantCulture),
        _ => elValor.ToString() ?? ""
    };

    private static string FormateeElCampoCsv(string elTexto) =>
        elTexto.IndexOfAny([';', '"', '\r', '\n']) >= 0 || elTexto != elTexto.Trim()
            ? "\"" + elTexto.Replace("\"", "\"\"") + "\""
            : elTexto;

    private static byte[] GenereElCsv(ResultadoReporte elResultado)
    {
        var elTexto = new StringBuilder();
        elTexto.Append(string.Join(';', elResultado.Columnas.Select(columna => FormateeElCampoCsv(columna.Titulo)))).Append("\r\n");
        foreach (var laFila in elResultado.Filas)
            elTexto.Append(string.Join(';', elResultado.Columnas.Select(columna => FormateeElCampoCsv(FormateeLaCelda(laFila[columna.Campo]))))).Append("\r\n");
        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetPreamble().Concat(Encoding.UTF8.GetBytes(elTexto.ToString())).ToArray();
    }

    private static byte[] GenereElXlsx(ResultadoReporte elResultado)
    {
        using var elLibro = new XLWorkbook();
        var laHoja = elLibro.Worksheets.Add(elResultado.Titulo.Length > 31 ? elResultado.Titulo[..31] : elResultado.Titulo);
        for (var columna = 0; columna < elResultado.Columnas.Count; columna++)
        {
            var laCelda = laHoja.Cell(1, columna + 1);
            laCelda.Value = elResultado.Columnas[columna].Titulo;
            laCelda.Style.Font.Bold = true;
            laCelda.Style.Fill.BackgroundColor = XLColor.LightGray;
        }
        for (var fila = 0; fila < elResultado.Filas.Count; fila++)
            for (var columna = 0; columna < elResultado.Columnas.Count; columna++)
            {
                var laCelda = laHoja.Cell(fila + 2, columna + 1);
                switch (elResultado.Filas[fila][elResultado.Columnas[columna].Campo])
                {
                    case null: break;
                    case decimal elDecimal: laCelda.Value = elDecimal; break;
                    case long elLargo: laCelda.Value = elLargo; break;
                    case int elEntero: laCelda.Value = elEntero; break;
                    case double elDoble: laCelda.Value = elDoble; break;
                    case var otro: laCelda.Value = otro.ToString(); break;
                }
            }
        laHoja.SheetView.FreezeRows(1);
        laHoja.Columns().AdjustToContents();

        var laHojaDeLectura = elLibro.Worksheets.Add("Lectura");
        laHojaDeLectura.Cell(1, 1).Value = "Reporte"; laHojaDeLectura.Cell(1, 2).Value = elResultado.Titulo;
        laHojaDeLectura.Cell(2, 1).Value = "Periodo"; laHojaDeLectura.Cell(2, 2).Value = $"{elResultado.Rango.Inicio:yyyy-MM-dd} a {elResultado.Rango.Fin:yyyy-MM-dd}";
        laHojaDeLectura.Cell(3, 1).Value = "Pregunta"; laHojaDeLectura.Cell(3, 2).Value = elResultado.Decision.Pregunta;
        laHojaDeLectura.Cell(4, 1).Value = "Cómo leerlo"; laHojaDeLectura.Cell(4, 2).Value = elResultado.Decision.ReglaLectura;
        laHojaDeLectura.Cell(5, 1).Value = "Acción sugerida"; laHojaDeLectura.Cell(5, 2).Value = elResultado.Decision.Accion;
        laHojaDeLectura.Column(1).Style.Font.Bold = true;
        laHojaDeLectura.Columns().AdjustToContents();

        using var laMemoria = new MemoryStream();
        elLibro.SaveAs(laMemoria);
        return laMemoria.ToArray();
    }

    private static byte[] GenereElPdf(ResultadoReporte elResultado, DateTimeOffset laFechaDeGeneracion)
    {
        return Document.Create(documento => documento.Page(pagina =>
        {
            pagina.Size(PageSizes.A4.Landscape());
            pagina.Margin(28);
            pagina.DefaultTextStyle(estilo => estilo.FontSize(10));

            pagina.Header().Column(columna =>
            {
                columna.Item().Text(elResultado.Titulo).FontSize(18).SemiBold();
                columna.Item().Text($"Periodo: {elResultado.Rango.Inicio:dd/MM/yyyy} a {elResultado.Rango.Fin:dd/MM/yyyy}").FontColor(Colors.Grey.Darken2);
            });

            pagina.Content().PaddingVertical(10).Column(columna =>
            {
                columna.Spacing(10);
                columna.Item().Text(texto =>
                {
                    texto.Span("Pregunta: ").SemiBold(); texto.Line(elResultado.Decision.Pregunta);
                    texto.Span("Cómo leerlo: ").SemiBold(); texto.Line(elResultado.Decision.ReglaLectura);
                    texto.Span("Acción sugerida: ").SemiBold(); texto.Line(elResultado.Decision.Accion);
                });

                if (elResultado.Filas.Count == 0)
                {
                    columna.Item().Text("No hay datos en este periodo.").Italic();
                    return;
                }
                columna.Item().Table(tabla =>
                {
                    tabla.ColumnsDefinition(definicion => { foreach (var _ in elResultado.Columnas) definicion.RelativeColumn(); });
                    tabla.Header(encabezado =>
                    {
                        foreach (var laColumna in elResultado.Columnas)
                            encabezado.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(laColumna.Titulo).SemiBold();
                    });
                    foreach (var laFila in elResultado.Filas)
                        foreach (var laColumna in elResultado.Columnas)
                            tabla.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(FormateeLaCelda(laFila[laColumna.Campo]));
                });
            });

            pagina.Footer().Row(fila =>
            {
                fila.RelativeItem().Text($"Generado el {laFechaDeGeneracion:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Darken1);
                fila.RelativeItem().AlignRight().Text(texto => { texto.Span("Página ").FontSize(8); texto.CurrentPageNumber().FontSize(8); });
            });
        })).GeneratePdf();
    }
}
