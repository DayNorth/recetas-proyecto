namespace Recetas.Api.Dominio;

public static class ZonaHoraria
{
    public static readonly TimeSpan COSTA_RICA = TimeSpan.FromHours(-6);

    public static DateOnly ObtengaLaFechaDeHoy(TimeProvider elReloj) =>
        DateOnly.FromDateTime(elReloj.GetUtcNow().ToOffset(COSTA_RICA).DateTime);

    public static DateTime ConviertaElInicioDelDiaAUtc(DateOnly laFecha) =>
        new DateTimeOffset(laFecha.ToDateTime(TimeOnly.MinValue), COSTA_RICA).UtcDateTime;
}
