using System.Globalization;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Dominio;

namespace Recetas.Api.Aplicacion.Servicios;

public class CalculadoraDeRangos(TimeProvider elReloj)
{
    private const int MAXIMO_DE_MESES = 24;
    private const int MESES_POR_DEFECTO = 6;

    public DateOnly ObtengaLaFechaDeHoy() => ZonaHoraria.ObtengaLaFechaDeHoy(elReloj);

    public RangoReporte ResuelvaElRango(DateOnly? elInicio, DateOnly? elFin, string? elMesDeInicio, string? elMesDeFin)
    {
        var usaFechas = elInicio is not null || elFin is not null;
        var usaMeses = !string.IsNullOrWhiteSpace(elMesDeInicio) || !string.IsNullOrWhiteSpace(elMesDeFin);
        if (usaFechas && usaMeses)
            throw ExcepcionDeNegocio.PorSolicitudInvalida("Use inicio/fin o mesInicio/mesFin, no ambos.");

        DateOnly inicio, fin;
        if (usaMeses)
        {
            if (string.IsNullOrWhiteSpace(elMesDeInicio) || string.IsNullOrWhiteSpace(elMesDeFin))
                throw ExcepcionDeNegocio.PorSolicitudInvalida("Indique mesInicio y mesFin (AAAA-MM).");
            inicio = ConviertaElMes(elMesDeInicio, "mesInicio");
            fin = ConviertaElMes(elMesDeFin, "mesFin").AddMonths(1).AddDays(-1);
        }
        else if (usaFechas)
        {
            if (elInicio is null || elFin is null) throw ExcepcionDeNegocio.PorSolicitudInvalida("Indique inicio y fin.");
            inicio = elInicio.Value;
            fin = elFin.Value;
        }
        else
        {
            var hoy = ObtengaLaFechaDeHoy();
            var elPrimeroDeEsteMes = new DateOnly(hoy.Year, hoy.Month, 1);
            inicio = elPrimeroDeEsteMes.AddMonths(-MESES_POR_DEFECTO);
            fin = elPrimeroDeEsteMes.AddDays(-1);
        }

        if (inicio > fin) throw ExcepcionDeNegocio.PorSolicitudInvalida("El inicio no puede ser posterior al fin.");
        var cuantosMeses = (fin.Year * 12 + fin.Month) - (inicio.Year * 12 + inicio.Month) + 1;
        if (cuantosMeses > MAXIMO_DE_MESES) throw ExcepcionDeNegocio.PorSolicitudInvalida("El rango no puede superar 24 meses.");
        return new RangoReporte(inicio, fin);
    }

    private static DateOnly ConviertaElMes(string elTexto, string elNombreDelParametro) =>
        DateOnly.TryParseExact(elTexto.Trim() + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var laFecha)
            ? laFecha
            : throw ExcepcionDeNegocio.PorSolicitudInvalida($"{elNombreDelParametro} debe tener formato AAAA-MM.");
}
