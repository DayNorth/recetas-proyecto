namespace Recetas.Api.Aplicacion.Comun;

public record Paginado<T>(IReadOnlyList<T> Items, int Pagina, int Tamano, int Total);

public record SolicitudDePagina(int Pagina, int Tamano)
{
    public const int TAMANO_POR_DEFECTO = 12;
    public const int TAMANO_MAXIMO = 50;

    public int CuantosSaltar => (Pagina - 1) * Tamano;

    public static SolicitudDePagina Normalice(int? laPagina, int? elTamano) =>
        new(Math.Max(laPagina ?? 1, 1), Math.Clamp(elTamano ?? TAMANO_POR_DEFECTO, 1, TAMANO_MAXIMO));
}
