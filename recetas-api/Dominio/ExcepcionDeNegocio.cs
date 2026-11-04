namespace Recetas.Api.Dominio;

public enum TipoDeFalla
{
    SolicitudInvalida,
    RecursoInexistente,
    Conflicto,
    AccesoProhibido,
    FaltaDeAutenticacion,
    ServicioNoDisponible
}

public class ExcepcionDeNegocio(TipoDeFalla tipo, string titulo, string? detalle = null) : Exception(titulo)
{
    public TipoDeFalla Tipo { get; } = tipo;
    public string Titulo { get; } = titulo;
    public string? Detalle { get; } = detalle;

    public static ExcepcionDeNegocio PorSolicitudInvalida(string titulo, string? detalle = null) =>
        new(TipoDeFalla.SolicitudInvalida, titulo, detalle);

    public static ExcepcionDeNegocio PorRecursoInexistente(string titulo = "No se encontró el recurso.") =>
        new(TipoDeFalla.RecursoInexistente, titulo);

    public static ExcepcionDeNegocio PorConflicto(string titulo, string? detalle = null) =>
        new(TipoDeFalla.Conflicto, titulo, detalle);

    public static ExcepcionDeNegocio PorAccesoProhibido(string titulo = "No tiene permiso para esta acción.") =>
        new(TipoDeFalla.AccesoProhibido, titulo);

    public static ExcepcionDeNegocio PorFaltaDeAutenticacion(string titulo = "Credenciales inválidas.") =>
        new(TipoDeFalla.FaltaDeAutenticacion, titulo);

    public static ExcepcionDeNegocio PorServicioNoDisponible(string titulo) =>
        new(TipoDeFalla.ServicioNoDisponible, titulo);
}
